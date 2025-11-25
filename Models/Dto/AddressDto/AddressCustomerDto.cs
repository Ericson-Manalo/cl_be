namespace cl_be.Models.Dto.AddressDto
{
    public class AddressCustomerDto
    {
        public int AddressId { get; set; }
        public string AddressLine1 { get; set; } = null!;
        public string City { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
    }
}
