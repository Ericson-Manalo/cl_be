namespace cl_be.Models.Dto.AddressDto
{
    public class AddressUpdateDto
    {
        public int AddressId { get; set; }
        public string AddressLine1 { get; set; }
        public string City { get; set; }
        public string CountryRegion { get; set; }
        public string StateProvince { get; set; }
        public string PostalCode { get; set; }
        public string AddressType { get; set; }   // Main Office, Shipping...
    }
}
