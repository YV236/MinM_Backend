namespace MinM_API.Repositories.Interfaces
{
    public interface IUserRepository
    {
        bool IsValidEmail(string email);
        bool AreAllFieldsFilled<T>(T user) where T : class;
    }
}
