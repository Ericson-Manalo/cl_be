using cl_be.Models.Dto.ProductDto.Admin;
using cl_be.Models.Pagination;
using cl_be.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cl_be.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("api/admin/products")]
    [ApiController]
    public class AdminProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public AdminProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<Page<AdminProductListDto>>> GetPagedProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? search = null)
        {
            var result = await _productService.GetProductsPagedAsync(page, pageSize, search);
            return Ok(result);
        }

        [HttpGet("{productId}")]
        public async Task<ActionResult<AdminProductDetailDto>> GetProductDetail(int productId)
        {
            var detailedProduct = await _productService.GetProductDetailAsync(productId);
            return Ok(detailedProduct);
        }
    }
}
