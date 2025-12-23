using cl_be.Models.Dto.OrderDto;
using cl_be.Models.Pagination;
using Microsoft.AspNetCore.Mvc;
namespace cl_be.Services.Interfaces
{
    public interface IOrderService
    {
        Task<List<OrderListDto>> GetAllOrdersAsync();

        Task<Page<OrderListDto>> GetOrdersPagedAsync(int page, int pageSize, string? search = null);

        Task<OrderDetailDto> GetDetailOrder(int orderId);

        Task<bool> DeleteOrderAsync(int orderId);

        Task<bool> UpdateOrderStatusAsync(int orderId, byte newStatus);


        //customer
        Task<OrderDetailDto?> GetCustomerOrderDetailAsync(int orderId, int customerId);

    }
}
