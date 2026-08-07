using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DayNeCu3726.Tests.EndToEnd
{
    /// <summary>
    /// Boots the real ASP.NET Core application in memory for end-to-end testing.
    /// <para>
    /// Everything the production application configures — routing, session, dependency injection,
    /// the authorisation filter, middleware and Razor views — is exercised exactly as deployed. The
    /// only difference is the database: the configurable provider is pointed at a temporary SQLite
    /// file, so the tests need no SQL Server instance and cannot touch real data.
    /// </para>
    /// <para>
    /// Being able to redirect the database purely through configuration, with no code change and no
    /// service-collection surgery, is a direct benefit of the Dependency Inversion Principle.
    /// </para>
    /// <para>
    /// This is the third level of the testing regime: unit tests verify single classes, integration
    /// tests verify collaborating layers, and these verify complete HTTP journeys.
    /// </para>
    /// </summary>
    public class SimsWebApplicationFactory : WebApplicationFactory<Program>, IDisposable
    {
        private readonly string _databasePath =
            Path.Combine(Path.GetTempPath(), $"sims-e2e-{Guid.NewGuid():N}.db");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            // Switch the application onto SQLite by configuration alone.
            builder.UseSetting("Database:Provider", "Sqlite");
            builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={_databasePath}");
        }

        /// <summary>
        /// Creates a client that keeps cookies and does not follow redirects, so a test can assert on
        /// the redirect itself — which is how the application signals "unauthenticated" and "forbidden".
        /// </summary>
        public HttpClient CreateNonRedirectingClient() =>
            CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true
            });

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing)
                return;

            // Remove the temporary database so repeated test runs never inherit stale data.
            TryDelete(_databasePath);
            TryDelete(_databasePath + "-shm");
            TryDelete(_databasePath + "-wal");
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (IOException)
            {
                // A locked file is harmless here; the operating system will reclaim the temp folder.
            }
        }
    }
}
