using System.Net;
using System.Text.RegularExpressions;

namespace DayNeCu3726.Tests.EndToEnd
{
    /// <summary>
    /// End-to-end tests driving the application over real HTTP requests.
    /// <para>
    /// These cover the journeys a marker or user would perform manually — signing in, being denied
    /// access, downloading an export — and prove that routing, session state, the authorisation
    /// filter, the controllers, the services and the CSV engine all work together.
    /// </para>
    /// </summary>
    public class WebApplicationEndToEndTests : IClassFixture<SimsWebApplicationFactory>
    {
        private readonly SimsWebApplicationFactory _factory;

        public WebApplicationEndToEndTests(SimsWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task LoginPage_IsReachableAnonymously()
        {
            var client = _factory.CreateNonRedirectingClient();

            var response = await client.GetAsync("/Auth/Login");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("text/html", response.Content.Headers.ContentType?.MediaType ?? string.Empty);
        }

        /// <summary>
        /// The single most important security assertion: a protected page must never be served to an
        /// anonymous visitor. This exercises the real authorisation filter inside the real pipeline.
        /// </summary>
        [Theory]
        [InlineData("/Dataset")]
        [InlineData("/Dataset/Export")]
        [InlineData("/Dataset/Template")]
        public async Task ProtectedPages_RedirectAnonymousVisitorsToLogin(string url)
        {
            var client = _factory.CreateNonRedirectingClient();

            var response = await client.GetAsync(url);

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/Auth/Login", response.Headers.Location?.ToString() ?? string.Empty);
        }

        [Fact]
        public async Task SecurityHeaders_ArePresentOnEveryResponse()
        {
            var client = _factory.CreateNonRedirectingClient();

            var response = await client.GetAsync("/Auth/Login");

            Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
            Assert.Equal("SAMEORIGIN", response.Headers.GetValues("X-Frame-Options").Single());
        }

        [Fact]
        public async Task Login_WithSeededAdminCredentials_EstablishesASession()
        {
            var client = _factory.CreateNonRedirectingClient();

            var response = await LoginAsync(client, "admin@sims.edu", "Admin@123");

            // A successful login redirects away from the login page rather than re-rendering it.
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.DoesNotContain("/Auth/Login", response.Headers.Location?.ToString() ?? string.Empty);
        }

        [Fact]
        public async Task Login_WithWrongPassword_StaysOnTheLoginPage()
        {
            var client = _factory.CreateNonRedirectingClient();

            var response = await LoginAsync(client, "admin@sims.edu", "TotallyWrong123!");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);   // Re-rendered with an error.
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("Login", body, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Full journey: authenticate as an administrator, then download the streaming CSV export and
        /// confirm the response really is CSV with the expected header row.
        /// </summary>
        [Fact]
        public async Task AuthenticatedAdmin_CanDownloadTheStudentCsvExport()
        {
            var client = _factory.CreateNonRedirectingClient();
            await LoginAsync(client, "admin@sims.edu", "Admin@123");

            var response = await client.GetAsync("/Dataset/Export");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);

            var csv = await response.Content.ReadAsStringAsync();
            Assert.StartsWith("StudentCode,FullName,Email", csv);
        }

        [Fact]
        public async Task AuthenticatedAdmin_CanDownloadTheImportTemplate()
        {
            var client = _factory.CreateNonRedirectingClient();
            await LoginAsync(client, "admin@sims.edu", "Admin@123");

            var response = await client.GetAsync("/Dataset/Template");
            var csv = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("FullName", csv);
            Assert.Contains("Email", csv);
            Assert.Contains("Program", csv);
        }

        /// <summary>
        /// Generates a sample dataset through the real endpoint and verifies the row count, proving
        /// the streaming writer handles a realistic volume over HTTP.
        /// </summary>
        [Fact]
        public async Task AuthenticatedAdmin_CanGenerateALargeSampleDataset()
        {
            var client = _factory.CreateNonRedirectingClient();
            await LoginAsync(client, "admin@sims.edu", "Admin@123");

            var response = await client.GetAsync("/Dataset/GenerateSample?recordCount=5000");
            var csv = await response.Content.ReadAsStringAsync();

            var lineCount = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(5_001, lineCount);     // header + 5,000 records
        }

        [Fact]
        public async Task AuthenticatedAdmin_CanOpenTheBulkDatasetPage()
        {
            var client = _factory.CreateNonRedirectingClient();
            await LoginAsync(client, "admin@sims.edu", "Admin@123");

            var response = await client.GetAsync("/Dataset");
            var html = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Bulk Dataset Processing", html);
        }

        /// <summary>
        /// Role enforcement across the whole stack: a student is authenticated but must still be
        /// refused access to an administrative page.
        /// </summary>
        [Fact]
        public async Task AuthenticatedStudent_IsDeniedAccessToTheAdminDatasetPage()
        {
            var client = _factory.CreateNonRedirectingClient();
            await LoginAsync(client, "minh@sims.edu", "Student@123");

            var response = await client.GetAsync("/Dataset");

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("AccessDenied", response.Headers.Location?.ToString() ?? string.Empty);
        }

        /// <summary>
        /// Posts a CSV file through the real upload endpoint and asserts the resulting report is
        /// rendered — the complete import journey exactly as a user performs it.
        /// </summary>
        [Fact]
        public async Task AuthenticatedAdmin_CanImportACsvFileThroughTheUploadForm()
        {
            var client = _factory.CreateNonRedirectingClient();
            await LoginAsync(client, "admin@sims.edu", "Admin@123");

            var page = await client.GetAsync("/Dataset");
            var token = ExtractAntiForgeryToken(await page.Content.ReadAsStringAsync());

            const string csv = """
                FullName,Email,Program
                E2E Student One,e2e.one@sims.edu,Computer Science
                E2E Student Two,e2e.two@sims.edu,Data Science
                ,missing.name@sims.edu,Data Science
                """;

            using var form = new MultipartFormDataContent
            {
                { new StringContent(token), "__RequestVerificationToken" },
                { new StringContent("500"), "BatchSize" },
                { CreateCsvFileContent(csv), "file", "students.csv" }
            };

            var response = await client.PostAsync("/Dataset/Import", form);
            var html = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Import report", html);
            Assert.Contains("2 succeeded", html);       // Two valid rows imported...
            Assert.Contains("1 failed", html);          // ...and the blank name rejected.
        }

        [Fact]
        public async Task ImportEndpoint_WithoutAFile_ShowsAValidationMessage()
        {
            var client = _factory.CreateNonRedirectingClient();
            await LoginAsync(client, "admin@sims.edu", "Admin@123");

            var page = await client.GetAsync("/Dataset");
            var token = ExtractAntiForgeryToken(await page.Content.ReadAsStringAsync());

            using var form = new MultipartFormDataContent
            {
                { new StringContent(token), "__RequestVerificationToken" },
                { new StringContent("1000"), "BatchSize" }
            };

            var response = await client.PostAsync("/Dataset/Import", form);
            var html = await response.Content.ReadAsStringAsync();

            Assert.Contains("choose a CSV file", html, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Cross-site request forgery protection must reject a POST that carries no token, even from
        /// an authenticated session.
        /// </summary>
        [Fact]
        public async Task ImportEndpoint_WithoutAnAntiForgeryToken_IsRejected()
        {
            var client = _factory.CreateNonRedirectingClient();
            await LoginAsync(client, "admin@sims.edu", "Admin@123");

            using var form = new MultipartFormDataContent
            {
                { CreateCsvFileContent("FullName,Email,Program\nX,x@sims.edu,CS"), "file", "students.csv" }
            };

            var response = await client.PostAsync("/Dataset/Import", form);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static StringContent CreateCsvFileContent(string csv)
        {
            var content = new StringContent(csv);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
            return content;
        }

        private static async Task<HttpResponseMessage> LoginAsync(HttpClient client, string email, string password)
        {
            var page = await client.GetAsync("/Auth/Login");
            var token = ExtractAntiForgeryToken(await page.Content.ReadAsStringAsync());

            var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["Email"] = email,
                ["Password"] = password
            });

            return await client.PostAsync("/Auth/Login", form);
        }

        /// <summary>Pulls the anti-forgery token out of the rendered HTML so POSTs are accepted.</summary>
        private static string ExtractAntiForgeryToken(string html)
        {
            var match = Regex.Match(
                html,
                @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""",
                RegexOptions.IgnoreCase);

            return match.Success ? match.Groups[1].Value : string.Empty;
        }
    }
}
