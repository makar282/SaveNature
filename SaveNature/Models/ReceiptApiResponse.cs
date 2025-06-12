namespace SaveNature.Models
{
    /// <summary>
    /// Модель для работы с чеками
    /// </summary>
    public class ReceiptApiResponse
    {
        public decimal TotalAmount { get; set; }
        public DateTime PurchaseDate { get; set; }
        //public List<ReceiptItem> Items { get; set; }
    }
}
