using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KtTest.Services
{
    public class UserContext : IUserContext
    {
        public int UserId { get; }
        public bool IsTeacher { get; }

        public UserContext(IHttpContextAccessor contextAccessor)
        {
            UserId = GetAuthenticatedUserId(contextAccessor.HttpContext);
            IsTeacher = IsAuthenticatedUserTeacher(contextAccessor.HttpContext);
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
            if (httpContext?.User?.Identity is ClaimsIdentity identity)
            {
                var claim = identity.FindFirst("Employee");
                if (claim != null)
                    return true;
            }

            return false;
        }
    }
}
