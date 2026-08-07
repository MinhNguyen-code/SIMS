# SIMS - Student Information Management System
The **Student Information Management System (SIMS)** is a modern web application built on **ASP.NET Core MVC** and **Microsoft SQL Server**, applying Object-Oriented Programming (OOP) principles and enterprise design patterns.

---

## 🌟 Key Features

### 1. Authentication & Role-Based Access Control
The system features strict Role-Based Access Control (RBAC) supporting three primary user roles:
* **Admin**:
  * Manage students, faculty members, and course offerings.
  * Assign faculty members to courses.
  * Access system-wide dashboard and analytics.
* **Faculty**:
  * Manage assigned courses and view class rosters.
  * Input grades (`Grade`, `LetterGrade`).
  * Take attendance and track student absenteeism (`Absences`).
* **Student**:
  * Register for and drop courses during registration periods.
  * View class timetables (Slot groups, Classrooms, Days).
  * Check grades, GPA, and academic status.

### 2. Student & Course Management
* Auto-generation of Student Codes (`StudentCode`) and Faculty Codes (`FacultyCode`).
* Slot group schedule management and classroom allocation.
* Capacity checking (`MaxEnrollment`) during course registration.

---

### 3. Large Dataset Processing (CSV)
The system treats CSV as a first-class bulk data format, at `/Dataset` (Admin and Faculty only):

* **Streaming import** — files are read row by row with constant memory usage, so a file of any
  size can be imported without exhausting server memory.
* **RFC 4180 compliant parsing** — quoted commas, escaped quotes and multi-line fields are handled
  correctly.
* **Batched persistence** — records are committed in configurable batches (default 1,000) instead of
  one database round-trip per row.
* **Per-row validation and reporting** — every rejected row is reported with its line number, column
  and reason, rather than being silently discarded.
* **Dry-run mode** — validate a file without writing anything to the database.
* **Streaming export** — the full student table is written straight to the HTTP response.
* **Single-pass analytics** — dataset-wide statistics computed without materialising the data.
* **Sample data generator** — produces synthetic datasets up to 500,000 records for benchmarking.

---

## 🏗️ Architecture & Design Patterns

The project incorporates established software design patterns across all three GoF categories:

### Creational
* **Factory (`UserFactory`, `GradeStrategyFactory`)**: Creates role-specific users and grading
  strategies, encapsulating the selection logic in one place.
* **Singleton (`SystemConfiguration`)**: Thread-safe single instance of application-wide settings.
* **Builder (`ImportOptionsBuilder`)**: Fluent, validated assembly of bulk-import configuration.

### Structural
* **Facade (`SIMSFacade`)**: Unified, simplified interface over the complex domain services.
* **Decorator (`AuditStudentServiceDecorator`)**: Adds audit logging around `IStudentService`
  without modifying it.
* **Adapter (`StudentCsvMapper`)**: Converts between flat CSV records and the domain model, keeping
  the two independent.

### Behavioural
* **Strategy (`IGradeStrategy`)**: Interchangeable grading schemes (BTEC / Letter / Numeric).
* **Observer (`EnrollmentEventPublisher`)**: Notifies audit and email subscribers of enrollment events.
* **Template Method (`BatchImportProcessor<T>`)**: Fixes the import algorithm skeleton while
  subclasses supply the entity-specific steps.
* **Chain of Responsibility (`RowValidationHandler`)**: Composable CSV validation rules.

### Data access
* **Repository & Unit of Work**: Decouples persistence (`IUnitOfWork`) from business logic and gives
  transactional consistency.
* **TPH (Table-Per-Hierarchy)**: Stores the `User` inheritance hierarchy in a single table.

---

## ✅ Automated Testing

**212 automated tests, all passing** — see [TESTING.md](TESTING.md) for the full regime.

| Level | Count |
|---|---:|
| Unit | 138 |
| Integration | 59 |
| End-to-end | 15 |

```bash
dotnet test DayNeCu3726.slnx
```

Tests run automatically on every push via GitHub Actions
([`.github/workflows/ci.yml`](.github/workflows/ci.yml)).

---

## 🛠️ Technology Stack

* **Backend**: C#, ASP.NET Core MVC (.NET 10)
* **Database**: Microsoft SQL Server or SQLite / Entity Framework Core 10 (Code First)
* **Frontend**: HTML5, CSS3, JavaScript, Bootstrap, Razor Views
* **Testing**: xUnit, Moq, EF Core InMemory, `WebApplicationFactory`, Coverlet
* **CI**: GitHub Actions
* **Security**: PBKDF2-HMAC-SHA256 password hashing with per-user salt

---

## 🚀 Getting Started

### Prerequisites
* [.NET SDK 10.0](https://dotnet.microsoft.com/download)
* Microsoft SQL Server (LocalDB / SQL Express / Standalone) — **optional**, SQLite works too
* SQL Server Management Studio (SSMS) (Recommended when using SQL Server)

### Installation & Execution

1. **Clone the repository**:
   ```bash
   git clone https://github.com/MinhNguyen-code/SIMS.git
   cd SIMS
   ```

2. **Configure Database Connection (`appsettings.json`)**:
   Open `DayNeCu3726/appsettings.json` and set your SQL Server connection string:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=DayNeCu3726Db;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;"
   }
   ```
   *(Update `(localdb)\\MSSQLLocalDB` with your SQL Server instance name if needed).*

   **Prefer SQLite? No SQL Server required.** The database provider is configurable — add
   `"Database": { "Provider": "Sqlite" }` and use a SQLite connection string:
   ```json
   "Database": { "Provider": "Sqlite" },
   "ConnectionStrings": { "DefaultConnection": "Data Source=sims.db" }
   ```

3. **Run the Application**:
   ```bash
   dotnet run --project DayNeCu3726
   ```
   *The database `DayNeCu3726Db` will be automatically created and populated with demo seed data upon the first run.*

4. **Run the tests**:
   ```bash
   dotnet test DayNeCu3726.slnx
   ```

5. **Try the bulk dataset features**: sign in as Admin, open **Bulk Dataset** in the sidebar,
   click **Generate** to produce a 50,000-record sample CSV, then upload it with **Run import**.

6. **Default Test Accounts**:
   * **Admin**: `admin@sims.edu` | Password: `Admin@123`
   * **Faculty (Term 5)**: `faculty5@sims.edu` | Password: `Faculty@123`
   * **Student (Minh)**: `minh@sims.edu` | Password: `Student@123`
   * **Student (Vinh)**: `vinh@sims.edu` | Password: `Student@123`

---

## 📝 License
Developed for academic and research purposes.
