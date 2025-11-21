using cl_be.Models;
using cl_be.Models.Dto.CategoryDto;
using cl_be.Models.Dto.ProductDto;
using cl_be.Models.Pagination;
using cl_be.Models.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cl_be.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowFrontEnd")]
    public class ProductsController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;

        private readonly ReviewService _reviewService;

        public ProductsController(
            AdventureWorksLt2019Context context, ReviewService reviewService)
        {
            _reviewService= reviewService;
            _context = context;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }
        [HttpGet("paged")]
        public async Task<Page<ProductCardDto>> GetPagedProducts(
            int pageNumber = 1,
            int pageSize = 20,
            int? categoryId = null) 
        {
            var query = _context.Products
                .AsNoTracking()
                .Include(p => p.ProductCategory)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.ProductCategoryId == categoryId.Value);
            }

         
            var totalItems = await query.CountAsync();
            var totalPages = totalItems > 0
                ? (int)Math.Ceiling(totalItems / (double)pageSize)
                : 1;

            pageNumber = Math.Clamp(pageNumber, 1, totalPages);

            // items paginati con filtro
            List<ProductCardDto> items = await query
                .OrderBy(p => p.ProductId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductCardDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    ListPrice = p.ListPrice,
                    ProductCategoryId = p.ProductCategoryId,
                    CategoryName = p.ProductCategory != null
                        ? p.ProductCategory.Name
                        : "No category",
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

        [HttpGet("categories")]
        [ProducesResponseType(typeof(List<CategoryDto>), 200)]
        public async Task<ActionResult<List<CategoryDto>>> GetCategories()
        {
            var categories = await _context.ProductCategories
                .AsNoTracking()
                .OrderBy(c => c.Products.Count())
                .Select(c => new CategoryDto
                {
                    CategoryId = c.ProductCategoryId,
                    Name = c.Name,
                    ProductCount = c.Products.Count()
                })
                .ToListAsync();

            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDetailDto>> GetProduct(int id)
        {
            var product = await _context.Products
                .Where(p => p.ProductId == id)
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductModel)
                    .ThenInclude(pm => pm!.ProductModelProductDescriptions)
                        .ThenInclude(pmpd => pmpd.ProductDescription)
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound(new { message = $"Product with ID {id} not found" });
            }

            var productDto = new ProductDetailDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Color = product.Color,
                StandardCost = product.StandardCost,
                ListPrice = product.ListPrice,
                ProductCategoryId = product.ProductCategoryId,
                CategoryName = product.ProductCategory?.Name ?? "No category",
                ThumbNailPhoto = product.ThumbNailPhoto,
                Size = product.Size,
                Weight = product.Weight,
                ProductNumber = product.ProductNumber,

                Descriptions = product.ProductModel?.ProductModelProductDescriptions
                    .ToDictionary(
                        pmpd => pmpd.Culture.Trim(),
                        pmpd => pmpd.ProductDescription.Description
                    ) ?? new Dictionary<string, string>(),

                Reviews = await _reviewService.GetReviewsForProduct(id)
            };

            return Ok(productDto);
        }



        // PUT: api/Products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, [FromBody] ProductUpdateDto dto)
        {
            // Controlla validità modello (includendo annotazioni)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != dto.ProductId)
            {
                return BadRequest("ID nel percorso e nel body non coincidono.");
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            if (dto.ProductCategoryId.HasValue &&
                !await _context.ProductCategories.AnyAsync(c => c.ProductCategoryId == dto.ProductCategoryId))
            {
                return BadRequest("Categoria prodotto non valida.");
            }

            if (dto.ProductModelId.HasValue &&
                !await _context.ProductModels.AnyAsync(m => m.ProductModelId == dto.ProductModelId))
            {
                return BadRequest("Modello prodotto non valido.");
            }

            product.Name = dto.Name;
            product.ProductNumber = dto.ProductNumber;
            product.StandardCost = dto.StandardCost;
            product.ListPrice = dto.ListPrice;
            product.ProductCategoryId = dto.ProductCategoryId;
            product.ProductModelId = dto.ProductModelId;
            product.Color = dto.Color;
            product.Size = dto.Size;
            product.Weight = dto.Weight;
            product.SellStartDate = dto.SellStartDate;
            product.SellEndDate = dto.SellEndDate;
            product.DiscontinuedDate = dto.DiscontinuedDate;
            product.ModifiedDate = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Products.Any(e => e.ProductId == id))
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
        [HttpPost]
        public async Task<ActionResult<ProductDetailDto>> PostProduct([FromBody] ProductCreateDto dto)
        {
            if (dto.ProductCategoryId.HasValue && !await _context.ProductCategories.AnyAsync(c => c.ProductCategoryId == dto.ProductCategoryId))
            {
                return BadRequest("Categoria prodotto non valida.");
            }

            // Validazione FK ProductModelId
            if (dto.ProductModelId.HasValue && !await _context.ProductModels.AnyAsync(m => m.ProductModelId == dto.ProductModelId))
            {
                return BadRequest("Modello prodotto non valido.");
            }

            var product = new Product
            {
                Name = dto.Name,
                ProductNumber = dto.ProductNumber,
                StandardCost = dto.StandardCost,
                ListPrice = dto.ListPrice,
                ProductCategoryId = dto.ProductCategoryId,
                ProductModelId = dto.ProductModelId,
                Color = dto.Color,
                Size = dto.Size,
                Weight = dto.Weight,
                //ThumbNailPhoto = dto.ThumbNailPhoto,
                //ThumbnailPhotoFileName = dto.ThumbnailPhotoFileName,
                SellStartDate = dto.SellStartDate,
                SellEndDate = dto.SellEndDate,
                DiscontinuedDate = dto.DiscontinuedDate,
                ModifiedDate = DateTime.UtcNow,
                Rowguid = Guid.NewGuid()
            };

            // Aggiungi e salva
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            
            var result = new ProductDetailDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Color = product.Color,
                StandardCost = product.StandardCost,
                ListPrice = product.ListPrice,
                ProductCategoryId = product.ProductCategoryId,
                CategoryName = dto.ProductCategoryId.HasValue ? (await _context!.ProductCategories!.FindAsync(dto.ProductCategoryId))!.Name : "No category",
                ThumbNailPhoto = product.ThumbNailPhoto,
                Size = product.Size,
                Weight = product.Weight,
                ProductNumber = product.ProductNumber


            };

            return CreatedAtAction(nameof(GetProduct), new { id = product.ProductId }, result);
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
