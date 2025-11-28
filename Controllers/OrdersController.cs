using cl_be.Models;
using cl_be.Models.Dto.AddressDto;
using cl_be.Models.Dto.OrderDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace cl_be.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {

        private readonly AdventureWorksLt2019Context _context;

        public OrdersController(AdventureWorksLt2019Context context)
        {
            _context = context;
        }

        // GET: api/<OrdersController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<OrdersController>/5
        //[Authorize]
        [HttpGet("{orderId}")]
        public async Task<ActionResult<OrderDetailDto>> GetOrderDetail(int orderId)
        {
            // Recupero ordine + i due indirizzi + metodo spedizione
            var order = await _context.SalesOrderHeaders
                .Include(o => o.BillToAddress)
                .Include(o => o.ShipToAddress)
                .FirstOrDefaultAsync(o => o.SalesOrderId == orderId);

            if (order == null)
                return NotFound("Order not found");


            //// Recupero CustomerId dal JWT
            //var customerIdClaim = User.FindFirst("CustomerId")?.Value;
            //if (customerIdClaim == null)
            //    return Unauthorized("Invalid token");

            //int customerId = int.Parse(customerIdClaim);

            //// Controllo che l'ordine appartenga al cliente
            //if (order.CustomerId != customerId)
            //    return Forbid("This order does not belong to the authenticated user");

            // Recupero gli items dell’ordine
            var items = await _context.SalesOrderDetails
                .Where(i => i.SalesOrderId == orderId)
                .Include(i => i.Product)
                .Select(i => new OrderDetailItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product!.Name,
                    OrderQty = i.OrderQty,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.LineTotal
                })
                .ToListAsync();

            // Creo DTO completo
            var dto = new OrderDetailDto
            {
                SalesOrderId = order.SalesOrderId,
                SalesOrderNumber = order.SalesOrderNumber!,
                OrderDate = order.OrderDate,
                ShipDate = order.ShipDate,
                Status = order.Status,
                ShipMethod = order.ShipMethod!,
                SubTotal = order.SubTotal,
                TaxAmt = order.TaxAmt,
                Freight = order.Freight,
                TotalDue = order.TotalDue,

                BillToAddress = order.BillToAddress != null
                    ? new AddressesDetailDto
                    {
                        AddressId = order.BillToAddress.AddressId,
                        AddressLine1 = order.BillToAddress.AddressLine1!,
                        City = order.BillToAddress.City!,
                        PostalCode = order.BillToAddress.PostalCode!
                    }
                    : null,

                ShipToAddress = order.ShipToAddress != null
                    ? new AddressesDetailDto
                    {
                        AddressId = order.ShipToAddress.AddressId,
                        AddressLine1 = order.ShipToAddress.AddressLine1!,
                        City = order.ShipToAddress.City!,
                        PostalCode = order.ShipToAddress.PostalCode!
                    }
                    : null,

                Items = items
            };

            return Ok(dto);
        }




        // POST api/<OrdersController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<OrdersController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<OrdersController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
