using cl_be.Models;
using cl_be.Models.Dto.ProductDto.Admin;
using cl_be.Models.Pagination;
using cl_be.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace cl_be.Services
{
    public class ProductService : IProductService
    {
        private readonly AdventureWorksLt2019Context _context;

        public ProductService(AdventureWorksLt2019Context context)
        {
            _context = context;
        }

        // admin
        public async Task<List<AdminProductListDto>> GetAllProductsAsync()
        {
            return await _context.Products
                .OrderBy(p => p.ProductId)
                .Select(p => new AdminProductListDto
                {
                    ProductId = p.ProductId,
                    ProductNumber = p.ProductNumber,
                    Name = p.Name,
                })
                .ToListAsync();
        }

        public async Task<Page<AdminProductListDto>> GetProductsPagedAsync(int page, int pageSize, string? search = null)
        {
            // AsNoTracking migliora le performance in lettura
            var query = _context.Products.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.ProductNumber.Contains(search) || p.Name.Contains(search));
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await query
                .OrderBy(p =>p.ProductId)
                .Skip((page -1) * pageSize)
                .Take(pageSize)
                .Select(p => new AdminProductListDto
                {
                    ProductId = p.ProductId,
                    ProductNumber = p.ProductNumber,
                    Name = p.Name,
                })
                .ToListAsync();

            return new Page<AdminProductListDto>
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                Items = items
            };
        }

        public async Task<AdminProductDetailDto> GetProductDetailAsync(int productId)
        {
            var product = await _context.Products
            .AsNoTracking()
            .Where(p => p.ProductId == productId)
            .Select(p => new AdminProductDetailDto
            {
                ProductId = p.ProductId,

                // General
                ProductCategoryId = p.ProductCategoryId,
                ProductModelId = p.ProductModelId,
                ProductNumber = p.ProductNumber,
                Name = p.Name,

                // Pricing
                ListPrice = p.ListPrice,
                StandardCost = p.StandardCost,

                // Attributes
                Color = p.Color,
                Size = p.Size,
                Weight = p.Weight,

                // Availability
                SellStartDate = p.SellStartDate,
                SellEndDate = p.SellEndDate,
                DiscontinuedDate = p.DiscontinuedDate,

                // Display-only fields
                //CategoryName = p.ProductCategory!.Name,
                //ParentCategoryName = p.ProductCategory!.ParentProductCategory!.Name,
                //ModelName = p.ProductModel!.Name,
            })
            .FirstOrDefaultAsync();

            if (product == null)
                throw new KeyNotFoundException($"Product with ID {productId} not found.");

            return product;
        }
    }
}
