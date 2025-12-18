namespace cl_be.Models.Dto.CartDto
{
    public class CartItemDTO
    {
        public int CartItemId { get; set; }  
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        //public string? ProductNumber { get; set; } 
        public decimal Price { get; set; }  // ListPrice del prodotto
        public int Quantity { get; set; }
        public decimal Subtotal => Price * Quantity;
        public byte[]? ThumbnailPhotoFileName { get; set; }
        public DateTime AddedDate { get; set; }
    }
}
