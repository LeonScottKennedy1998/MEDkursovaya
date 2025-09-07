using Microsoft.AspNetCore.Mvc;
using Med.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;

namespace Med.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl = "http://localhost:5072";
        private readonly ILogger<CartController> _logger;

        public CartController(IHttpClientFactory httpClientFactory, ILogger<CartController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;

        }

        private async Task<HttpClient> GetAuthenticatedClient()
        {
            var token = Request.Cookies["jwt"];
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }
        public async Task<IActionResult> Index()
        {
            int? userId = null;
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim))
                    userId = int.Parse(userIdClaim);
            }

            _logger.LogInformation("Cart access: userId={userId}, IsAuthenticated={isAuth}", userId, User.Identity.IsAuthenticated);

            var cookieCart = Request.Cookies["cart"];
            var cart = string.IsNullOrEmpty(cookieCart)
                ? new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(cookieCart);

            if (cart.Count == 0)
                return View(cart);

            var client = await GetAuthenticatedClient();
            var requestCart = cart.Select(c => new
            {
                ProductID = c.ProductID,
                Quantity = c.Quantity
            }).ToList();

            var content = new StringContent(JsonSerializer.Serialize(requestCart), Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{_apiBaseUrl}/api/cart/items", content);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Не удалось получить информацию о товарах.";
                return View(new List<CartItem>());
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var enrichedCart = JsonSerializer.Deserialize<List<CartItem>>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return View(enrichedCart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            if (quantity < 1)
            {
                TempData["ErrorMessage"] = "Количество должно быть больше 0.";
                return RedirectToAction("Catalog", "Home");
            }

            var client = await GetAuthenticatedClient();

            var checkResponse = await client.GetAsync($"{_apiBaseUrl}/api/cart/check-stock/{productId}/{quantity}");
            if (!checkResponse.IsSuccessStatusCode)
            {
                if (checkResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    TempData["ErrorMessage"] = "Недостаточно товара на складе.";
                    return RedirectToAction("Catalog", "Home");
                }
                if (checkResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    TempData["ErrorMessage"] = "Товар не найден.";
                    return RedirectToAction("Catalog", "Home");
                }
                TempData["ErrorMessage"] = "Не удалось проверить наличие на складе.";
                return RedirectToAction("Catalog", "Home");
            }

            var cookieCart = Request.Cookies["cart"];
            var cart = string.IsNullOrEmpty(cookieCart)
                ? new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(cookieCart);

            var item = cart.FirstOrDefault(c => c.ProductID == productId);
            if (item == null)
            {
                cart.Add(new CartItem
                {
                    ProductID = productId,
                    Quantity = quantity
                });
            }
            else
            {

                var newQuantity = item.Quantity + quantity;
                var secondCheck = await client.GetAsync($"{_apiBaseUrl}/api/cart/check-stock/{productId}/{newQuantity}");
                if (!secondCheck.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Недостаточно товара на складе для указанного количества.";
                    return RedirectToAction("Catalog", "Home");
                }
                item.Quantity = newQuantity;
            }

            Response.Cookies.Append("cart", JsonSerializer.Serialize(cart),
                new Microsoft.AspNetCore.Http.CookieOptions
                {
                    Expires = System.DateTime.Now.AddDays(30),
                    IsEssential = true
                });

            return RedirectToAction("Index");
        }



        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cookieCart = Request.Cookies["cart"];
            if (!string.IsNullOrEmpty(cookieCart))
            {
                var cart = JsonSerializer.Deserialize<List<CartItem>>(cookieCart);
                cart = cart.Where(c => c.ProductID != productId).ToList();

                Response.Cookies.Append("cart", JsonSerializer.Serialize(cart),
                    new CookieOptions { Expires = DateTime.Now.AddDays(30), IsEssential = true });
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ClearCart()
        {
            Response.Cookies.Delete("cart");
            return RedirectToAction("Index");
        }
    }
}
