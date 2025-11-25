namespace cl_be.Models.Dto.OrderDto
{
    public class OrderCustomerDto
    {
        public int SalesOrderId { get; set; }
        public string SalesOrderNumber { get; set; } = null!;
        public DateTime OrderDate { get; set; }
        public decimal TotalDue { get; set; }
        public byte Status { get; set; }
    }
}
