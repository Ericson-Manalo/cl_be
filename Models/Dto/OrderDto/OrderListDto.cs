namespace cl_be.Models.Dto.OrderDto
{
    public class OrderListDto
    {
        public int SalesOrderId { get; set; }
        public string OrderDate { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalDue { get; set; }
    }
}
