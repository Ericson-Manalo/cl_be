using cl_be.Models;
using cl_be.Models.Dto.ProductDto;
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
        /// Admin: get product list
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductListDto>>> GetProducts(
            int pageNumber = 1,
            int pageSize = 20,
            string? sortBy = null,
            string? sortDirection = "asc")
        {
            var result = await _adminProductService.GetProductsAsync(pageNumber, pageSize, sortBy, sortDirection);
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
        public async Task<ActionResult<IEnumerable<AdminCategoryDto>>> GetCategories()
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


    }
}
