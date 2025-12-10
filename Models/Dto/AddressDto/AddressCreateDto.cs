namespace cl_be.Models.Dto.AddressDto
{
    public class AddressCreateDto
    {
        public string AddressLine1 { get; set; }
        public string City { get; set; }
        public string CountryRegion { get; set; }
        public string StateProvince { get; set; }
        public string PostalCode { get; set; }
        public string AddressType { get; set; }   // Main Office, Shipping...

        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        //public string Rowguid { get; set; }
    }
}
