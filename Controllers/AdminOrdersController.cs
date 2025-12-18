using cl_be.Models;
using cl_be.Models.Dto.CustomerDto;
using cl_be.Models.Dto.OrderDto;
using cl_be.Models.Pagination;
using cl_be.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cl_be.Controllers
{
    [Authorize]
    [Route("api/admin/orders")]
    [ApiController]
    public class AdminOrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public AdminOrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET: api/AdminOrders
        //[HttpGet]
        //public async Task<ActionResult<OrderListDto>> GetAllOrders()
        //{
        //    var orderList = await _orderService.GetAllOrdersAsync();

        //    return Ok(orderList);
        //}

        [HttpGet]
        public async Task<ActionResult<Page<OrderListDto>>> GetAllOrdersPaged(int page = 1, int pageSize = 50, string? search = null)
        {
            var orderList = await _orderService.GetOrdersPagedAsync(page, pageSize, search);

            return Ok(orderList);
        }

        [HttpGet("{orderId}")]
        public async Task<ActionResult<OrderDetailDto>> GetOrderDetail(int orderId)
        {
            var orderDetail = await _orderService.GetDetailOrder(orderId);

            if (orderDetail == null)
            {
                return NotFound($"Ordine con ID {orderId} non trovato.");
            }

            return Ok(orderDetail);
        }

        [HttpDelete("{orderId}")]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            var deleted = await _orderService.DeleteOrderAsync(orderId);

            if (!deleted)
            {
                return NotFound($"Impossibile trovare l'ordine {orderId}");
            }

            return NoContent(); // Ritorna 204 se l'operazione è riuscita
        }


        [HttpPatch("{orderId}/status")]
        public async Task<IActionResult> UpdateStatus(int orderId, [FromBody] OrderStatusUpdateDto dto)
        {
            if (dto.Status <= 0) return BadRequest("Stato non valido");

            var result = await _orderService.UpdateOrderStatusAsync(orderId, dto.Status);

            if (!result) return NotFound($"Ordine {orderId} non trovato");

            return NoContent(); // Ritorna 204 Success
        }
    }
}
