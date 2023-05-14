using Microsoft.AspNetCore.Identity;
using PojisteniApp2.Models;
using System.Data;
using System.Security.Claims;

namespace PojisteniApp2.Helpers
{
    public class UserHelper
    {
        public static bool IsAuthorOrAdmin(ClaimsPrincipal user, IAuthor model)
        {
            return IsAdmin(user) || IsAuthor(user, model);
        }

        public static bool IsAuthor(ClaimsPrincipal user, IAuthor model)
        {
            string? userId = GetUserId(user);
            return userId != null && model.AuthorId == userId;
        }

        public static bool IsAdmin(ClaimsPrincipal user)
        {
            return user.HasClaim(ClaimTypes.Role, "admin");
        }

        public static bool IsLoggedUser(ClaimsPrincipal user)
        {
            return user.Identity?.IsAuthenticated ?? false;
        }

        public static string? GetUserId(ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
