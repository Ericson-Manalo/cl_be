namespace cl_be.Models.Dto.OrderDto
{
    public class OrderDetailDto
    {
        public int SalesOrderId { get; set; }

        public DateTime OrderDate { get; set; }

        public byte Status { get; set; }

        public int CustomerId { get; set; }

        public int? ShipToAddressId { get; set; }

        public int? BillToAddressId { get; set; }

        public decimal TotalDue { get; set; }

        // Address
        //public AddressDto? BillToAddress { get; set; }
        //public AddressDto? ShipToAddress { get; set; }

        // Riga dettagli
        //public List<OrderLineDto> Lines { get; set; } = new();


        //In dto separati
        //public class AddressDto
        //{
        //    public string AddressLine1 { get; set; } = "";
        //    public string City { get; set; } = "";
        //    public string PostalCode { get; set; } = "";
        //}


        //public class OrderLineDto
        //{
        //    public int ProductId { get; set; }
        //    public string ProductName { get; set; } = "";
        //    public int Quantity { get; set; }
        //    public decimal UnitPrice { get; set; }
        //    public decimal LineTotal { get; set; }
        //}
    }
}
