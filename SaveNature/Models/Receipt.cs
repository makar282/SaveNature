namespace SaveNature.Models
{
    /// <summary>
    /// Модель чека
    /// </summary>
    public class Receipt
    {
        public int Id { get; set; }
        public required string UserName { get; set; }
        public required string ReceiptData { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<Item> Items { get; set; } = new List<Item>(); // свойство навигации
    }
}