using cl_be.Models;
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
    [Route("api/[controller]")]
    [ApiController]
    public class CartItemsController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;

        public CartItemsController(AdventureWorksLt2019Context context)
        {
            _context = context;
        }

        [HttpGet("{customerId}")]
        [Authorize(Policy="UserPolicy")]
        public async Task<IActionResult> GetCart(int customerId)
        {
            var cart = await _context.CartItems
                .Where(c => c.CustomerId == customerId)
                .Include(c => c.Product)
                .ToListAsync();

            return Ok(cart);
        }


    }
}
