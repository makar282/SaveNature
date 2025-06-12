namespace SaveNature.Models
{
    /// <summary>
    /// Модель товара
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Email { get; set; }
    }

    public class ProfileViewModel
    {
        public DateTime? FirstReceiptDate { get; set; }
        public int EcoPurchasesCount { get; set; }
        public double EcoRating { get; set; }
    }
}

