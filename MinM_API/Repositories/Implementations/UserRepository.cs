using Microsoft.EntityFrameworkCore;
using MinM_API.Data;
using MinM_API.Models;
using MinM_API.Repositories.Interfaces;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace MinM_API.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        public async Task<User> FindUser(ClaimsPrincipal userToFind, DataContext context)
        {
            var id = GetNameIdentifier(userToFind);

            return await context.Users.Include(u => u.Address).FirstOrDefaultAsync(u => u.Id.ToString() == id);
        }

        public bool IsValidEmail(string email)
        {
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }

        public bool AreAllFieldsFilled<T>(T user) where T : class
        {
            var properties = user.GetType().GetProperties();

            foreach (var property in properties)
            {
                var value = property.GetValue(user) as string;

                if (value is null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(value))
                {
                    return false;
                }
            }
            return true;
        }

        private string GetNameIdentifier(ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        }
    }
}
