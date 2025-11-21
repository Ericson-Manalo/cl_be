namespace cl_be.Models.Dto.CategoryDto
{
    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public int ProductCount { get; set; }
    }
}
