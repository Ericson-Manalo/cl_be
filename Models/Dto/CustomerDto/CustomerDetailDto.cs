using cl_be.Models.Dto.AddressDto;

namespace cl_be.Models.Dto.CustomerDto
{
    public class CustomerDetailDto
    {
        public int CustomerId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? EmailAddress { get; set; }
        public string? Phone { get; set; }
        public List<AddressCustomerDto> Addresses { get; set; } = new();
    }
}
