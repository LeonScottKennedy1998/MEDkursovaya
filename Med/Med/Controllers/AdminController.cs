using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Med.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace Med.Controllers
{
    [Authorize(Roles = "Админ")]
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AdminController> _logger;
        private readonly string _apiBaseUrl = "http://localhost:5072";
        public AdminController(
            IHttpClientFactory httpClientFactory,
            ILogger<AdminController> logger,
            IConfiguration configuration)
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
        public async Task<IActionResult> AdminManage()
        {
            var client = await GetAuthenticatedClient();
            var response = await client.GetAsync($"{_apiBaseUrl}/api/admin/alldata");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error fetching admin data: {response.StatusCode}");
                return View("Error");
            }

            var data = await response.Content.ReadFromJsonAsync<AdminTables>();
            return View(data);
        }

        public async Task<IActionResult> AddUser()
        {
            var client = await GetAuthenticatedClient();
            var response = await client.GetAsync($"{_apiBaseUrl}/api/roles/roles");

            if (response.IsSuccessStatusCode)
            {
                ViewBag.Roles = await response.Content.ReadFromJsonAsync<List<Role>>();
            }
            else
            {
                ViewBag.Roles = new List<Role>();
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(User user)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await GetRolesFromApi();
                return View(user);
            }

            var client = await GetAuthenticatedClient();
            var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/api/users/users", user);

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", errorMessage);
                ViewBag.Roles = await GetRolesFromApi();
                return View(user);
            }

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Произошла ошибка. Пожалуйста, попробуйте позже.");
                ViewBag.Roles = await GetRolesFromApi();
                return View(user);
            }

            TempData["SuccessMessage"] = "Пользователь успешно добавлен!";
            return RedirectToAction("AdminManage");
        }

        public async Task<IActionResult> EditUser(int id)
        {
            var client = await GetAuthenticatedClient();
            var response = await client.GetAsync($"{_apiBaseUrl}/api/users/users/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }

            if (!response.IsSuccessStatusCode)
            {
                return View("Error");
            }

            var user = await response.Content.ReadFromJsonAsync<User>();
            ViewBag.Roles = await GetRolesFromApi();
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(User user)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await GetRolesFromApi();
                return View(user);
            }

            var client = await GetAuthenticatedClient();
            var response = await client.PutAsJsonAsync($"{_apiBaseUrl}/api/users/users/{user.UserID}", user);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Произошла ошибка. Пожалуйста, попробуйте позже.");
                ViewBag.Roles = await GetRolesFromApi();
                return View(user);
            }

            return RedirectToAction("AdminManage");
        }

        public async Task<IActionResult> DeleteUser(int id)
        {
            var client = await GetAuthenticatedClient();
            var response = await client.DeleteAsync($"{_apiBaseUrl}/api/users/users/{id}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error deleting user: {response.StatusCode}");
            }

            return RedirectToAction("AdminManage");
        }

        public async Task<IActionResult> AddProduct()
        {
            ViewBag.Categories = await GetCategoriesFromApi();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product, IFormFile ImageFile)
        {
            try
            {
                System.IO.File.AppendAllText("D:\\ProjectMPT\\EditProduct.log", $"[{DateTime.Now}] Entered method\n");

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await ImageFile.CopyToAsync(ms);

                    var content = new MultipartFormDataContent();
                    content.Add(new StreamContent(new MemoryStream(ms.ToArray())), "file", ImageFile.FileName);

                    var client = await GetAuthenticatedClient();
                    var response = await client.PostAsync($"{_apiBaseUrl}/api/media/upload", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var raw = await response.Content.ReadAsStringAsync();
                        System.IO.File.AppendAllText("D:\\ProjectMPT\\EditProduct.log", $"[{DateTime.Now}] Upload raw response: {raw}\n");

                        var result = await response.Content.ReadFromJsonAsync<UploadResponse>();
                        if (result != null)
                        {
                            product.ImageUrl = result.Url;
                        }
                    }
                    else
                    {
                        var errorRaw = await response.Content.ReadAsStringAsync();
                        System.IO.File.AppendAllText("D:\\ProjectMPT\\EditProduct.log",
                            $"[{DateTime.Now}] Upload failed: {response.StatusCode}, {response.ReasonPhrase}, Content={errorRaw}\n");
                    }
                }
                System.IO.File.AppendAllText("D:\\ProjectMPT\\EditProduct.log", $"[{DateTime.Now}] Before API call\n");
                var apiClient = await GetAuthenticatedClient();
                System.IO.File.AppendAllText(
   "D:\\ProjectMPT\\EditProduct.log",
   $"[{DateTime.Now}] Before PUT, ImageUrl={product.ImageUrl}\n"
);
                var productResponse = await apiClient.PostAsJsonAsync($"{_apiBaseUrl}/api/products/products", product);
                System.IO.File.AppendAllText("D:\\ProjectMPT\\EditProduct.log", $"[{DateTime.Now}] API call completed: {productResponse.StatusCode}\n");

                if (string.IsNullOrEmpty(product.ImageUrl))
                {
                    ModelState.AddModelError("ImageFile", "Изображение продукта обязательно.");
                    ViewBag.Categories = await GetCategoriesFromApi();
                    return View(product);
                }

                if (!productResponse.IsSuccessStatusCode)
                {
                    var errorContent = await productResponse.Content.ReadAsStringAsync();
                    System.IO.File.AppendAllText("D:\\ProjectMPT\\EditProduct.log", $"[{DateTime.Now}] API returned error: {errorContent}\n");

                    ModelState.AddModelError("", "Произошла ошибка. Пожалуйста, попробуйте позже.");
                    ViewBag.Categories = await GetCategoriesFromApi();
                    return View(product);
                }

                System.IO.File.AppendAllText("D:\\ProjectMPT\\EditProduct.log", $"[{DateTime.Now}] Product updated successfully\n");

                return RedirectToAction("AdminManage");
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(
                        "D:\\ProjectMPT\\EditProduct.log",
                        $"[{DateTime.Now}] Caught Exception: {ex}\nStackTrace: {ex.StackTrace}\n\n"
                    );
                return View(product);
            }
        }

        public async Task<IActionResult> EditProduct(int id)
        {
            var client = await GetAuthenticatedClient();
            var response = await client.GetAsync($"{_apiBaseUrl}/api/products/products/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }

            if (!response.IsSuccessStatusCode)
            {
                return View("Error");
            }

            var product = await response.Content.ReadFromJsonAsync<Product>();
            ViewBag.Categories = await GetCategoriesFromApi();
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(Product product, IFormFile ImageFile)
        {
            try
            {

                System.IO.File.AppendAllText("D:\\ProjectMPT\\EditProduct.log", $"[{DateTime.Now}] Entered method\n");

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await ImageFile.CopyToAsync(ms);

                    var content = new MultipartFormDataContent();
                    content.Add(new StreamContent(new MemoryStream(ms.ToArray())), "file", ImageFile.FileName);

                    var client = await GetAuthenticatedClient();
                    var response = await client.PostAsync($"{_apiBaseUrl}/api/media/upload", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var raw = await response.Content.ReadAsStringAsync();
                        System.IO.File.AppendAllText("D:\\ProjectMPT\\EditProduct.log", $"[{DateTime.Now}] Upload raw response: {raw}\n");

                        var result = await response.Content.ReadFromJsonAsync<UploadResponse>();
                        if (result != null)
                        {
                            product.ImageUrl = result.Url;
                        }
                    }
                    else
                    {
                        var errorRaw = await response.Content.ReadAsStringAsync();
                        System.IO.File.AppendAllText("D:\\ProjectMPT\\EditProduct.log",
                            $"[{DateTime.Now}] Upload failed: {response.StatusCode}, {response.ReasonPhrase}, Content={errorRaw}\n");
                    }
                }

                System.IO.File.AppendAllText("D:\\ProjectMPT\\EditProduct.log", $"[{DateTime.Now}] Before API call\n");
                var apiClient = await GetAuthenticatedClient();
                        System.IO.File.AppendAllText(
            "D:\\ProjectMPT\\EditProduct.log",
            $"[{DateTime.Now}] Before PUT, ImageUrl={product.ImageUrl}\n"
        );
                var productResponse = await apiClient.PutAsJsonAsync($"{_apiBaseUrl}/api/products/products/{product.ProductID}", product);
                System.IO.File.AppendAllText("D:\\ProjectMPT\\EditProduct.log", $"[{DateTime.Now}] API call completed: {productResponse.StatusCode}\n");

                if (!productResponse.IsSuccessStatusCode)
                {
                    var errorContent = await productResponse.Content.ReadAsStringAsync();
                    System.IO.File.AppendAllText("D:\\ProjectMPT\\EditProduct.log", $"[{DateTime.Now}] API returned error: {errorContent}\n");

                    ModelState.AddModelError("", "Произошла ошибка. Пожалуйста, попробуйте позже.");
                    ViewBag.Categories = await GetCategoriesFromApi();
                    return View(product);
                }

                System.IO.File.AppendAllText("D:\\ProjectMPT\\EditProduct.log", $"[{DateTime.Now}] Product updated successfully\n");
                return RedirectToAction("AdminManage");
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(
                    "D:\\ProjectMPT\\EditProduct.log",
                    $"[{DateTime.Now}] Caught Exception: {ex}\nStackTrace: {ex.StackTrace}\n\n"
                );
                return View(product);
            }
        }

        public async Task<IActionResult> DeleteProduct(int id)
        {
            var client = await GetAuthenticatedClient();
            var response = await client.DeleteAsync($"{_apiBaseUrl}/api/products/products/{id}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error deleting products: {response.StatusCode}");
            }

            return RedirectToAction("AdminManage");
        }


        public async Task<IActionResult> AddOrder()
        {
            ViewBag.Users = await GetUsersFromApi();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddOrder(Order order)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Users = await GetUsersFromApi();
                return View(order);
            }

            var client = await GetAuthenticatedClient();
            var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/api/admin/orders", order);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Произошла ошибка. Пожалуйста, попробуйте позже.");
                ViewBag.Users = await GetUsersFromApi();
                return View(order);
            }

            return RedirectToAction("AdminManage");
        }

        public async Task<IActionResult> EditOrder(int id)
        {
            var client = await GetAuthenticatedClient();
            var response = await client.GetAsync($"{_apiBaseUrl}/api/admin/orders/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }

            if (!response.IsSuccessStatusCode)
            {
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });

            }

            var order = await response.Content.ReadFromJsonAsync<Order>();
            if (order != null)
                order.OrderDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(order.OrderDate, "Russian Standard Time");

            ViewBag.Users = await GetUsersFromApi();
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> EditOrder(Order order)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Users = await GetUsersFromApi();
                return View(order);
            }
            var mskZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
            order.OrderDate = TimeZoneInfo.ConvertTime(order.OrderDate, mskZone, TimeZoneInfo.Utc);

            var client = await GetAuthenticatedClient();
            var response = await client.PutAsJsonAsync($"{_apiBaseUrl}/api/admin/orders/{order.OrderID}", order);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Произошла ошибка. Пожалуйста, попробуйте позже.");
                ViewBag.Users = await GetUsersFromApi();
                return View(order);
            }

            return RedirectToAction("AdminManage");
        }

        public async Task<IActionResult> DeleteOrder(int id)
        {
            var client = await GetAuthenticatedClient();
            var response = await client.DeleteAsync($"{_apiBaseUrl}/api/admin/orders/{id}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error deleting orders: {response.StatusCode}");
            }

            return RedirectToAction("AdminManage");
        }

        public async Task<IActionResult> AddOrderDetails()
        {
            ViewBag.Products = await GetProductsFromApi();
            ViewBag.Orders = await GetOrdersFromApi();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddOrderDetails(OrderDetail orderDetails)
        {
            ModelState.Remove("ProductName");

            if (!ModelState.IsValid)
            {
                ViewBag.Products = await GetProductsFromApi();
                ViewBag.Orders = await GetOrdersFromApi();
                return View(orderDetails);
            }

            var client = await GetAuthenticatedClient();
            var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/api/orderdetails/orderdetails", orderDetails);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Произошла ошибка. Пожалуйста, попробуйте позже.");
                ViewBag.Products = await GetProductsFromApi();
                ViewBag.Orders = await GetOrdersFromApi();
                return View(orderDetails);
            }

            return RedirectToAction("AdminManage");
        }

        public async Task<IActionResult> EditOrderDetails(int id)
        {
            var client = await GetAuthenticatedClient();
            var response = await client.GetAsync($"{_apiBaseUrl}/api/orderdetails/orderdetails/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }

            if (!response.IsSuccessStatusCode)
            {
                return View("Error");
            }

            var orderdetail = await response.Content.ReadFromJsonAsync<OrderDetail>();
            ViewBag.Products = await GetProductsFromApi();
            ViewBag.Orders = await GetOrdersFromApi();
            return View(orderdetail);
        }

        [HttpPost]
        public async Task<IActionResult> EditOrderDetails(OrderDetail orderdetail)
        {
            ModelState.Remove("ProductName");

            if (!ModelState.IsValid)
            {
                ViewBag.Products = await GetProductsFromApi();
                ViewBag.Orders = await GetOrdersFromApi();
                return View(orderdetail);
            }

            var client = await GetAuthenticatedClient();
            var response = await client.PutAsJsonAsync($"{_apiBaseUrl}/api/orderdetails/orderdetails/{orderdetail.OrderDetailID}", orderdetail);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Произошла ошибка. Пожалуйста, попробуйте позже.");
                ViewBag.Products = await GetProductsFromApi();
                ViewBag.Orders = await GetOrdersFromApi();
                return View(orderdetail);
            }

            return RedirectToAction("AdminManage");
        }

        public async Task<IActionResult> DeleteOrderDetails(int id)
        {
            var client = await GetAuthenticatedClient();
            var response = await client.DeleteAsync($"{_apiBaseUrl}/api/orderdetails/orderdetails/{id}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error deleting orderdetails: {response.StatusCode}");
            }

            return RedirectToAction("AdminManage");
        }


        // Методы для работы с категориями
        public IActionResult AddCategory()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(Category category)
        {
            if (!ModelState.IsValid) return View(category);

            var client = await GetAuthenticatedClient();
            var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/api/categories/categories", category);

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("CategoryName", errorMessage);
                return View(category);
            }

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Произошла ошибка. Попробуйте позже.");
                return View(category);
            }

            TempData["SuccessMessage"] = "Категория успешно добавлена!";
            return RedirectToAction("AdminManage");
        }

        public async Task<IActionResult> EditCategory(int id)
        {
            var client = await GetAuthenticatedClient();
            var response = await client.GetAsync($"{_apiBaseUrl}/api/categories/categories/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }

            if (!response.IsSuccessStatusCode)
            {
                return View("Error");
            }

            var category = await response.Content.ReadFromJsonAsync<Category>();
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(Category category)
        {
            if (!ModelState.IsValid)
            {

                return View(category);
            }

            var client = await GetAuthenticatedClient();
            var response = await client.PutAsJsonAsync($"{_apiBaseUrl}/api/categories/categories/{category.CategoryID}", category);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Произошла ошибка. Пожалуйста, попробуйте позже.");
                return View(category);
            }

            return RedirectToAction("AdminManage");
        }

        public async Task<IActionResult> DeleteCategory(int id)
        {
            var client = await GetAuthenticatedClient();
            var response = await client.DeleteAsync($"{_apiBaseUrl}/api/categories/categories/{id}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error deleting categories: {response.StatusCode}");
            }

            return RedirectToAction("AdminManage");
        }

        public IActionResult AddRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddRole(Role role)
        {
            if (!ModelState.IsValid) return View(role);

            var client = await GetAuthenticatedClient();
            var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/api/roles/roles", role);

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("RoleName", errorMessage);
                return View(role);
            }

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Произошла ошибка. Попробуйте позже.");
                return View(role);
            }

            TempData["SuccessMessage"] = "Роль успешно добавлена!";
            return RedirectToAction("AdminManage");
        }

        public async Task<IActionResult> EditRole(int id)
        {
            var client = await GetAuthenticatedClient();
            var response = await client.GetAsync($"{_apiBaseUrl}/api/roles/roles/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }

            if (!response.IsSuccessStatusCode)
            {
                return View("Error");
            }

            var category = await response.Content.ReadFromJsonAsync<Role>();
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(Role role)
        {
            if (!ModelState.IsValid)
            {

                return View(role);
            }

            var client = await GetAuthenticatedClient();
            var response = await client.PutAsJsonAsync($"{_apiBaseUrl}/api/roles/roles/{role.RoleID}", role);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Произошла ошибка. Пожалуйста, попробуйте позже.");
                return View(role);
            }

            return RedirectToAction("AdminManage");
        }

        public async Task<IActionResult> DeleteRole(int id)
        {
            var client = await GetAuthenticatedClient();
            var response = await client.DeleteAsync($"{_apiBaseUrl}/api/roles/roles/{id}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error deleting roles: {response.StatusCode}");
            }

            return RedirectToAction("AdminManage");
        }

        public async Task<IActionResult> AuditLog()
        {
            var client = await GetAuthenticatedClient();
            var response = await client.GetAsync($"{_apiBaseUrl}/api/admin/auditlogs");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.ErrorMessage = "Не удалось загрузить журнал аудита";
                return View(new List<AuditLog>());
            }

            var logs = await response.Content.ReadFromJsonAsync<List<AuditLog>>() ?? new List<AuditLog>();
            return View(logs);
        }





        private async Task<List<Role>> GetRolesFromApi()
        {
            var client = await GetAuthenticatedClient();
            var response = await client.GetAsync($"{_apiBaseUrl}/api/roles/roles");
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<List<Role>>()
                : new List<Role>();
        }

        private async Task<List<Category>> GetCategoriesFromApi()
        {
            var client = await GetAuthenticatedClient();
            var response = await client.GetAsync($"{_apiBaseUrl}/api/categories/categories");
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<List<Category>>()
                : new List<Category>();
        }

        private async Task<List<User>> GetUsersFromApi()
        {
            var client = await GetAuthenticatedClient();
            var response = await client.GetAsync($"{_apiBaseUrl}/api/users/users");
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<List<User>>()
                : new List<User>();
        }

        private async Task<List<Product>> GetProductsFromApi()
        {
            var client = await GetAuthenticatedClient();
            var response = await client.GetAsync($"{_apiBaseUrl}/api/products/products");
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<List<Product>>()
                : new List<Product>();
        }

        private async Task<List<Order>> GetOrdersFromApi()
        {
            var client = await GetAuthenticatedClient();
            var response = await client.GetAsync($"{_apiBaseUrl}/api/admin/orders");
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<List<Order>>()
                : new List<Order>();
        }
    }
}