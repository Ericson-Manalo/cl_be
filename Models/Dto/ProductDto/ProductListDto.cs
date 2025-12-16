namespace cl_be.Models.Dto.ProductDto
{
    public class ProductListDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string? Color { get; set; }
        public decimal StandardCost { get; set; }
        public decimal ListPrice { get; set; }
        public string? Size { get; set; }
        public decimal? Weight { get; set; }
    }
}
