using DayNeCu3726.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DayNeCu3726.Infrastructure.Authorization
{
    /// <summary>
    /// Declarative role-based access control for controllers and actions.
    /// <para>
    /// Previously every action began with the same two lines:
    /// <code>
    /// if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");
    /// if (GetRole() != "Admin") return RedirectToAction("AccessDenied", "Auth");
    /// </code>
    /// repeated across fourteen controllers. That duplication is a clean-coding failure and a real
    /// security risk: forgetting the check on one new action silently exposes it. Centralising the
    /// rule in a filter means access control is enforced in exactly one place and declared with a
    /// single readable attribute.
    /// </para>
    /// <para>
    /// Single Responsibility Principle: controllers now handle only request coordination; the
    /// cross-cutting authorisation concern lives in this filter.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
    {
        internal const string UserIdSessionKey = "UserId";
        internal const string UserRoleSessionKey = "UserRole";

        private readonly UserRole[] _allowedRoles;

        /// <summary>
        /// Restricts access to the listed roles. When no role is supplied, any authenticated user
        /// is allowed — the attribute then acts as a plain "must be logged in" gate.
        /// </summary>
        public AuthorizeRoleAttribute(params UserRole[] allowedRoles)
        {
            _allowedRoles = allowedRoles ?? Array.Empty<UserRole>();
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var session = context.HttpContext.Session;

            var userId = session.GetString(UserIdSessionKey);
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            if (_allowedRoles.Length == 0)
                return;     // Authenticated is sufficient for this action.

            var roleName = session.GetString(UserRoleSessionKey)?.Trim();
            if (!Enum.TryParse<UserRole>(roleName, ignoreCase: true, out var currentRole) ||
                !Enum.IsDefined(currentRole) ||
                !_allowedRoles.Contains(currentRole))
            {
                // Fail closed: an unrecognised or unlisted role is denied rather than allowed.
                context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
            }
        }
    }
}
