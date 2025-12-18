using System.ComponentModel.DataAnnotations;

namespace cl_be.Models.Dto.CartDto
{
    public class RemoveFromCartDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int CartItemId { get; set; }
    }
}
