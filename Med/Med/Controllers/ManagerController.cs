using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Med.Models;
using System.Text.Json;
using System.Net.Http.Headers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Hosting;
using System.Text;
using System.Net;
using System.IO;
using Med.Services;

namespace Med.Controllers
{
    [Authorize(Roles = "Менеджер")]
    public class ManagerController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ManagerController> _logger;
        private readonly IApiService _apiService;

        public ManagerController(
            IHttpClientFactory httpClientFactory,
            IWebHostEnvironment webHostEnvironment,
            ILogger<ManagerController> logger,IApiService apiService)
        {
            _httpClientFactory = httpClientFactory;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _apiService = apiService;
        }

        public async Task<IActionResult> ManagerManage()
        {
            try
            {
                var client = await _apiService.CreateAuthenticatedClient(HttpContext);
                var response = await client.GetAsync($"{_apiService.BaseUrl}/api/products/products");

                List<Product> products = new List<Product>(); 

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    products = JsonSerializer.Deserialize<List<Product>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<Product>(); 
                }
                else
                {
                    _logger.LogError($"Ошибка при получении продуктов: {response.StatusCode}");
                    TempData["ErrorMessage"] = "Ошибка при загрузке данных";
                }

                return View(new ManagerTables { Products = products });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в ManagerManage");
                TempData["ErrorMessage"] = "Ошибка сервера";
                return View(new ManagerTables { Products = new List<Product>() });
            }
        }

        public async Task<IActionResult> AddProduct()
        {
            ViewBag.Categories = await GetCategoriesFromApi();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product, IFormFile ImageFile)
        {
            if (ImageFile != null && ImageFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await ImageFile.CopyToAsync(ms);

                var content = new MultipartFormDataContent();
                content.Add(new StreamContent(new MemoryStream(ms.ToArray())), "file", ImageFile.FileName);

                var client = await _apiService.CreateAuthenticatedClient(HttpContext);
                var response = await client.PostAsync($"{_apiService.BaseUrl}/api/media/upload", content);

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

            var apiclient = await _apiService.CreateAuthenticatedClient(HttpContext);
            var productResponse = await apiclient.PostAsJsonAsync($"{_apiService.BaseUrl}/api/products/products", product);

            if (!productResponse.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Произошла ошибка. Пожалуйста, попробуйте позже.");
                ViewBag.Categories = await GetCategoriesFromApi();
                return View(product);
            }

            return RedirectToAction("ManagerManage");
        }

        public async Task<IActionResult> EditProduct(int id)
        {
            var client = await _apiService.CreateAuthenticatedClient(HttpContext);
            var response = await client.GetAsync($"{_apiService.BaseUrl}/api/products/products/{id}");

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

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await ImageFile.CopyToAsync(ms);

                    var content = new MultipartFormDataContent();
                    content.Add(new StreamContent(new MemoryStream(ms.ToArray())), "file", ImageFile.FileName);

                    var client = await _apiService.CreateAuthenticatedClient(HttpContext);
                    var response = await client.PostAsync($"{_apiService.BaseUrl}/api/media/upload", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<UploadResponse>();
                        if (result != null)
                        {
                            product.ImageUrl = result.Url;
                        }
                    }
                    else
                    {
                        var errorRaw = await response.Content.ReadAsStringAsync();
                    }
                }
  
                var apiclient = await _apiService.CreateAuthenticatedClient(HttpContext);                    
                var productResponse = await apiclient.PutAsJsonAsync($"{_apiService.BaseUrl}/api/products/products/{product.ProductID}", product);

                if (!productResponse.IsSuccessStatusCode)
                {
                    var errorContent = await productResponse.Content.ReadAsStringAsync();

                    ModelState.AddModelError("", "Произошла ошибка. Пожалуйста, попробуйте позже.");
                    ViewBag.Categories = await GetCategoriesFromApi();
                    return View(product);
                }

                return RedirectToAction("ManagerManage");
            }
            catch (Exception ex)
            {
                return View(product);
            }
        }

        public async Task<IActionResult> DeleteProduct(int id)
        {
            var client = await _apiService.CreateAuthenticatedClient(HttpContext);
            var response = await client.DeleteAsync($"{_apiService.BaseUrl}/api/products/products/{id}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error deleting products: {response.StatusCode}");
            }

            return RedirectToAction("ManagerManage");
        }

        public async Task<IActionResult> SalesAnalytics()
        {
            try
            {
                var client = await _apiService.CreateAuthenticatedClient(HttpContext);
                var response = await client.GetAsync($"{_apiService.BaseUrl}/api/analytics/sales");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Ошибка при получении аналитики: {response.StatusCode}");
                    return View();
                }

                var content = await response.Content.ReadAsStringAsync();
                var analytics = JsonSerializer.Deserialize<SalesAnalyticsResponse>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                ViewBag.ProductNames = JsonSerializer.Serialize(analytics.ByProduct.Select(p => p.ProductName));
                ViewBag.ProductSales = JsonSerializer.Serialize(analytics.ByProduct.Select(p => p.TotalSales));
                ViewBag.SalesDates = JsonSerializer.Serialize(analytics.ByDate.Select(d => d.Date));
                ViewBag.SalesValues = JsonSerializer.Serialize(analytics.ByDate.Select(d => d.TotalSales));

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в SalesAnalytics");
                TempData["ErrorMessage"] = "Ошибка сервера";
                return View();
            }
        }

        public async Task<IActionResult> ExportToPDF()
        {
            try
            {
                var client = await _apiService.CreateAuthenticatedClient(HttpContext);
                var response = await client.GetAsync($"{_apiService.BaseUrl}/api/analytics/sales");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Ошибка при получении данных для PDF: {response.StatusCode}");
                    TempData["ErrorMessage"] = "Ошибка при подготовке отчета";
                    return RedirectToAction("SalesAnalytics");
                }

                var content = await response.Content.ReadAsStringAsync();
                var analytics = JsonSerializer.Deserialize<SalesAnalyticsResponse>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var memoryStream = new MemoryStream();
                var document = new Document(PageSize.A4, 25, 25, 30, 30);
                var writer = PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                var fontPath = Path.Combine(_webHostEnvironment.WebRootPath, "fonts", "Montserrat-VariableFont_wght.ttf");
                var baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                var font = new Font(baseFont, 12);
                var titleFont = new Font(baseFont, 16, Font.BOLD);

                var title = new Paragraph("Аналитика продаж по товарам", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);

                var table = new PdfPTable(2);
                table.WidthPercentage = 100;
                table.SpacingBefore = 20f;
                table.SpacingAfter = 20f;

                var cell = new PdfPCell(new Phrase("Название товара", font));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Общая сумма продаж", font));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);

                foreach (var item in analytics.ByProduct)
                {
                    table.AddCell(new Phrase(item.ProductName, font));
                    table.AddCell(new Phrase(item.TotalSales.ToString("C"), font));
                }

                document.Add(table);
                document.Close();

                return File(memoryStream.ToArray(), "application/pdf", "SalesAnalytics.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании PDF");
                TempData["ErrorMessage"] = "Ошибка при создании отчета";
                return RedirectToAction("SalesAnalytics");
            }
        }

        private async Task<List<Category>> GetCategoriesFromApi()
        {
            var client = await _apiService.CreateAuthenticatedClient(HttpContext);
            var response = await client.GetAsync($"{_apiService.BaseUrl}/api/categories/categories");
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<List<Category>>()
                : new List<Category>();
        }

    }
}