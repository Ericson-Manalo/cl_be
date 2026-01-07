using cl_be.Models;
using cl_be.Models.Dto.CustomerDto;
using cl_be.Models.Dto.ProductDto;
using cl_be.Models.Pagination;
using cl_be.Services;
using cl_be.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cl_be.Controllers.Admin
{
    [Route("api/admin/products")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminProductsController : ControllerBase
    {
        private readonly IAdminProductService _adminProductService;

        public AdminProductsController(IAdminProductService adminProductService)
        {
            _adminProductService = adminProductService;
        }

        /// <summary>
        /// Admin: get product list. THIS IS FOR TABLE LIST SET UP
        /// </summary>
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<ProductListDto>>> GetProducts(
        //    int pageNumber = 1,
        //    int pageSize = 20,
        //    string? sortBy = null,
        //    string? sortDirection = "asc")
        //{
        //    var result = await _adminProductService.GetProductsAsync(pageNumber, pageSize, sortBy, sortDirection);
        //    return Ok(result);
        //}

        [HttpGet]
        public async Task<ActionResult<Page<ProductListDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string? search = null)
        {
            var result = await _adminProductService.GetAllProductsAsync(page, pageSize, search);
            return Ok(result);
        }

        /// <summary>
        /// Admin: get product for edit/create form
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AdminProductEditDto>> GetProduct(int id)
        {
            var product = await _adminProductService.GetProductForEditAsync(id);

            if (product == null)
                return NotFound();

            return Ok(product);
        }

        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<AdminProductCategoryDto>>> GetCategories()
        {
            var categories = await _adminProductService.GetCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("models")]
        public async Task<ActionResult<IEnumerable<AdminProductModelDto>>> GetModels()
        {
            var models = await _adminProductService.GetModelsAsync();
            return Ok(models);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(
        int id,
        [FromBody] AdminProductUpdateDto dto)
        {
            /* 1. Route vs Body guard */
            if (id != dto.ProductId)
                return BadRequest("ProductId mismatch between route and body.");

            /* 2. Model validation (DTO rules) */
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                /* 3. Delegate to service layer */
                await _adminProductService.UpdateAsync(dto);
                return NoContent(); // 204
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

    }
}
