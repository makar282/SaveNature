using System.ComponentModel.DataAnnotations.Schema;

namespace SaveNature.Models
{
    public class Item
    {
        public int Id { get; set; }
        public int ReceiptId { get; set; }
        public required string ProductName { get; set; }
        public decimal Price { get; set; }
        public int EcoScore { get; set; }

        [ForeignKey("ReceiptId")]
        public required Receipt Receipt { get; set; } // Добавлено свойство навигации
    }
}