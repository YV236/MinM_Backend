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
            var id = userToFind.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;

            return await context.Users.Include(u => u.Address).FirstOrDefaultAsync(u => u.Id.ToString() == id);
        }
    }
}
