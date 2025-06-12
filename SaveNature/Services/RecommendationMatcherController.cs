using Microsoft.EntityFrameworkCore;
using SaveNature.Data;

namespace SaveNature.Services
{
    public class RecommendationMatcherService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<RecommendationMatcherService> _logger;

        public RecommendationMatcherService(ApplicationDbContext dbContext, ILogger<RecommendationMatcherService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<string> GetRecommendationAsync(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
            {
                _logger.LogWarning("Product name is null or empty");
                return "Нет рекомендации";
            }

            var recommendation = await _dbContext.Recommendations
                .Where(r => productName.ToLower().Contains(r.Purchase.ToLower()))
                .Select(r => r.RecommendationText)
                .FirstOrDefaultAsync();

            return recommendation ?? "Нет рекомендации";
        }
    }
}