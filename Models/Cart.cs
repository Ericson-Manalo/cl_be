using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cl_be.Models
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; }
        public int CustomerId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;

        // Navigazione verso le righe
        public virtual ICollection<CartItem> Items { get; set; } =[];

    }
}
