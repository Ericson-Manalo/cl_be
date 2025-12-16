using cl_be.Models;
using cl_be.Models.Dto.ProductDto;
using cl_be.Models.Pagination;
using cl_be.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace cl_be.Services.Implementations
{
    public class AdminProductService : IAdminProductService
    {
        private readonly AdventureWorksLt2019Context _context;

        public AdminProductService(AdventureWorksLt2019Context context)
        {
            _context = context;
        }

        public async Task<PagedResult<ProductListDto>> GetProductsAsync(int pageNumber, int pageSize, string? sortBy, string? sortDirection)
        {
            var query = _context.Products.AsNoTracking();

            query = sortBy switch
            {
                "name" => sortDirection == "desc"
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name),

                "price" => sortDirection == "desc"
                    ? query.OrderByDescending(p => p.ListPrice)
                    : query.OrderBy(p => p.ListPrice),

                "category" => sortDirection == "desc"
                    ? query.OrderByDescending(p => p.ProductCategory!.Name)
                    : query.OrderBy(p => p.ProductCategory!.Name),

                // I want to sort parent category too!
                "parentcategory" => sortDirection == "desc"
                    ? query.OrderByDescending(p => 
                        p.ProductCategory != null &&
                        p.ProductCategory.ParentProductCategory != null
                            ? p.ProductCategory.ParentProductCategory.Name: null)
                    : query.OrderBy(p => 
                        p.ProductCategory != null &&
                        p.ProductCategory.ParentProductCategory != null
                            ? p.ProductCategory.ParentProductCategory.Name : null),

                _ => query.OrderBy(p => p.ProductId),
            };  

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductListDto
                {
                    ProductId = p.ProductId,
                    ProductNumber = p.ProductNumber,
                    Name = p.Name,                 
                    ListPrice = p.ListPrice,
                    CategoryId = p.ProductCategoryId,
                    CategoryName = p.ProductCategory != null
                        ? p.ProductCategory.Name
                        : null,

                    ParentCategoryId = p.ProductCategory != null
                        ? p.ProductCategory.ParentProductCategoryId
                        : null,

                    ParentCategoryName = p.ProductCategory != null
                        && p.ProductCategory.ParentProductCategory != null
                            ? p.ProductCategory.ParentProductCategory.Name
                            : null
                })
                .ToListAsync();

            return new PagedResult<ProductListDto>
            {
                TotalCount = totalCount,
                Items = items
            };
        }
    }
}
