using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace KtTest.Services
{
    public class UserContext : IUserContext
    {
        public int UserId { get; }
        public bool IsTeacher { get; }
        public bool IsOwner { get; }

        public UserContext(IHttpContextAccessor contextAccessor)
        {
            UserId = GetAuthenticatedUserId(contextAccessor.HttpContext);
            IsTeacher = IsAuthenticatedUserTeacher(contextAccessor.HttpContext);
            IsOwner = IsOrganizationOwner(contextAccessor.HttpContext);
        }

        public static int GetAuthenticatedUserId(HttpContext httpContext)
        {
            int result = -1;

            if (httpContext?.User?.Identity is ClaimsIdentity identity)
            {
                string nameIdentifierValue = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int.TryParse(nameIdentifierValue, out result);
            }

            return result;
        }

        public static bool IsAuthenticatedUserTeacher(HttpContext httpContext)
        {
            return HasClaim(httpContext, "Employee");
        }

        public static bool IsOrganizationOwner(HttpContext httpContext)
        {
            return HasClaim(httpContext, "Owner");
        }

        private static bool HasClaim(HttpContext httpContext, string claimName)
        {
            if (httpContext?.User?.Identity is ClaimsIdentity identity)
            {
                var claim = identity.FindFirst(claimName);
                if (claim != null)
                    return true;
            }

            return false;
        }
    }
}
