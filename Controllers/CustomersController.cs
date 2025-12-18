using cl_be.Models.Dto.CustomerDto;
using cl_be.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace cl_be.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        //recupero l'id dal jwt
        private int GetCustomerId()
        {
            var claim = User.FindFirst("CustomerId")?.Value;
            if (claim == null)
                throw new UnauthorizedAccessException();

            return int.Parse(claim);
        }

        // GET api/customer/1
        [HttpGet("profile")]
        public async Task<ActionResult<CustomerProfileDto>> GetCustomerProfile()
        {
            int customerId = GetCustomerId();
            var dto = await _customerService.GetCustomerProfileAsync(customerId);
            return Ok(dto);
        }


        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile(CustomerUpdateDto dto)
        {
            int customerId = GetCustomerId();
            await _customerService.UpdateCustomerAsync(customerId, dto);
            return NoContent();
        }
    }
}
