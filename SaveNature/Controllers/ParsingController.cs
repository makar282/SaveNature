using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using SaveNature.Data;
using SaveNature.Models;

namespace SaveNature.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParsingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ParsingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Parsing/ParseNew
        [HttpGet("ParseNew")]
        public async Task<IActionResult> ParseNewReceipts()
        {
            var receipts = await _context.Receipts
                .Where(r => !_context.Items.Any(i => i.ReceiptId == r.Id))
                .ToListAsync();

            foreach (var receipt in receipts)
            {
                if (string.IsNullOrEmpty(receipt.ReceiptData))
                    continue;

                try
                {
                    var jObject = JObject.Parse(receipt.ReceiptData);
                    JToken? items = null;


                    // Пробуем первый путь
                    items = jObject["data"]?["json"]?["items"];

                    // Если не найдено, пробуем второй
                    if (items == null)
                        items = jObject["ticket"]?["document"]?["receipt"]?["items"];

                    // Если нашли — обрабатываем
                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            var name = item["name"]?.ToString();
                            decimal price = item["price"].Value<decimal>();
                            decimal sum = item["sum"].Value<decimal>();

                            if (name == null || price == null)
                            {
                                continue;
                            }
                            _context.Items.Add(new Item
                            {
                                ReceiptId = receipt.Id,
                                Receipt = receipt,
                                ProductName = name,
                                Price = price,
                                EcoScore = 0
                            });
                        }
                    }
                }
                catch
                {
                    Console.WriteLine($"Ошибка при парсинге чека с ID {receipt.Id}");
                    continue;
                }
            }

            await _context.SaveChangesAsync();
            return Ok("Данные успешно распарсены и добавлены.");
        }
    }
}
