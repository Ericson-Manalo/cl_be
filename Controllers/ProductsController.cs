using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cl_be.Models;
using cl_be.Models.Dto;
using cl_be.Models.Pagination;

namespace cl_be.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;

        public ProductsController(AdventureWorksLt2019Context context)
        {
            _context = context;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        [HttpGet("paged")]
        public async Task<Page<ProductCardDto>> GetPagedProducts(int pageNumber = 1, int pageSize = 20)
        {
            var totalItems = await _context.Products.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            pageNumber = Math.Clamp(pageNumber, 1, totalPages);

           List<ProductCardDto> items = await _context.Products
                .AsNoTracking()
                .Include(p => p.ProductCategory)
                .OrderBy(p => p.ProductId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductCardDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Color = p.Color,
                    StandardCost = p.StandardCost,
                    ListPrice = p.ListPrice,
                    ProductCategoryId = p.ProductCategoryId,
                    CategoryName = p.ProductCategory != null ? p.ProductCategory.Name : "No category",
                    ThumbNailPhoto = p.ThumbNailPhoto
                })
                .ToListAsync();

            return new Page<ProductCardDto>
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                Items = items
            };
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDetailDto>> GetProduct(int id)
        {
            var productDto = await _context.Products
                .Where(p => p.ProductId == id)
                .Select(p => new ProductDetailDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Color = p.Color,
                    StandardCost = p.StandardCost,
                    ListPrice = p.ListPrice,
                    ProductCategoryId = p.ProductCategoryId,
                    CategoryName = p.ProductCategory != null ? p.ProductCategory.Name : "No category",
                    ThumbNailPhoto = p.ThumbNailPhoto,
                    Size = p.Size,
                    Weight = p.Weight,
                    ProductNumber = p.ProductNumber
                })
                .FirstOrDefaultAsync();

            if (productDto == null)
            {
                return NotFound();
            }

            return Ok(productDto);
        }


        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.ProductId)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProduct", new { id = product.ProductId }, product);
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
