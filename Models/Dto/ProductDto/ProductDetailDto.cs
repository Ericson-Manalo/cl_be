namespace cl_be.Models.Dto.ProductDto
{
    public class ProductDetailDto
    {


        public int ProductId { get; set; }

        public string Name { get; set; } = null!;


        public string? Color { get; set; }

        public decimal StandardCost { get; set; }


        public decimal ListPrice { get; set; }


        public int? ProductCategoryId { get; set; }


        public string CategoryName { get; set; } = string.Empty;


        public byte[]? ThumbNailPhoto { get; set; }


        public string? Size { get; set; }

      
        public decimal? Weight { get; set; }


        public string ProductNumber { get; set; } = null!;


        public List<Review> Reviews { get; set; } = [];



       
        public Dictionary<string, string> Descriptions { get; set; } = [];
        //public string Culture { get; set; } = "en-US";

    }
}
