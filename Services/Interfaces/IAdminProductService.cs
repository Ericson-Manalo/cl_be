using cl_be.Models.Dto.ProductDto;
using cl_be.Models.Pagination;

namespace cl_be.Services.Interfaces
{
    public interface IAdminProductService
    {
        Task<PagedResult<ProductListDto>> GetProductsAsync(int pageNumber, int pageSize, string? sortBy, string? sortDirection);
        Task<AdminProductEditDto> GetProductForEditAsync(int productId);
        Task<IEnumerable<AdminCategoryDto>> GetCategoriesAsync();
        Task<IEnumerable<AdminProductModelDto>> GetModelsAsync();
    }
}
