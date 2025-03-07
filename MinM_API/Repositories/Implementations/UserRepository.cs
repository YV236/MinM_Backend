using MinM_API.Repositories.Interfaces;

namespace MinM_API.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        public bool AreAllFieldsFilled<T>(T user) where T : class
        {
            throw new NotImplementedException();
        }

        public bool IsValidEmail(string email)
        {
            throw new NotImplementedException();
        }
    }
}
