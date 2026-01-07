using cl_be.Models.Dto.ProductDto;
using cl_be.Models.Pagination;

namespace cl_be.Services.Interfaces
{
    public interface IAdminProductService
    {
        // GET: for a table list UI setup
        Task<PagedResult<ProductListDto>> GetProductsAsync(int pageNumber, int pageSize, string? sortBy, string? sortDirection);

        Task<AdminProductEditDto> GetProductForEditAsync(int productId);

        // GET: for modern list UI setup
        Task<Page<ProductListDto>> GetAllProductsAsync(int page, int pageSize, string? search=null);


        // HELPERS: To get the categories and models
        Task<IEnumerable<AdminProductCategoryDto>> GetCategoriesAsync();

        Task<IEnumerable<AdminProductModelDto>> GetModelsAsync();

        // PUT/PATCH:
        Task UpdateAsync(AdminProductUpdateDto dto);
    }
}
