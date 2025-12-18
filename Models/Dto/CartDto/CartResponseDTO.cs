namespace cl_be.Models.Dto.CartDto
{
    public class CartResponseDTO
    {
        public int CartId { get; set; }  
        public List<CartItemDTO> Items { get; set; } = [];

        // Calcoli automatici
        public decimal TotalAmount => Items.Sum(i => i.Subtotal);
        public int TotalItems => Items.Sum(i => i.Quantity);
        public int UniqueProducts => Items.Count; 

        // Metadata carrello (dalla testata Cart)
        //public DateTime CreatedDate { get; set; }
        //public DateTime ModifiedDate { get; set; }
    }
}
