namespace SaveNature.Models
{
    public class Recommendation
    {
        public int Id { get; set; }
        public required string Purchase { get; set; }
        public string? RecommendationText { get; set; }
    }
}
