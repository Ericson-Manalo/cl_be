using cl_be.Models.Dto.AddressDto;

namespace cl_be.Models.Dto.OrderDto
{
    public class OrderDetailDto
    {
        public int SalesOrderId { get; set; }
        public string SalesOrderNumber { get; set; } = null!;
        public DateTime OrderDate { get; set; }
        public DateTime? ShipDate { get; set; }
        public byte Status { get; set; }

        public string ShipMethod { get; set; } = null!;
        public decimal SubTotal { get; set; }
        public decimal TaxAmt { get; set; }
        public decimal Freight { get; set; }
        public decimal TotalDue { get; set; }

        public AddressesDetailDto? BillToAddress { get; set; }
        public AddressesDetailDto? ShipToAddress { get; set; }

        public List<OrderDetailItemDto> Items { get; set; } = new();
    }
}
