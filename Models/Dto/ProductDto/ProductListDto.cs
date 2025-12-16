namespace cl_be.Models.Dto.ProductDto
{
    public class ProductListDto
    {
        public int ProductId { get; set; }
        public string ProductNumber { get; set; }
        public string Name { get; set; }
        public decimal ListPrice { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
    }
}
