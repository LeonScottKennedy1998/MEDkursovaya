using Med.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http;

namespace Med.Controllers
{
    public class AccountAdminManager : Controller
    {
        private readonly ILogger<AccountAdminManager> _logger;
        private readonly AppDbContext _appDbContext;
        private readonly string _apiBaseUrl = "http://localhost:5072";
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountAdminManager(ILogger<AccountAdminManager> logger, AppDbContext appDbContext,  IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _appDbContext = appDbContext;
        }

        private async Task<HttpClient> GetAuthenticatedClient()
        {
            var token = Request.Cookies["jwt"];
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SignIn()
        {
            if (HttpContext.Session.Keys.Contains("AuthUser"))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(LoginModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient(); 
            var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/api/auth/login", model);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Ошибка входа");
                return View(model);
            }

            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (result != null)
            {
                Response.Cookies.Append("jwt", result.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                });

                await Authenticate(
                        result.User.UserID,   
                        result.User.Username, 
                        result.User.NameUser, 
                        result.User.Role
                    );

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Ошибка авторизации");
            return View(model);
        }



        private async Task Authenticate(int userId, string userName, string nameUser, string role)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),  
        new Claim(ClaimsIdentity.DefaultNameClaimType, userName), 
        new Claim("FullName", nameUser),                          
        new Claim(ClaimsIdentity.DefaultRoleClaimType, role)      
    };

            var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));

            _logger.LogInformation("Claims after authentication:");
            foreach (var claim in claims)
            {
                _logger.LogInformation($"Type: {claim.Type}, Value: {claim.Value}");
            }
        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear(); 
            Response.Cookies.Delete(".AspNetCore.Cookies");
            Response.Cookies.Delete("jwt");
            return RedirectToAction("SignIn");
        }


        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(RegisterModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = await GetAuthenticatedClient();
            var response = await client.PostAsJsonAsync(
                $"{_apiBaseUrl}/api/auth/register", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Регистрация прошла успешно!";
                return RedirectToAction("SignIn");
            }
            else
            {
                var errorObj = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                if (errorObj != null && errorObj.ContainsKey("Message"))
                {
                    ModelState.AddModelError("", errorObj["Message"]);
                }
                else
                {
                    ModelState.AddModelError("", "Ошибка при регистрации. Попробуйте позже.");
                }
                return View(model);
            }
        
    }
    }
}
