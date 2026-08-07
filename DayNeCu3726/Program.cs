using Microsoft.EntityFrameworkCore;
using DayNeCu3726.DataProcessing.Abstractions;
using DayNeCu3726.DataProcessing.Csv;
using DayNeCu3726.Infrastructure;
using DayNeCu3726.Patterns.Decorator;
using DayNeCu3726.Patterns.Facade;
using DayNeCu3726.Patterns.Observer;
using DayNeCu3726.Patterns.Strategy;
using DayNeCu3726.Patterns.Singleton;
using DayNeCu3726.Repositories;
using DayNeCu3726.Repositories.Interfaces;
using DayNeCu3726.Security;
using DayNeCu3726.Services;
using DayNeCu3726.Services.Interfaces;

namespace DayNeCu3726
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ── MVC ────────────────────────────────────────────────────────
            builder.Services.AddControllersWithViews();

            // ── Session ────────────────────────────────────────────────────
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(2);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Name = ".SIMS.Session";
            });

            // ── Database & DbContext ───────────────────────────────────────────
            // The provider is chosen from configuration rather than hard-coded. Previously the code
            // called UseSqlServer unconditionally while the comments and the checked-in sims.db files
            // described SQLite, and the provider could not be swapped for automated testing.
            // Reading it from "Database:Provider" keeps SQL Server as the default for deployment while
            // letting a developer or a test run against SQLite with no code change (Open/Closed).
            ConfigureDatabase(builder);

            // ── Repository / Unit of Work (Scoped for DB transaction consistency) ──
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // ── Security ─────────────────────────────────────────────────────────
            // Registered against the IPasswordHasher abstraction (DIP): swapping PBKDF2 for a
            // different algorithm later is a one-line change here, invisible to every consumer.
            builder.Services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

            // ── CSV data-processing engine ───────────────────────────────────────
            // Stateless and thread-safe, so a single shared instance is sufficient.
            builder.Services.AddSingleton<ICsvRecordReader>(_ => new StreamingCsvRecordReader());
            builder.Services.AddSingleton<ICsvRecordWriter>(_ => new StreamingCsvRecordWriter());

            // ── Observer pattern ─────────────────────────────────────────────────
            // The publisher and its subscribers are now configured centrally instead of being
            // constructed inside EnrollmentService, so observers can be added or removed here alone.
            builder.Services.AddScoped(_ =>
            {
                var publisher = new EnrollmentEventPublisher();
                publisher.Subscribe(new EmailNotificationObserver());
                publisher.Subscribe(new AuditLogObserver());
                return publisher;
            });

            // ── Strategy pattern ─────────────────────────────────────────────────
            builder.Services.AddScoped<IGradeStrategy>(_ =>
                GradeStrategyFactory.Create(SystemConfiguration.Instance.GradingScheme));

            // ── Services (Scoped for DbContext injection safety) ─────────────────
            builder.Services.AddScoped<IAuthService, AuthService>();

            // Register StudentService wrapped with AuditStudentServiceDecorator (Decorator Pattern)
            builder.Services.AddScoped<IStudentService>(sp =>
            {
                var uow = sp.GetRequiredService<IUnitOfWork>();
                var hasher = sp.GetRequiredService<IPasswordHasher>();
                var baseService = new StudentService(uow, hasher);
                return new AuditStudentServiceDecorator(baseService);  // Decorator wraps base service
            });

            // Large-dataset CSV import / export / analysis.
            builder.Services.AddScoped<IDatasetService, DatasetService>();

            builder.Services.AddScoped<ICourseService, CourseService>();
            builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
            builder.Services.AddScoped<IFacultyService, FacultyService>();
            builder.Services.AddScoped<IServiceRequestService, ServiceRequestService>();
            builder.Services.AddScoped<IFinanceService, FinanceService>();
            builder.Services.AddScoped<IFeedbackService, FeedbackService>();
            builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
            builder.Services.AddScoped<IReportService, ReportService>();
            builder.Services.AddScoped<IExamService, ExamService>();
            builder.Services.AddScoped<IAssignmentService, AssignmentService>();

            // ── Facade Pattern ─────────────────────────────────────────────
            builder.Services.AddScoped<SIMSFacade>();

            var app = builder.Build();

            // ── Seed Data & Ensure DB Created ──────────────────────────────
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<AppDbContext>();
                context.Database.EnsureCreated(); // Creates SQLite database & tables automatically
                
                var uow = services.GetRequiredService<IUnitOfWork>();
                DataSeeder.Seed(uow);
            }

            // ── HTTP Pipeline ──────────────────────────────────────────────
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
                app.UseHttpsRedirection();  // Only redirect to HTTPS in production
            }

            // Baseline security response headers, addressing the Security non-functional requirement.
            app.Use(async (context, next) =>
            {
                var headers = context.Response.Headers;
                headers["X-Content-Type-Options"] = "nosniff";
                headers["X-Frame-Options"] = "SAMEORIGIN";
                headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                await next();
            });

            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();               // Session must come after UseRouting
            app.UseAuthorization();

            // Default route: controller default=Home, action default=Index
            // HomeController.Index() handles the / redirect to Login or Dashboard
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }

        /// <summary>
        /// Registers <see cref="AppDbContext"/> with the provider named in configuration.
        /// Supported values for <c>Database:Provider</c> are <c>SqlServer</c> (default) and <c>Sqlite</c>.
        /// </summary>
        private static void ConfigureDatabase(WebApplicationBuilder builder)
        {
            var provider = builder.Configuration["Database:Provider"] ?? "SqlServer";
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
                {
                    options.UseSqlite(connectionString ?? "Data Source=sims.db");
                }
                else
                {
                    options.UseSqlServer(connectionString
                        ?? "Server=(localdb)\\MSSQLLocalDB;Database=DayNeCu3726Db;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;");
                }
            });
        }
    }
}
