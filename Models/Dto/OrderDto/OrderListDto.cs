namespace cl_be.Models.Dto.OrderDto
{
    public class OrderListDto
    {
        public int SalesOrderId { get; set; }

        public DateTime OrderDate { get; set; }

        public byte Status { get; set; }

        public int CustomerId { get; set; }

        public int? ShipToAddressId { get; set; }

        public int? BillToAddressId { get; set; }

        public decimal TotalDue { get; set; }

    }
}
