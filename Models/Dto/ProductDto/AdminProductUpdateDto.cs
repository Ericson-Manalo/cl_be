using System.ComponentModel.DataAnnotations;

namespace cl_be.Models.Dto.ProductDto
{
    public class AdminProductUpdateDto
    {
        [Required]
        public int ProductId { get; set; }

        // General
        [Required]
        public int? ProductCategoryId { get; set; }

        [Required]
        public int? ProductModelId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(25)]
        public string ProductNumber { get; set; } = string.Empty;

        // Pricing
        [Range(0, 999999)]
        public decimal StandardCost { get; set; }

        [Range(0, 999999)]
        public decimal ListPrice { get; set; }

        // Attributes
        [StringLength(15)]
        public string Color { get; set; } = string.Empty;

        [StringLength(5)]
        public string Size { get; set; }  = string.Empty;

        [Range(0, 999999)]
        public decimal? Weight { get; set; }

        // Availability
        [Required]
        [DataType(DataType.Date)]
        public DateTime SellStartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? SellEndDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DiscontinuedDate { get; set; }
    }

}
