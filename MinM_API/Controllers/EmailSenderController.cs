using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MinM_API.Extension;
using MinM_API.Models;
using MinM_API.Services.Implementations;
using MinM_API.Services.Interfaces;
using System.Net;

namespace MinM_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailSenderController(UserManager<User> userManager, IEmailService emailService, JwtTokenService tokenService) : ControllerBase
    {
        [HttpPost("request-confirmation-code")]
        public async Task<IActionResult> RequestConfirmationCode([FromBody] string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null) return NotFound("User not found");

            var code = LuhnCodeGenerator.Generate6DigitCode();
            var token = tokenService.CreateCodeToken(email, code, TimeSpan.FromMinutes(10));

            await emailService.SendEmailAsync(email, "Код підтвердження", $"Ваш код: {code}");

            return Ok(new { token });
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmWithTokenRequest request)
        {
            var (isValid, email, actualCode) = tokenService.ValidateCodeToken(request.Token);
            if (!isValid)
                return BadRequest("Invalid or expired token.");

            if (email != request.Email || actualCode != request.Code)
                return BadRequest("Invalid code or email.");

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            user.EmailConfirmed = true;
            await userManager.UpdateAsync(user);

            return Ok("Email confirmed successfully.");
        }

        public class ConfirmWithTokenRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Code { get; set; } = string.Empty;
            public string Token { get; set; } = string.Empty;
        }
    }
}
