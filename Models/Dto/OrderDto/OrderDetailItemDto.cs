namespace cl_be.Models.Dto.OrderDto
{
    public class OrderDetailItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int OrderQty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        public string? Thumbnail { get; set; }
    }
}
