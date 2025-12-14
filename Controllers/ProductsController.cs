using cl_be.Models;
using cl_be.Models.Dto.CategoryDto;
using cl_be.Models.Dto.ProductDto;
using cl_be.Models.Pagination;
using cl_be.Models.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
            _reviewService = reviewService;
            _context = context;
        }

        // =================================================================================================
        // HELPER METHODS (Logica Ricorsiva Categorie)
        // =================================================================================================

        /// <summary>
        /// Recupera l'ID della categoria passata e tutti gli ID delle sue sottocategorie (ricorsivamente).
        /// Risolve il problema di selezionare una macro-categoria (es. Bikes) e non vedere prodotti.
        /// </summary>
        private async Task<List<int>> GetCategoryAndDescendantsIdsAsync(int rootCategoryId)
        {
            // Recuperiamo tutte le categorie in memoria (tabella piccola, ~40 righe in AdventureWorks)
            // per evitare query ricorsive complesse al DB.
            var allCategories = await _context.ProductCategories
                .AsNoTracking()
                .Select(c => new { c.ProductCategoryId, c.ParentProductCategoryId })
                .ToListAsync();

            var resultIds = new List<int> { rootCategoryId };
            AddChildrenIds(rootCategoryId, allCategories, resultIds);

            return resultIds;
        }

        private void AddChildrenIds(int parentId, dynamic allCategories, List<int> resultIds)
        {
            // Trova i figli diretti
            var childrenIds = ((IEnumerable<dynamic>)allCategories)
                .Where(c => c.ParentProductCategoryId == parentId)
                .Select(c => (int)c.ProductCategoryId)
                .ToList();

            foreach (var childId in childrenIds)
            {
                if (!resultIds.Contains(childId))
                {
                    resultIds.Add(childId);
                    // Ricorsione per trovare i figli dei figli
                    AddChildrenIds(childId, allCategories, resultIds);
                }
            }
        }

        // =================================================================================================
        // ENDPOINTS
        // =================================================================================================

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
            int? categoryId = null,
            string? mainCategory = null
            ) 
        {
            var query = _context.Products
                .AsNoTracking()
                .Include(p => p.ProductCategory)
                .ThenInclude(c => c.ParentProductCategory)
                .AsQueryable();

            // LOGICA CORRETTA: Filtro gerarchico
            if (categoryId.HasValue)
            {
                // Ottieni ID categoria padre + tutti i figli
                var allowedCategoryIds = await GetCategoryAndDescendantsIdsAsync(categoryId.Value);

                query = query.Where(p => p.ProductCategoryId.HasValue &&
                                         allowedCategoryIds.Contains(p.ProductCategoryId.Value));
            }

            // Main category filter (via parent)
            if (!string.IsNullOrWhiteSpace(mainCategory))
            {
                query = query.Where(p =>
                    p.ProductCategory != null &&
                    p.ProductCategory.ParentProductCategory != null &&
                    p.ProductCategory.ParentProductCategory.Name.ToLower() == mainCategory.ToLower()
                );
            }
         
            // Sub category filter
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.ProductCategoryId == categoryId.Value);
            }

            var totalItems = await query.CountAsync();
            var totalPages = totalItems > 0
                ? (int)Math.Ceiling(totalItems / (double)pageSize)
                : 1;

            pageNumber = Math.Clamp(pageNumber, 1, totalPages);

            // Selezione prodotti paginati
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

        [HttpGet("categories")]
        [ProducesResponseType(typeof(List<CategoryDto>), 200)]
        public async Task<ActionResult<List<CategoryDto>>> GetCategories(string? mainCategory = null)
        {
            var query = _context.ProductCategories
                .AsNoTracking()
                .Include(c => c.Products)
                .AsQueryable();

            // Only sub-categories
            query = query.Where(c => c.ParentProductCategoryId != null);

            if (!string.IsNullOrWhiteSpace(mainCategory))
            {
                query = query.Where(c => 
                    c.ParentProductCategory != null &&
                    c.ParentProductCategory.Name.ToLower() == mainCategory.ToLower()
                );
            }

            var categories = await query
                .OrderByDescending(c => c.Products.Count())
                .Select(c => new CategoryDto
                {
                    CategoryId = c.ProductCategoryId,
                    Name = c.Name,
                    ProductCount = c.Products.Count() // Conta solo prodotti diretti
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
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != dto.ProductId) return BadRequest("ID mismatch");

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            if (dto.ProductCategoryId.HasValue &&
                !await _context.ProductCategories.AnyAsync(c => c.ProductCategoryId == dto.ProductCategoryId))
            {
                return BadRequest("Invalid Category ID");
            }

            if (dto.ProductModelId.HasValue &&
                !await _context.ProductModels.AnyAsync(m => m.ProductModelId == dto.ProductModelId))
            {
                return BadRequest("Invalid Product Model ID");
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
                if (!ProductExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // POST: api/Products
        [HttpPost]
        public async Task<ActionResult<ProductDetailDto>> PostProduct([FromBody] ProductCreateDto dto)
        {
            if (dto.ProductCategoryId.HasValue && !await _context.ProductCategories.AnyAsync(c => c.ProductCategoryId == dto.ProductCategoryId))
                return BadRequest("Invalid Category");

            if (dto.ProductModelId.HasValue && !await _context.ProductModels.AnyAsync(m => m.ProductModelId == dto.ProductModelId))
                return BadRequest("Invalid Model");

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
                SellStartDate = dto.SellStartDate,
                SellEndDate = dto.SellEndDate,
                DiscontinuedDate = dto.DiscontinuedDate,
                ModifiedDate = DateTime.UtcNow,
                Rowguid = Guid.NewGuid()
            };

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
                CategoryName = dto.ProductCategoryId.HasValue ? (await _context.ProductCategories.FindAsync(dto.ProductCategoryId))!.Name : "No category",
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
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("bestsellers")]
        public async Task<ActionResult<List<ProductCardDto>>> GetBestSellers(int topN = 20)
        {
            var bestSellersQuery = _context.SalesOrderDetails
                .AsNoTracking()
                .GroupBy(sod => sod.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(sod => sod.OrderQty)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(topN);

            var bestSellers = await bestSellersQuery
                .Join(_context.Products.Include(p => p.ProductCategory),
                      bs => bs.ProductId,
                      p => p.ProductId,
                      (bs, p) => new ProductCardDto
                      {
                          ProductId = p.ProductId,
                          Name = p.Name,
                          ListPrice = p.ListPrice,
                          ProductCategoryId = p.ProductCategoryId,
                          CategoryName = p.ProductCategory!.Name ?? "No category",
                          ThumbNailPhoto = p.ThumbNailPhoto
                      })
                .ToListAsync();

            return Ok(bestSellers);
        }

        [HttpGet("newarrivals")]
        public async Task<ActionResult<List<ProductCardDto>>> GetNewArrivals(int topN = 20)
        {
            var newArrivals = await _context.Products
                .AsNoTracking()
                .Include(p => p.ProductCategory)
                .OrderByDescending(p => p.SellStartDate)
                .Take(topN)
                .Select(p => new ProductCardDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    ListPrice = p.ListPrice,
                    ProductCategoryId = p.ProductCategoryId,
                    CategoryName = p.ProductCategory!.Name ?? "No category",
                    ThumbNailPhoto = p.ThumbNailPhoto
                })
                .ToListAsync();

            return Ok(newArrivals);
        }

        [HttpGet("featured-categories")]
        public async Task<ActionResult<List<CategoryDto>>> GetFeaturedCategories()
        {
            // 1. Recupera tutte le categorie in memoria
            var allCategories = await _context.ProductCategories
                .AsNoTracking()
                .Select(c => new { c.ProductCategoryId, c.Name, c.ParentProductCategoryId })
                .ToListAsync();

            // 2. Recupera tutti i prodotti (solo ID e CategoryID) per contare in memoria
            // Questo evita query N+1 ed è veloce su dataset AdventureWorks
            var allProducts = await _context.Products
                .AsNoTracking()
                .Select(p => new { p.ProductId, p.ProductCategoryId })
                .ToListAsync();

            // 3. Filtra solo le categorie "root" (Macro Categorie)
            var rootCategories = allCategories
                .Where(c => c.ParentProductCategoryId == null)
                .ToList();

            var result = new List<CategoryDto>();

            foreach (var rootCat in rootCategories)
            {
                // Trova tutti gli ID discendenti per questa root
                var descendantIds = new List<int> { rootCat.ProductCategoryId };
                AddChildrenIds(rootCat.ProductCategoryId, allCategories, descendantIds);

                // Conta i prodotti che appartengono a uno qualsiasi di questi ID
                var count = allProducts.Count(p => p.ProductCategoryId.HasValue && descendantIds.Contains(p.ProductCategoryId.Value));

                result.Add(new CategoryDto
                {
                    CategoryId = rootCat.ProductCategoryId,
                    Name = rootCat.Name,
                    ProductCount = count
                });
            }

            return Ok(result.OrderByDescending(c => c.ProductCount).ToList());
        }

        [HttpGet("categoryV2/{categoryId}")]
        public async Task<ActionResult<List<ProductCardDto>>> GetProductsByCategoryV2(int categoryId)
        {
            // Aggiornato per usare la stessa logica robusta del metodo paged
            var allowedCategoryIds = await GetCategoryAndDescendantsIdsAsync(categoryId);

            var products = await _context.Products
                .AsNoTracking()
                .Include(p => p.ProductCategory)
                .Where(p => p.ProductCategoryId.HasValue && allowedCategoryIds.Contains(p.ProductCategoryId.Value))
                .Select(p => new ProductCardDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    ListPrice = p.ListPrice,
                    ProductCategoryId = p.ProductCategoryId ?? 0,
                    CategoryName = p.ProductCategory!.Name ?? "No category",
                    ThumbNailPhoto = p.ThumbNailPhoto
                })
                .ToListAsync();

            return Ok(products);
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}

//[HttpGet("category/{categoryId}")]
//public async Task<ActionResult<List<ProductCardDto>>> GetProductsByCategory(int categoryId)
//{
//    var products = await _context.Products
//        .AsNoTracking()
//        .Include(p => p.ProductCategory)
//        .Where(p => p.ProductCategoryId == categoryId)
//        .Select(p => new ProductCardDto
//        {
//            ProductId = p.ProductId,
//            Name = p.Name,
//            ListPrice = p.ListPrice,
//            ProductCategoryId = p.ProductCategoryId,
//            CategoryName = p.ProductCategory!.Name ?? "No category",
//            ThumbNailPhoto = p.ThumbNailPhoto
//        })
//        .ToListAsync();

//    return Ok(products);
//}