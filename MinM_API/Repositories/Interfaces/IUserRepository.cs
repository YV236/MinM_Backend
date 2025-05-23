using MinM_API.Data;
using MinM_API.Models;
using System.Security.Claims;

namespace MinM_API.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User> FindUser(ClaimsPrincipal userToFind, DataContext context);
    }
}
