# SIMS — Automated Testing Regime

This document describes the automated testing strategy for the Student Information Management
System (SIMS) and records the evidence produced by the most recent run.

---

## 1. How to run the tests

```bash
# Run everything
dotnet test DayNeCu3726.slnx

# Run one level only
dotnet test --filter "FullyQualifiedName~Tests.Unit"
dotnet test --filter "FullyQualifiedName~Tests.Integration"
dotnet test --filter "FullyQualifiedName~Tests.EndToEnd"

# Run with a results file and code coverage
dotnet test DayNeCu3726.slnx \
  --logger "trx;LogFileName=test-results.trx" \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults
```

The same command runs automatically on every push through
[`.github/workflows/ci.yml`](.github/workflows/ci.yml).

---

## 2. Latest results

| Metric | Value |
|---|---|
| Total tests | **212** |
| Passed | **212** |
| Failed | **0** |
| Skipped | 0 |
| Duration | ~16 s |

### Tests by level

| Level | Count | Purpose |
|---|---:|---|
| Unit | 138 | One class in isolation, no I/O |
| Integration | 59 | Several layers together against a real database context |
| End-to-end | 15 | Full HTTP journeys through the running application |

### Tests by class

| Test class | Level | Tests |
|---|---|---:|
| `PatternsTests` | Unit | 29 |
| `RowValidationTests` | Unit | 24 |
| `CsvLineParserTests` | Unit | 17 |
| `StreamingCsvTests` | Unit | 17 |
| `Pbkdf2PasswordHasherTests` | Unit | 15 |
| `AuthServiceTests` | Unit | 14 |
| `BatchImportProcessorTests` | Unit | 12 |
| `AuthorizationTests` | Unit | 10 |
| `DatasetServiceIntegrationTests` | Integration | 20 |
| `AcademicAverageTests` | Integration | 7 |
| `RepositoryIntegrationTests` | Integration | 17 |
| `StudentServiceIntegrationTests` | Integration | 15 |
| `WebApplicationEndToEndTests` | End-to-end | 15 |

---

## 3. The three levels of testing

### 3.1 Unit testing

Each test exercises a **single class with every dependency replaced by a test double**. No
database, file system or network is involved, so a unit test runs in microseconds and its failure
points at exactly one class.

Example — the RFC 4180 parser:

```csharp
[Fact]
public void Split_QuotedFieldContainingComma_KeepsFieldIntact()
{
    var fields = CsvLineParser.Split("BH00001,\"Nguyen Van A, Jr.\",a@sims.edu");

    Assert.Equal(3, fields.Count);
    Assert.Equal("Nguyen Van A, Jr.", fields[1]);
}
```

Unit testing is only possible because the application follows the **Dependency Inversion
Principle**: `AuthService` depends on `IPasswordHasher` and `IUnitOfWork`, never on
`Pbkdf2PasswordHasher` or `AppDbContext`, so both can be substituted in a test.

### 3.2 Integration testing

Integration tests combine **several real components** — service, repository, Unit of Work, entity
mapping and the EF Core provider — against an isolated in-memory database. They catch the class of
defect a unit test cannot see: broken queries, incorrect entity configuration and transaction
boundaries that only fail once the parts are assembled.

Example — the complete import pipeline:

```csharp
[Fact]
public async Task ImportStudentsAsync_InvalidRows_AreRejectedButValidRowsPersist()
{
    await using var stream = TestData.CsvStream(csvWithThreeBadRows);
    var result = await _service.ImportStudentsAsync(stream, ImportOptions.Default);

    Assert.Equal(2, result.SuccessCount);
    Assert.Equal(3, result.FailureCount);
    Assert.Equal(2, _unitOfWork.Students.Count());
}
```

### 3.3 End-to-end testing

End-to-end tests start the **real application in memory** with `WebApplicationFactory<Program>` and
drive it over HTTP. Routing, session state, the authorisation filter, model binding, the controllers,
the services and the Razor views all execute exactly as they do in deployment. Only the database
provider differs — configuration points it at a temporary SQLite file.

Example — role enforcement across the whole stack:

```csharp
[Fact]
public async Task AuthenticatedStudent_IsDeniedAccessToTheAdminDatasetPage()
{
    var client = _factory.CreateNonRedirectingClient();
    await LoginAsync(client, "student@sims.edu", "Student@123");

    var response = await client.GetAsync("/Dataset");

    Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    Assert.Contains("AccessDenied", response.Headers.Location?.ToString() ?? "");
}
```

### 3.4 Comparison

| Criterion | Unit | Integration | End-to-end |
|---|---|---|---|
| Scope | One class | Several layers | Whole application |
| Dependencies | All faked | Real database context | Everything real except the DB provider |
| Speed | Microseconds | Milliseconds | Hundreds of milliseconds |
| Count here | 138 | 52 | 15 |
| Failure diagnosis | Pinpoints one method | Points at a layer boundary | Says "the journey broke" |
| Catches | Logic errors | Mapping and query errors | Wiring, routing, security errors |
| Cost to maintain | Low | Medium | High (brittle to UI change) |

The distribution follows the **test pyramid**: many cheap unit tests, fewer integration tests and a
small number of expensive end-to-end tests covering only the critical journeys.

---

## 4. Code coverage

| Area | Line coverage |
|---|---:|
| `DataProcessing.Validation` | 98.8% |
| `DataProcessing.Pipeline` | 98.3% |
| `DataProcessing.Csv` | 98.1% |
| `DataProcessing.Mapping` | 100% |
| `Security` | 100% |
| `Infrastructure.Authorization` | 100% |
| `Infrastructure` (DbContext, seeder) | 97.4% |
| `Repositories` (Unit of Work) | 90.9% |
| `Repositories.EF` | 39.3% |
| `Patterns.Factory` | 66.2% |
| `Patterns.Strategy` | 57.4% |
| Controllers | 6.0% |
| **Overall** | **37.6%** |

Coverage is deliberately concentrated on the code where a defect would be most damaging: the
large-dataset processing pipeline, password hashing and access control are all at or near 100%.
The overall figure is held down by the legacy MVC controllers and Razor views, which are covered
only indirectly by the fifteen end-to-end tests. Raising controller coverage is the main outstanding
weakness of the current regime.

---

## 5. Developer-produced versus vendor-provided testing tools

The suite deliberately uses both, so the trade-off can be assessed from real experience.

### Vendor-provided

| Tool | Role |
|---|---|
| **xUnit.net** | Test framework: discovery, `[Fact]`/`[Theory]`, assertions, parallel execution |
| **Moq** | Dynamic mock objects generated at runtime from an interface |
| **Microsoft.AspNetCore.Mvc.Testing** | Hosts the real application in memory for end-to-end tests |
| **EF Core InMemory / SQLite** | Substitute database providers |
| **Coverlet** | Cross-platform code coverage collection |
| **GitHub Actions** | Continuous integration runner |

*Strengths:* mature, documented, maintained by others, integrate with the IDE and CI out of the box,
and give powerful features — `Times.Once` interaction verification, in-memory hosting — that would
take weeks to build.
*Weaknesses:* an external dependency to keep updated; a learning curve; occasional version conflicts
(this project hit exactly that — EF Core 9 and EF Core 10 assemblies clashed until the versions were
aligned); and licensing must be checked (FluentAssertions 8 was removed for this reason).

### Developer-produced

| Test double | Role |
|---|---|
| `FakePasswordHasher` | Hand-written `IPasswordHasher` stub |
| `FakeSession` | Minimal `ISession` implementation for filter tests |
| `TestData` | Fixture factory for contexts, entities and CSV streams |
| `RecordingProcessor` | Test-only subclass capturing persisted batches |

*Strengths:* no dependency, trivially readable, and tunable for speed — `FakePasswordHasher` avoids
the 120,000 PBKDF2 iterations that would otherwise dominate every service test.
*Weaknesses:* they are production code that must itself be maintained, they can silently drift from
the real interface, and they cannot verify interactions as expressively as a mocking library.

**Conclusion:** vendor tools were used for infrastructure and interaction verification; hand-written
doubles were used where a simple, fast, explicit stub was clearer than a configured mock.

---

## 6. Known limitations

1. **Controller coverage is low (6%).** Only paths reached by end-to-end tests are exercised.
   *Mitigation:* add focused controller tests using `Mock<IStudentService>`.
2. **`EF InMemory` is not a relational database.** It does not enforce foreign keys or unique
   constraints, so a test can pass where SQL Server would reject the data.
   *Mitigation:* the end-to-end tests use SQLite, which is relational; a future step is to run the
   integration suite against SQLite as well.
3. **No load or stress testing.** Throughput is characterised with 5,000- and 20,000-row datasets
   inside the functional tests, but there is no sustained concurrent-user benchmark.
4. **No UI/browser testing.** End-to-end tests assert on returned HTML, not on rendered behaviour;
   JavaScript is untested. Playwright or Selenium would close this gap.
5. **Timing assertions can be flaky** on a heavily loaded CI machine. The budgets are set
   deliberately loose (15 s for 20,000 rows) to avoid false failures.
