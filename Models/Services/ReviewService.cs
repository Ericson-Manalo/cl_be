using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace cl_be.Models.Services
{
    public class ReviewService
    {
        //private readonly IMongoCollection<Review> _reviews;

        //public ReviewService(IOptions<ReviewMDBConfig> config)
        //{
        //    var mongoConfig = config.Value; // prelevo i valori da appsettings.json

        //    var client = new MongoClient(mongoConfig.ConnectionString);
        //    var database = client.GetDatabase(mongoConfig.DatabaseName);

        //    _reviews = database.GetCollection<Review>(mongoConfig.ReviewsCollectionName);
        //}

        //public async Task<List<Review>> GetReviewsForProduct(int productId)
        //{
        //    return await _reviews.Find(r => r.ProductId == productId.ToString()).ToListAsync();
        //}

    }
}
