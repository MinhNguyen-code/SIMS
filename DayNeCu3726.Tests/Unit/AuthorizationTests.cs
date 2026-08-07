using DayNeCu3726.Infrastructure.Authorization;
using DayNeCu3726.Models.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System.Text;

namespace DayNeCu3726.Tests.Unit
{
    /// <summary>
    /// Unit tests for role-based access control.
    /// <para>
    /// Access control is security-critical and was previously duplicated across every controller,
    /// so it now has dedicated tests covering the allow, deny and unauthenticated paths.
    /// </para>
    /// </summary>
    public class AuthorizationTests
    {
        /// <summary>
        /// Minimal in-memory <see cref="ISession"/> — a developer-produced test double, since
        /// ASP.NET Core provides no lightweight fake session out of the box.
        /// </summary>
        private sealed class FakeSession : ISession
        {
            private readonly Dictionary<string, byte[]> _store = new();

            public bool IsAvailable => true;
            public string Id => "test-session";
            public IEnumerable<string> Keys => _store.Keys;

            public void Clear() => _store.Clear();
            public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
            public void Remove(string key) => _store.Remove(key);
            public void Set(string key, byte[] value) => _store[key] = value;
            public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value!);

            public void SetText(string key, string value) => Set(key, Encoding.UTF8.GetBytes(value));
        }

        private static AuthorizationFilterContext CreateContext(string? userId, string? role)
        {
            var session = new FakeSession();
            if (userId is not null) session.SetText("UserId", userId);
            if (role is not null) session.SetText("UserRole", role);

            var httpContext = new DefaultHttpContext { Session = session };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
        }

        [Fact]
        public void NotLoggedIn_IsRedirectedToLogin()
        {
            var attribute = new AuthorizeRoleAttribute(UserRole.Admin);
            var context = CreateContext(userId: null, role: null);

            attribute.OnAuthorization(context);

            var redirect = Assert.IsType<RedirectToActionResult>(context.Result);
            Assert.Equal("Login", redirect.ActionName);
            Assert.Equal("Auth", redirect.ControllerName);
        }

        [Fact]
        public void AllowedRole_IsPermitted()
        {
            var attribute = new AuthorizeRoleAttribute(UserRole.Admin, UserRole.Faculty);
            var context = CreateContext(userId: "user-1", role: "Faculty");

            attribute.OnAuthorization(context);

            Assert.Null(context.Result);    // Null result means the request continues to the action.
        }

        [Fact]
        public void DisallowedRole_IsRedirectedToAccessDenied()
        {
            var attribute = new AuthorizeRoleAttribute(UserRole.Admin);
            var context = CreateContext(userId: "user-1", role: "Student");

            attribute.OnAuthorization(context);

            var redirect = Assert.IsType<RedirectToActionResult>(context.Result);
            Assert.Equal("AccessDenied", redirect.ActionName);
        }

        [Fact]
        public void NoRolesSpecified_AnyAuthenticatedUserIsPermitted()
        {
            var attribute = new AuthorizeRoleAttribute();
            var context = CreateContext(userId: "user-1", role: "Student");

            attribute.OnAuthorization(context);

            Assert.Null(context.Result);
        }

        /// <summary>
        /// Fail-closed check: a session holding a role name the system does not recognise must be
        /// denied, not allowed through by default.
        /// </summary>
        [Theory]
        [InlineData("Hacker")]
        [InlineData("")]
        [InlineData("999")]
        [InlineData("Student")]
        public void UnrecognisedRoleValue_IsDenied(string role)
        {
            var attribute = new AuthorizeRoleAttribute(UserRole.Admin);
            var context = CreateContext(userId: "user-1", role: role);

            attribute.OnAuthorization(context);

            var redirect = Assert.IsType<RedirectToActionResult>(context.Result);
            Assert.Equal("AccessDenied", redirect.ActionName);
        }

        /// <summary>
        /// A numeric string must not be accepted as a role. <c>Enum.TryParse</c> happily converts any
        /// integer into an enum value, so the filter additionally checks the value is actually defined.
        /// </summary>
        [Fact]
        public void NumericRoleValueOutsideTheEnum_IsDenied()
        {
            var attribute = new AuthorizeRoleAttribute(UserRole.Admin);
            var context = CreateContext(userId: "user-1", role: "12345");

            attribute.OnAuthorization(context);

            Assert.IsType<RedirectToActionResult>(context.Result);
        }

        [Fact]
        public void RoleMatchingIsCaseInsensitive()
        {
            var attribute = new AuthorizeRoleAttribute(UserRole.Admin);
            var context = CreateContext(userId: "user-1", role: "admin");

            attribute.OnAuthorization(context);

            Assert.Null(context.Result);
        }
    }
}
