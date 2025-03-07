using MinM_API.Repositories.Interfaces;
using System.Text.RegularExpressions;

namespace MinM_API.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
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
                    return false; // Якщо поле містить лише пробіли або порожнє, повертаємо false
                }
            }
            return true;
        }
    }
}
