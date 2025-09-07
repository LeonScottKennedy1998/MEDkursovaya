using Microsoft.AspNetCore.Mvc;
using Med.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using System.Text;

namespace Med.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OrderController> _logger;
        private readonly string _apiBaseUrl = "http://localhost:5072";

        public OrderController(
            IHttpClientFactory httpClientFactory,
            ILogger<OrderController> logger)
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

            try
            {
                var client = await GetAuthenticatedClient();
                var response = await client.GetAsync($"{_apiBaseUrl}/api/orders");

                if (!response.IsSuccessStatusCode)
                    return HandleError(response);

                var content = await response.Content.ReadAsStringAsync();
                var orders = JsonSerializer.Deserialize<List<Order>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching orders");
                TempData["ErrorMessage"] = "Ошибка сервера";
                return View(new List<Order>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder()
        {
            var cookieCart = Request.Cookies["cart"];
            if (string.IsNullOrEmpty(cookieCart))
            {
                return RedirectToAction("Index", "Cart");
            }

            try
            {
                var cart = JsonSerializer.Deserialize<List<CartItem>>(cookieCart);
                var requestData = cart.Select(item => new
                {
                    ProductID = item.ProductID,
                    Quantity = item.Quantity
                }).ToList();

                var client = await GetAuthenticatedClient();
                var content = new StringContent(
                    JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync($"{_apiBaseUrl}/api/orders", content);

                if (!response.IsSuccessStatusCode)
                {
                    return HandleError(response);

                }

                Response.Cookies.Delete("cart");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                TempData["ErrorMessage"] = "Ошибка сервера";
                return RedirectToAction("Index", "Cart");
            }
        }
        private IActionResult HandleError(HttpResponseMessage response)
        {
            var statusCode = (int)response.StatusCode;
            TempData["ErrorMessage"] = statusCode switch
            {
                400 => "Недостаточно товара на складе",
                401 => "Требуется авторизация",
                404 => "Ресурс не найден",
                _ => "Произошла ошибка"
            };
            return RedirectToAction("Index", "Cart");
        }
    }
}
