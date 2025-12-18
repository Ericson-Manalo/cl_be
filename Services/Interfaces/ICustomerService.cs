using cl_be.Models.Dto.CustomerDto;
using cl_be.Models.Pagination;

namespace cl_be.Services.Interfaces
{
    public interface ICustomerService
    {

        //user
        Task<CustomerProfileDto> GetCustomerProfileAsync(int customerId);
        Task UpdateCustomerAsync(int customerId, CustomerUpdateDto dto);

        //admin
        Task<List<CustomerListDto>> GetAllCustomersAsync();
        Task<Page<CustomerListDto>> GetCustomersPagedAsync(int page, int pageSize, string? search=null);
        Task<CustomerDetailDto> GetCustomerDetailAsync(int customerId);
        Task DeleteCustomerAsync(int customerId);

    }
}
