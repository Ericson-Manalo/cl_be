using cl_be.Models.Dto.ProductDto.Admin;
using cl_be.Models.Pagination;

namespace cl_be.Services.Interfaces
{
    public interface IProductService
    {
        // admin
        Task<List<AdminProductListDto>> GetAllProductsAsync();
        Task<Page<AdminProductListDto>> GetProductsPagedAsync(int page, int pageSize, string? search=null);
        Task<AdminProductDetailDto> GetProductDetailAsync(int productId);
    }
}
