using cl_be.Models;
using cl_be.Models.Dto.CustomerDto;
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

        // To get the list of products (table list version)
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
                            ? p.ProductCategory.ParentProductCategory.Name : null)
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
                            : null,

                    ModifiedDate = p.ModifiedDate,
                })
                .ToListAsync();

            return new PagedResult<ProductListDto>
            {
                TotalCount = totalCount,
                Items = items
            };
        }

        // To get the list of products (modern UI version)
        public async Task<Page<ProductListDto>> GetAllProductsAsync(int page, int pageSize, string? search=null)
        {
            var query = _context.Products.AsNoTracking(); // AsNoTracking migliora le performance in lettura

            if (!string.IsNullOrWhiteSpace(search))
            {
                // Rimuoviamo ToLower() per testare se il DB gestisce l'insensibilità di default
                query = query.Where(p => p.Name.Contains(search) || p.ProductNumber.Contains(search));
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await query
                .OrderBy(p => p.ProductId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductListDto
                {
                    ProductId = p.ProductId,
                    ProductNumber = p.ProductNumber,
                    Name = p.Name,
                })
                .ToListAsync();

            return new Page<ProductListDto>
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                Items = items
            };
        }

        public async Task<AdminProductEditDto?> GetProductForEditAsync(int productId)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.ProductCategory)
                .Include(p => p.SalesOrderDetails)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
                return null;

            return new AdminProductEditDto
            {
                // General
                ProductId = product.ProductId,
                Name = product.Name,
                ProductNumber = product.ProductNumber,
                ProductCategoryId = product.ProductCategoryId,
                ParentCategoryId = product.ProductCategory?.ParentProductCategoryId,
                ProductModelId = product.ProductModelId,

                // Pricing
                ListPrice = product.ListPrice,
                StandardCost = product.StandardCost,

                // Attributes
                Color = product.Color,
                Size = product.Size,
                Weight = product.Weight,

                // Availability
                SellStartDate = product.SellStartDate,
                SellEndDate = product.SellEndDate,
                DiscontinuedDate = product.DiscontinuedDate,

                // Rules
                HasOrders = product.SalesOrderDetails.Any()
            };
        }


        // To get reference datas (categories and models)
        public async Task<IEnumerable<AdminProductCategoryDto>> GetCategoriesAsync()
        {
            return await _context.ProductCategories
                .AsNoTracking()
                .Select(c => new AdminProductCategoryDto
                {
                    ProductCategoryId = c.ProductCategoryId,
                    Name = c.Name,
                    ParentProductCategoryId = c.ParentProductCategoryId,
                })
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<AdminProductModelDto>> GetModelsAsync()
        {
            return await _context.ProductModels
                .AsNoTracking()
                .Select(m => new AdminProductModelDto
                {
                    ProductModelId = m.ProductModelId,
                    Name = m.Name
                })
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        // To update product
        public async Task UpdateAsync(AdminProductUpdateDto dto)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == dto.ProductId);

            if (product == null)
                throw new KeyNotFoundException("Product not found");

            // General
            product.Name = dto.Name.Trim();
            product.ProductNumber = dto.ProductNumber.Trim();
            product.ProductModelId = dto.ProductModelId;
            product.ProductCategoryId = dto.ProductCategoryId;

            // Pricing
            product.ListPrice = dto.ListPrice;
            product.StandardCost = dto.StandardCost;

            // Attributes
            product.Color = dto.Color?.Trim();
            product.Size = dto.Size?.Trim();
            product.Weight = dto.Weight;

            // Availability
            product.SellStartDate = dto.SellStartDate;
            product.SellEndDate = dto.SellEndDate;
            product.DiscontinuedDate = dto.DiscontinuedDate;

            product.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}
