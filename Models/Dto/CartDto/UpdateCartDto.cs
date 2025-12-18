using System.ComponentModel.DataAnnotations;

namespace cl_be.Models.Dto.CartDto
{
    public class UpdateCartDto
    {
        [Required]
        [Range(0, 999, ErrorMessage = "Quantity must be between 0 and 999")]
        public int Quantity { get; set; }
    }
}
