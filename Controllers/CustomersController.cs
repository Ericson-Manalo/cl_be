using cl_be.Models;
using cl_be.Models.Dto.AddressDto;
using cl_be.Models.Dto.CustomerDto;
using cl_be.Models.Dto.OrderDto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cl_be.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;

        public CustomersController(AdventureWorksLt2019Context context)
        {
            _context = context;
        }

        // GET api/customer/1
        [HttpGet("{customerId}")]
        public async Task<ActionResult<CustomerProfileDto>> GetCustomerProfile(int customerId)
        {
            // Recupera il customer
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (customer == null)
                return NotFound("Customer not found");

            // Recupera indirizzi light
            var addresses = await _context.CustomerAddresses
                .Where(ca => ca.CustomerId == customerId)
                .Include(ca => ca.Address)
                .Select(ca => new AddressCustomerDto
                {
                    AddressId = ca.Address.AddressId,
                    AddressLine1 = ca.Address.AddressLine1,
                    City = ca.Address.City,
                    PostalCode = ca.Address.PostalCode
                })
                .ToListAsync();

            // Recupera ordini light
            var orders = await _context.SalesOrderHeaders
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderCustomerDto
                {
                    SalesOrderId = o.SalesOrderId,
                    SalesOrderNumber = o.SalesOrderNumber!,
                    OrderDate = o.OrderDate,
                    TotalDue = o.TotalDue,
                    Status = o.Status
                })
                .ToListAsync();

            // Compila il DTO finale
            var dto = new CustomerProfileDto
            {
                CustomerId = customer.CustomerId,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                EmailAddress = customer.EmailAddress,
                Phone = customer.Phone,
                Addresses = addresses,
                Orders = orders
            };

            return Ok(dto);
        }


        // PUT: api/Customers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer(int id, Customer customer)
        {
            if (id != customer.CustomerId)
            {
                return BadRequest();
            }

            _context.Entry(customer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
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

        // POST: api/Customers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Customer>> PostCustomer(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCustomer", new { id = customer.CustomerId }, customer);
        }

        // DELETE: api/Customers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }
    }
}
