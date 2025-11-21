namespace cl_be.Models.Dto.ProductDto
{
    public class ProductCardDto
    {

        public int ProductId { get; set; }

        public string Name { get; set; } = null!;


        //public string? Color { get; set; }

        //public decimal StandardCost { get; set; }


        public decimal ListPrice { get; set; }

        
        public int? ProductCategoryId { get; set; }


        public string CategoryName { get; set; } = string.Empty;


        public byte[]? ThumbNailPhoto { get; set; }
    }
}
