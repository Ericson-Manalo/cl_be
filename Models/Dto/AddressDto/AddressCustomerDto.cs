namespace cl_be.Models.Dto.AddressDto
{
    public class AddressCustomerDto
    {
        public int AddressId { get; set; }
        public string AddressLine1 { get; set; } = null!;
        public string City { get; set; } = null!;
        public string PostalCode { get; set; } = null!;

        public string StateProvince { get; set; } = null!;
        public string CountryRegion { get; set; } = null!;
        public string AddressType { get; set; } = null!;
    }
}
