using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MinM_API.Models;
using MinM_API.Services.Interfaces;
using System.Net;

namespace MinM_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailSenderController(UserManager<User> userManager, IEmailService emailService) : ControllerBase
    {
        [HttpPost("request-confirmation-code")]
        public async Task<IActionResult> RequestConfirmationCode([FromBody] string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null) return NotFound("User not found");

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token); // ОБОВʼЯЗКОВО кодуємо для передачі в URL або JSON

            await emailService.SendEmailAsync(email, "Код підтвердження", $"Ваш код: {encodedToken}");

            return Ok("Код надіслано");
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
        {
            var user = await userManager.FindByIdAsync(request.UserId);
            if (user == null) return NotFound("User not found");

            var token = WebUtility.UrlDecode(request.Token); // ОБОВʼЯЗКОВО декодуємо

            var result = await userManager.ConfirmEmailAsync(user, token);
            return result.Succeeded
                ? Ok("Email confirmed successfully.")
                : BadRequest("Invalid or expired token.");
        }

        public class ConfirmEmailRequest
        {
            public string UserId { get; set; } = string.Empty;
            public string Token { get; set; } = string.Empty;
        }
    }

}
