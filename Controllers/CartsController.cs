using cl_be.Interfaces.IServices;
using cl_be.Models;
using cl_be.Models.Dto.CartDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace cl_be.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartsController(ICartService cartService)
        {
            _cartService = cartService;
        }


        private int GetAuthenticatedCustomerId()
        {
            var customerIdClaim = User.FindFirst("CustomerId")?.Value;

            if (string.IsNullOrEmpty(customerIdClaim) || !int.TryParse(customerIdClaim, out int customerId))
            {
                throw new UnauthorizedAccessException("Invalid or missing CustomerId in token.");
            }

            return customerId;
        }

        // ========================================
        // CUSTOMER CART OPERATIONS
        // ========================================

        /// <summary>
        /// Get or create the authenticated user's cart
        /// </summary>
        [HttpGet("my-cart")]
        [Authorize]
        public async Task<ActionResult<CartResponseDTO>> GetMyCart()
        {
            try
            {
                var customerId = GetAuthenticatedCustomerId();
                var cart = await _cartService.GetOrCreateCartAsync(customerId);
                return Ok(cart);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", details = ex.Message });
            }
        }

        /// <summary>
        /// Add a product to the cart
        /// </summary>
        [HttpPost("items")]
        [Authorize]
        public async Task<ActionResult<CartResponseDTO>> AddToCart([FromBody] AddToCartDto addToCartDto)
        {

            try
            {
                var customerId = GetAuthenticatedCustomerId();

                var cart = await _cartService.AddToCartAsync(customerId, addToCartDto);

                

                return Ok(cart);



            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                // Prodotto non esiste
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                // Validazione parametri 
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Errore generico/DB
                return StatusCode(500, new { message = "An error occurred while adding to cart.", details = ex.Message });
            }

        }

        /// <summary>
        /// Update the quantity of a cart item
        /// </summary>
        [HttpPut("items/{cartItemId}")]
        [Authorize]
        public async Task<ActionResult<CartResponseDTO>> UpdateCartItem(int cartItemId, [FromBody] UpdateCartDto updateCartDto)
        {

            try
            {

                var customerId = GetAuthenticatedCustomerId();

                var cart = await _cartService.UpdateCartItemAsync(customerId, cartItemId, updateCartDto);

                return Ok(cart);

            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                // Cart item non esiste
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                // Validazione parametri 
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Errore generico/DB
                return StatusCode(500, new { message = "An error occurred while updating cart item.", details = ex.Message });
            }


        }

        /// <summary>
        /// Remove an item from the cart
        /// </summary>
        [HttpDelete("items/{cartItemId}")]
        [Authorize]
        public async Task<ActionResult<CartResponseDTO>> RemoveFromCart(int cartItemId)
        {

            try
            {

                var customerId = GetAuthenticatedCustomerId();

                var cart = await _cartService.RemoveFromCartAsync(customerId, new RemoveFromCartDto { CartItemId = cartItemId });

                return Ok(cart);

            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                // Cart item non esiste
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                // Validazione parametri 
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Errore generico/DB
                return StatusCode(500, new { message = "An error occurred while updating cart item.", details = ex.Message });
            }

        }

        /// <summary>
        /// Clear all items from the cart
        /// </summary>
        [HttpDelete("clear")]
        [Authorize]
        public async Task<ActionResult<CartResponseDTO>> ClearCart()
        {
            try
            {

                var customerId = GetAuthenticatedCustomerId();

                var cart = await _cartService.ClearCartAsync(customerId);

                return Ok(cart);

            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                // Cart item non esiste
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                // Validazione parametri 
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Errore generico/DB
                return StatusCode(500, new { message = "An error occurred while updating cart item.", details = ex.Message });
            }
        }

        // ========================================
        // CART INFORMATION & VALIDATION
        // ========================================

        /// <summary>
        /// Get the total amount of the cart
        /// </summary>
        [HttpGet("total")]
        [Authorize]
        public async Task<ActionResult<decimal>> GetCartTotal()
        {
            try
            {

                var customerId = GetAuthenticatedCustomerId();

                var total = await _cartService.GetCartTotalAsync(customerId);

                return Ok(total);

            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                // Cart item non esiste
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                // Validazione parametri 
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Errore generico/DB
                return StatusCode(500, new { message = "An error occurred while updating cart item.", details = ex.Message });
            }
        }

        /// <summary>
        /// Get the total number of items in the cart
        /// </summary>
        [HttpGet("items/count")]
        [Authorize]
        public async Task<ActionResult<int>> GetCartItemsCount()
        {
            try
            {

                var customerId = GetAuthenticatedCustomerId();

                var itemCount = await _cartService.GetCartItemsCountAsync(customerId);

                return Ok(itemCount);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                // Cart item non esiste
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                // Validazione parametri 
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Errore generico/DB
                return StatusCode(500, new { message = "An error occurred while updating cart item.", details = ex.Message });
            }
        }

        /// <summary>
        /// Check if a product exists in the cart
        /// </summary>
        [HttpGet("products/{productId}/exists")]
        [Authorize]
        public async Task<ActionResult<bool>> IsProductInCart(int productId)
        {
            try
            {

                var customerId = GetAuthenticatedCustomerId();

                var exists = await _cartService.IsProductInCartAsync(customerId, productId);

                return Ok(exists);


            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                // Cart item non esiste
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                // Validazione parametri 
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Errore generico/DB
                return StatusCode(500, new { message = "An error occurred while updating cart item.", details = ex.Message });
            }
        }

        // ========================================
        // ADMIN OPERATIONS
        // ========================================

        /// <summary>
        /// Get all active carts (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<CartResponseDTO>>> GetAllActiveCarts()
        {
            try
            {

                var carts = await _cartService.GetAllActiveCartsAsync();

                return Ok(carts);

            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                // Cart item non esiste
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                // Validazione parametri 
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Errore generico/DB
                return StatusCode(500, new { message = "An error occurred while updating cart item.", details = ex.Message });
            }
        }

        /// <summary>
        /// Get a specific cart by ID (Admin only)
        /// </summary>
        [HttpGet("{cartId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CartResponseDTO>> GetCartById(int cartId)
        {

            try
            {

                var cart = await _cartService.GetCartByIdAsync(cartId);

                if (cart == null)
                    return NotFound(new { message = $"Cart with ID {cartId} not found." });

                return Ok(cart);


            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                // Cart item non esiste
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                // Validazione parametri 
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Errore generico/DB
                return StatusCode(500, new { message = "An error occurred while updating cart item.", details = ex.Message });
            }

        }

        /// <summary>
        /// Get all carts for a specific customer (Admin only)
        /// </summary>
        [HttpGet("customer/{customerId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<CartResponseDTO>>> GetCustomerCarts(int customerId)
        {
            try
            {

                var carts = await _cartService.GetCustomerCartsAsync(customerId);
                return Ok(carts);

            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                // Cart item non esiste
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                // Validazione parametri 
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Errore generico/DB
                return StatusCode(500, new { message = "An error occurred while updating cart item.", details = ex.Message });
            }
        }

        /// <summary>
        /// Delete a cart by ID (Admin only)
        /// </summary>
        [HttpDelete("{cartId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteCart(int cartId)
        {

            try
            {

                var result = await _cartService.DeleteCartByIdAsync(cartId);

                if (!result)
                    return NotFound(new { message = $"Cart with ID {cartId} not found." });

                return Ok(new { message = "Cart deleted successfully." });

            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                // Cart item non esiste
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                // Validazione parametri 
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Errore generico/DB
                return StatusCode(500, new { message = "An error occurred while updating cart item.", details = ex.Message });
            }

        }
    }
}