using System.ComponentModel.DataAnnotations;

namespace MinM_API.Dtos.RefreshToken
{
    public class RefreshTokenRequest
    {
        public string AccessToken { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;
    }
}
