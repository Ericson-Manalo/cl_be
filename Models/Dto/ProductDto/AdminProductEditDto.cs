namespace cl_be.Models.Dto.ProductDto
{
    public class AdminProductEditDto
    {
        // General
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public string ProductNumber { get; set; } = null!;
        public int? ProductCategoryId { get; set; }
        public int? ParentCategoryId { get; set; }
        public int? ProductModelId { get; set; }

        // Pricing
        public decimal ListPrice { get; set; }
        public decimal StandardCost { get; set; }

        // Attributes
        public string? Color { get; set; }
        public string? Size { get; set; }
        public decimal? Weight { get; set; }

        // Availability
        public DateTime SellStartDate { get; set; }
        public DateTime? SellEndDate { get; set; }
        public DateTime? DiscontinuedDate { get; set; }

        // Business rules
        public bool HasOrders { get; set; }
    }
}
