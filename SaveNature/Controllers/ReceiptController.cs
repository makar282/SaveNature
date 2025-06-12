using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using SaveNature.Data;
using SaveNature.Models;
using SaveNature.Services;

namespace SaveNature.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReceiptController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _dbContext;
        private const string ApiUrl = "https://proverkacheka.com/api/v1/check/get";
        private const string ApiToken = "33812.cIfyRu0WPp22Y8btm"; // Замените на ваш токен
        private readonly ILogger<ReceiptController> _logger; // Добавьте поле

        public ReceiptController(HttpClient httpClient, ApplicationDbContext dbContext, ILogger<ReceiptController> logger)
        {
            _httpClient = httpClient;
            _dbContext = dbContext;
            _logger = logger; // Инициализация логгера
        }

        [HttpPost("add-receipt")]
        public async Task<IActionResult> AddReceipt([FromBody] QrCodeRequest request)
        {
            if (request == null)
            {
                _logger.LogError("Request is null");
                return BadRequest(new { errors = new { General = "Request body is null." } });
            }

            if (string.IsNullOrEmpty(request.QrRaw) && string.IsNullOrEmpty(request.QrUrl))
            {
                _logger.LogWarning("Both QrRaw and QrUrl are empty");
                return BadRequest(new { errors = new { General = "QR code data or URL is required." } });
            }

            var formData = new Dictionary<string, string>
    {
        { "token", ApiToken },
        { request.QrRaw != null ? "qrraw" : "qrurl", request.QrRaw ?? request.QrUrl }
    };

            try
            {
                using var content = new FormUrlEncodedContent(formData);
                _httpClient.DefaultRequestHeaders.Add("Cookie", "ENGID=1.1");
                var response = await _httpClient.PostAsync(ApiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("API request failed: {StatusCode}, {Response}", response.StatusCode, responseContent);
                    return StatusCode((int)response.StatusCode, new { errors = new { ApiError = responseContent } });
                }

                if (string.IsNullOrEmpty(responseContent))
                {
                    _logger.LogWarning("API response is empty");
                    return BadRequest("API response is empty");
                }

                _logger.LogInformation("API response: {ResponseContent}", responseContent);

                var receipt = new Receipt
                {
                    ReceiptData = responseContent,
                    TotalAmount = 0,
                    PurchaseDate = DateTime.UtcNow.Date,
                    CreatedAt = DateTime.UtcNow,
                    UserName = request.UserName ?? "Unknown"
                };

                _dbContext.Receipts.Add(receipt);
                await _dbContext.SaveChangesAsync();
                await ParseReceiptDataAndSaveItems(receipt); // ← парсинг происходит сразу после сохранения
                _logger.LogInformation("Receipt and items added successfully");
                return Ok(new { message = "Чек и товары успешно добавлены!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding receipt");
                return StatusCode(500, new { errors = new { ServerError = ex.Message } });
            }
        }

        private async Task ParseReceiptDataAndSaveItems(Receipt receipt)
        {
            if (string.IsNullOrWhiteSpace(receipt.ReceiptData)) return;

            try
            {
                var parsed = JObject.Parse(receipt.ReceiptData);
                var items = parsed["data"]?["json"]?["items"];

                if (items != null)
                {
                    foreach (var item in items)
                    {
                        var name = item["name"]?.ToString();
                        var priceStr = item["price"]?.ToString();
                        decimal.TryParse(priceStr, out decimal price);

                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            _dbContext.Items.Add(new Item
                            {
                                ReceiptId = receipt.Id,
                                Receipt = receipt,
                                ProductName = name,
                                Price = 0,
                                EcoScore = 0
                            });
                        }
                    }

                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при парсинге ReceiptData для чека с ID {ReceiptId}", receipt.Id);
            }
        }



        [HttpPost]
        public async Task<IActionResult> AddReceipt([FromBody] Receipt receipt)
        {
            if (receipt == null || string.IsNullOrWhiteSpace(receipt.ReceiptData))
                return BadRequest("Некорректный чек");

            _dbContext.Receipts.Add(receipt);
            await _dbContext.SaveChangesAsync();

            try
            {
                // Пытаемся распарсить JSON из ReceiptData
                JObject parsed = JObject.Parse(receipt.ReceiptData);

                var items = parsed["data"]?["json"]?["items"];
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        var name = item["name"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            _dbContext.Items.Add(new Item
                            {
                                ReceiptId = receipt.Id,
                                Receipt = receipt,
                                ProductName = name,
                                Price = 0,
                                EcoScore = 0
                            });
                        }
                    }

                    await _dbContext.SaveChangesAsync();
                }
            }
            catch
            {
                return BadRequest("Не удалось распарсить JSON из ReceiptData");
            }

            return Ok("Чек и товары успешно добавлены");
        }

        /// <summary>
        /// GetReceipts
        /// </summary>
        /// <param name="matcherService"></param>
        /// <returns></returns>
        [HttpGet("get-receipts")]
        public async Task<IActionResult> GetReceipts()
        {
            var userName = User?.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
            {
                _logger.LogWarning("User must be authenticated");
                return BadRequest("User must be authenticated");
            }

            // Шаг 1: Загружаем все чеки и их элементы для пользователя
            var receipts = await _dbContext.Receipts
                .Where(r => r.UserName == userName)
                .Include(r => r.Items)
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            // Шаг 2: Загружаем все рекомендации
            var recommendations = await _dbContext.Recommendations.ToListAsync();

            // Шаг 3: Формируем ответ с сопоставлением рекомендаций
            var result = receipts.Select(r => new
            {
                Id = r.Id,
                PurchaseDate = r.PurchaseDate,
                Items = r.Items.Select(i => new
                {
                    i.ProductName,
                    i.EcoScore,
                    Recommendation = recommendations
                        .Where(rec => i.ProductName.ToLower().Contains(rec.Purchase.ToLower()))
                        .Select(rec => rec.RecommendationText)
                        .FirstOrDefault() ?? "Нет рекомендации"
                }).ToList()
            }).ToList();

            _logger.LogInformation("Retrieved receipts for user: {UserName}", userName);
            return Ok(result);
        }

        private async Task<int> CalculateEcoScore(string productName)
        {
            var recommendation = await _dbContext.Recommendations
                .FirstOrDefaultAsync(r => r.Purchase == productName);
            if (recommendation.RecommendationText.Contains("Отличный выбор"))
            {
                return recommendation != null ? (90) : 50;
            }
            else
            {
                return recommendation != null ? (20) : 50;
            }
        }

    }
}