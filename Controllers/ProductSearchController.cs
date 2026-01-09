using cl_be.Models;
using cl_be.Models.Dto.ProductDto;
using cl_be.Models.Pagination;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cl_be.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowFrontEnd")]
    public class ProductSearchController(AdventureWorksLt2019Context context) : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context = context;


        [HttpGet]
        public async Task<ActionResult<Page<ProductCardDto>>> SearchProducts(
            string? query = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int pageNumber = 1,
            int pageSize = 20
        )
        {
            var productsQuery = _context.Products
                .AsNoTracking()
                .Include(p => p.ProductCategory)
                .AsQueryable();

            if (!string.IsNullOrEmpty(query))
                productsQuery = productsQuery.Where(p => p.Name.ToLower().Contains( query.ToLower()));


            if (minPrice.HasValue)
                productsQuery = productsQuery.Where(p => p.ListPrice >= minPrice);

            if (maxPrice.HasValue)
                productsQuery = productsQuery.Where(p => p.ListPrice <= maxPrice);

            var totalItems = await productsQuery.CountAsync();
            var totalPages = totalItems > 0
                ? (int)Math.Ceiling(totalItems / (double)pageSize)
                : 1;

            pageNumber = Math.Clamp(pageNumber, 1, totalPages);

            var items = await productsQuery
                .OrderBy(p => p.ProductId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductCardDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    ListPrice = p.ListPrice,
                    ProductCategoryId = p.ProductCategoryId,
                    CategoryName = p.ProductCategory != null ? p.ProductCategory.Name : "No category",
                    ThumbNailPhoto = p.ThumbNailPhoto
                })
                .ToListAsync();

            return Ok(new Page<ProductCardDto>
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                Items = items
            });
        }





    }
}
