        using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FinancialSuite.Application.Users
{
    public interface IUserContext
    {
        CurrentUser? GetCurrentUser();
    }

    public class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
    {
        public CurrentUser? GetCurrentUser()
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                throw new InvalidOperationException("User Context not present");
            }

            if (user.Identity == null || !user.Identity.IsAuthenticated)
            {
                return null;
            }

            var userId = user.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)!.Value;
            var email = user.FindFirst(c => c.Type == ClaimTypes.Email)!.Value;
            var roles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);

            // Date Of Birth is stored as a string in the format "yyyy-MM-dd" in the claims
            var dobStr = user.FindFirst(c => c.Type == "DateOfBirth")?.Value;
            var dob = dobStr == null
                ? (DateOnly?)null
                : DateOnly.ParseExact(dobStr, "yyyy-MM-dd");

            return new CurrentUser(userId, email, roles.ToList(), dob);
        }
    }

}
