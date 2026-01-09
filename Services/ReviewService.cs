using cl_be.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace cl_be.Services
{
    public class ReviewService
    {
        private readonly IMongoCollection<Review> _reviews;

        public ReviewService(IOptions<ReviewMDBConfig> config)
        {
            var mongoConfig = config.Value;

            var client = new MongoClient(mongoConfig.ConnectionString);
            var database = client.GetDatabase(mongoConfig.DatabaseName);
            _reviews = database.GetCollection<Review>(mongoConfig.ReviewsCollectionName);
        }

        public async Task<List<Review>> GetReviewsForProduct(int productId)
        {
            return await _reviews.Find(r => r.ProductId == productId).ToListAsync();
        }

    }
}
