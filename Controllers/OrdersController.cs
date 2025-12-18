using cl_be.Models;
using cl_be.Models.Dto.AddressDto;
using cl_be.Models.Dto.OrderDto;
using cl_be.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace cl_be.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Proteggiamo l'intero controller
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("{orderId}")]
        public async Task<ActionResult<OrderDetailDto>> GetOrderDetail(int orderId)
        {
            // 1. Estrazione ID dal Token (Unica logica che resta nel controller)
            var customerIdClaim = User.FindFirst("CustomerId")?.Value;
            if (string.IsNullOrEmpty(customerIdClaim)) return Unauthorized("Token non valido");

            int customerId = int.Parse(customerIdClaim);

            // 2. Chiamata al Service
            var dto = await _orderService.GetCustomerOrderDetailAsync(orderId, customerId);

            if (dto == null)
                return NotFound("Ordine non trovato o non appartenente a questo utente");

            return Ok(dto);
        }
    }
}
