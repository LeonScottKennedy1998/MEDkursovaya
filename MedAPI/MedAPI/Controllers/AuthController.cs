using MedAPI.Models;
using MedAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MedAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;

        public AuthController(AppDbContext context, IConfiguration config, EmailService emailService)
        {
            _context = context;
            _config = config;
            _emailService = emailService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _context.Users
        .FirstOrDefaultAsync(u => u.Username == model.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.PasswordUser, user.PasswordUser))
                return Unauthorized(new { Message = "Неверный логин или пароль" });


            var role = await _context.Roles
                .Where(r => r.RoleID == user.RoleID)
                .Select(r => r.RoleName)
                .FirstOrDefaultAsync();

            var token = GenerateJwtToken(user, role);
            return Ok(new
            {
                token,
                user = new
                {
                    user.UserID,
                    user.Username,
                    user.NameUser,
                    Role = role
                }
            });
        }

        private string GenerateJwtToken(User user, string role)
        {

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
            new Claim("FullName", user.NameUser),
            new Claim(ClaimTypes.Role, role)

        };


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                return BadRequest(new { Message = "Этот email уже используется." });

            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                return BadRequest(new { Message = "Этот логин уже используется." });

            var user = new User
            {
                NameUser = model.NameUser,
                Email = model.Email,
                Username = model.Username,
                PasswordUser = BCrypt.Net.BCrypt.HashPassword(model.PasswordUser),
                AddressUser = model.AddressUser,
                RoleID = 2
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Регистрация прошла успешно" });
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
                return BadRequest(new { Message = "Пользователь с таким email не найден." });

            var token = Guid.NewGuid().ToString();

            user.ResetToken = token;
            user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            await _context.SaveChangesAsync();

            var resetLink = $"{_config["AppSettings:FrontendUrl"]}/AccountAdminManager/ResetPassword?email={user.Email}&token={token}";

            var emailBody = $@"
        <h3>Сброс пароля</h3>
        <p>Для сброса пароля перейдите по ссылке:</p>
        <a href='{resetLink}'>Сбросить пароль</a>
        <p>Ссылка действительна 1 час.</p>";

            await _emailService.SendEmailAsync(user.Email, "Сброс пароля - Медятина", emailBody);

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email &&
                                          u.ResetToken == model.Token &&
                                          u.ResetTokenExpiry > DateTime.UtcNow);
            if (user == null)
                return BadRequest(new { Message = "Неверный или просроченный токен." });

            user.PasswordUser = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;
            await _context.SaveChangesAsync();

            return Ok();
        }


    }

}
