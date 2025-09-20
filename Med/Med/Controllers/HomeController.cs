using Med.Models;
using Med.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace Med.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private AppDbContext _appDbContext;
        private readonly IApiService _apiService;
        public HomeController(ILogger<HomeController> logger, AppDbContext appDbContext,IApiService apiService)
        {
            _logger = logger;
            _appDbContext = appDbContext;
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.Keys.Contains("AuthUser"))
            {
                var name = HttpContext.Session.GetString("Username");
                var roleName = HttpContext.Session.GetString("UserRole");

                var client = _apiService.CreateClient();
                try
                {
                    var roleResponse = await client.GetFromJsonAsync<List<Role>>($"{_apiService.BaseUrl}/api/roles/roles");

                    var role = roleResponse.FirstOrDefault(r => r.RoleName == roleName);
                    ViewBag.Role = role; 
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при получении ролей через API");
                    ViewBag.Role = null;
                }
            }

            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }


        public IActionResult Cart()
        {
            return RedirectToAction("Index", "Cart");
        }

        public async Task<IActionResult> Catalog(string? searchQuery, int? categoryId)
        {
            var handler = new HttpClientHandler { UseCookies = true };
            var client = await _apiService.CreateAuthenticatedClient(HttpContext);

            var url = $"{_apiService.BaseUrl}/api/catalog/catalog";
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(searchQuery))
                queryParams.Add($"searchQuery={Uri.EscapeDataString(searchQuery)}");
            if (categoryId.HasValue)
                queryParams.Add($"categoryId={categoryId.Value}");

            if (queryParams.Count > 0)
                url += "?" + string.Join("&", queryParams);

            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Ошибка при запросе к API каталога: {StatusCode}", response.StatusCode);
                var fallbackProducts = await _appDbContext.Products.Include(p => p.Category).ToListAsync();
                ViewBag.Categories = await _appDbContext.Categories.ToListAsync();
                return View(fallbackProducts);
            }

            var productsResponse = await response.Content.ReadFromJsonAsync<List<Product>>();

            ViewBag.Categories = await _appDbContext.Categories.ToListAsync();
            return View(productsResponse);
        }

        [Authorize]
        public async Task<IActionResult> Reviews(int productId)
        {
            var client = await _apiService.CreateAuthenticatedClient(HttpContext);

            var reviews = await client.GetFromJsonAsync<List<Review>>($"{_apiService.BaseUrl}/api/Reviews/product/{productId}");

            var product = await _appDbContext.Products
                .FirstOrDefaultAsync(p => p.ProductID == productId);

            if (product == null)
                return NotFound();

            int? userId = null;
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim != null)
                    userId = int.Parse(userIdClaim);
            }

            bool canReview = false;
            if (userId.HasValue)
            {
                canReview = await _appDbContext.Orders
                .Where(o => o.UserID == userId.Value)
                .Include(o => o.OrderDetails)
                .AnyAsync(o => o.OrderDetails.Any(od => od.ProductID == productId));

                _logger.LogInformation(
            "Проверка покупки: userId={UserId}, productId={ProductId}, canReview={CanReview}",
            userId, productId, canReview
        );

            }
            var userReview = await _appDbContext.Reviews
    .FirstOrDefaultAsync(r => r.ProductID == productId && r.UserID == userId);

            ViewBag.CanReview = canReview;
            ViewBag.ProductID = productId;
            ViewBag.Product = product;
            ViewBag.UserReview = userReview;


            return View(reviews);
        }





        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}
