using cl_be.Models.Dto.AddressDto;
using cl_be.Models.Dto.OrderDto;

namespace cl_be.Models.Dto.CustomerDto
{
    public class CustomerProfileDto
    {
        public int CustomerId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? EmailAddress { get; set; }
        public string? Phone { get; set; }
        public List<AddressCustomerDto> Addresses { get; set; } = new();
        public List<OrderCustomerDto> Orders { get; set; } = new();
    }
}
