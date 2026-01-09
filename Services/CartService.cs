using cl_be.Models;
using cl_be.Models.Dto.CartDto;
using cl_be.Services.Interfaces;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace cl_be.Services
{
    public class CartService : ICartService
    {

        private readonly AdventureWorksLt2019Context _context;

         public CartService (AdventureWorksLt2019Context context)
         {
                _context = context;
         }

        public async Task<CartResponseDTO> AddToCartAsync(int customerId, AddToCartDto addToCartDto)
        {
            // Validazioni
            if (customerId <= 0)
                throw new ArgumentException("Invalid Customer ID.");
            if (addToCartDto.ProductId <= 0)
                throw new ArgumentException("Invalid Product ID.");
            if (addToCartDto.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.");

            // Verifica prodotto
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == addToCartDto.ProductId);

            if (product == null)
                throw new KeyNotFoundException("Product does not exist.");

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart == null)
            {
                cart = new Cart
                {
                    CustomerId = customerId,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow,
                    Items = new List<CartItem>()
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // Aggiungi o aggiorna item
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == addToCartDto.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += addToCartDto.Quantity;
                _context.CartItems.Update(existingItem);
            }
            else
            {
                var newItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = addToCartDto.ProductId,
                    Quantity = addToCartDto.Quantity,
                    AddedDate = DateTime.UtcNow
                };
                cart.Items.Add(newItem);
                _context.CartItems.Add(newItem);
            }

            cart.ModifiedDate = DateTime.UtcNow;
            _context.Carts.Update(cart);

            // Salva tutto
            await _context.SaveChangesAsync();

            cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.CartId == cart.CartId);

            var response = new CartResponseDTO
            {
                CartId = cart.CartId,
                Items = cart.Items.Select(i => new CartItemDTO
                {
                    CartItemId = i.CartItemId,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,      
                    Price = i.Product.ListPrice,
                    Quantity = i.Quantity,
                    ThumbnailPhotoFileName = i.Product.ThumbNailPhoto,
                    AddedDate = i.AddedDate
                }).ToList()
            };

            return response;
        }

        public async Task<CartResponseDTO> ClearCartAsync(int customerId)
        {

            if(customerId <= 0)
                throw new ArgumentException("Invalid Customer ID.");

            var cart = await _context.Carts
             .Include(c => c.Items)
             .FirstOrDefaultAsync(c => c.CustomerId == customerId);


            if (cart == null)
            {

                return new CartResponseDTO
                {
                    CartId = 0,
                    Items = []
                };

            }

            if (cart.Items.Any())
            {
                _context.CartItems.RemoveRange(cart.Items); 

               
                cart.ModifiedDate = DateTime.UtcNow;
                _context.Carts.Update(cart);

                // Salva modifiche
                await _context.SaveChangesAsync();
            }

            //  Ritorna cart vuoto
            return new CartResponseDTO
            {
                CartId = cart.CartId,
                Items = new List<CartItemDTO>() 
            };

        }

        public async Task<bool> DeleteCartAsync(int customerId)
        {

            if (customerId <= 0)
                throw new ArgumentException("Invalid Customer ID.");



            var cart = await _context.Carts
        .Include(c => c.Items)
        .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart == null)
                return false;

            if (cart.Items.Any())
            {
                _context.CartItems.RemoveRange(cart.Items);
            }

            _context.Carts.Remove(cart);  
            await _context.SaveChangesAsync();
            return true;

        }

        public async Task<bool> DeleteCartByIdAsync(int cartId)
        {
            if (cartId <= 0)
                throw new ArgumentException("Invalid Cart ID.");

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CartId == cartId);

            if (cart == null)
                return false;  

         
            if (cart.Items.Any())
            {
                _context.CartItems.RemoveRange(cart.Items);
            }

            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
            return true;

        }


        public async Task<List<CartResponseDTO>> GetAllActiveCartsAsync()
        {

            var carts = await _context.Carts
                 .Include(c => c.Customer)
                 .Include(c => c.Items)
                     .ThenInclude(i => i.Product)
                 .OrderByDescending(c => c.ModifiedDate)
                 .ToListAsync();

            if (carts == null || !carts.Any())
                return[];

            var response = carts.Select(cart => new CartResponseDTO
            {
                CartId = cart.CartId,
                Items = cart.Items.Select(i => new CartItemDTO
                {
                    CartItemId = i.CartItemId,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    Price = i.Product.ListPrice,
                    Quantity = i.Quantity,
                    ThumbnailPhotoFileName = i.Product.ThumbNailPhoto,
                    AddedDate = i.AddedDate
                }).ToList()
            }).ToList();

            return response;

        }

        public async Task<CartResponseDTO?> GetCartByIdAsync(int cartId)
        {

            if (cartId <= 0)
                throw new ArgumentException("Invalid Cart ID.");

            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.CartId == cartId);

            if (cart == null)
                return null;


            var response = new CartResponseDTO
            {
                CartId = cart.CartId,
                Items = cart.Items.Select(i => new CartItemDTO
                {
                    CartItemId = i.CartItemId,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    Price = i.Product.ListPrice,
                    Quantity = i.Quantity,
                    ThumbnailPhotoFileName = i.Product.ThumbNailPhoto,
                    AddedDate = i.AddedDate
                }).ToList()
            };

            return response;


        }
        

        public async Task<int> GetCartItemsCountAsync(int customerId)
        {
            if (customerId <= 0)
                throw new ArgumentException("Invalid Customer ID.");

            var itemCount = await _context.CartItems
              .Where(ci => ci.Cart.CustomerId == customerId)
              .SumAsync(ci => ci.Quantity);

            if (itemCount == null)
                itemCount = 0;


            return itemCount;

        }

        public async Task<decimal> GetCartTotalAsync(int customerId)
        {

            if (customerId <= 0)
                throw new ArgumentException("Invalid Customer ID.");

            var total = await _context.CartItems
              .Where(ci => ci.Cart.CustomerId == customerId)
              .SumAsync(ci => ci.Quantity * ci.Product.ListPrice);

            return total;


        }

        public async Task<List<CartResponseDTO>> GetCustomerCartsAsync(int customerId)
        {
            
            if (customerId <= 0)
                throw new ArgumentException("Invalid Customer ID.");

            var carts = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .Where(c => c.CustomerId == customerId)
                .OrderByDescending(c => c.ModifiedDate)
                .ToListAsync();


            if (carts == null || !carts.Any())
                return [];

            var response = carts.Select(cart => new CartResponseDTO
            {
                CartId = cart.CartId,
                Items = cart.Items.Select(i => new CartItemDTO
                {
                    CartItemId = i.CartItemId,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    Price = i.Product.ListPrice,
                    Quantity = i.Quantity,
                    ThumbnailPhotoFileName = i.Product.ThumbNailPhoto,
                    AddedDate = i.AddedDate
                }).ToList()
            }).ToList();


            return response;


        }

        public async Task<CartResponseDTO> GetOrCreateCartAsync(int customerId)
        {

            if (customerId <= 0)
                throw new ArgumentException("Invalid Customer ID.");


            var cart = await _context.Carts
              .Include(c => c.Items)
              .ThenInclude(i => i.Product)
              .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart == null)
            {
                cart = new Cart
                {
                    CustomerId = customerId,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow,
                    Items = new List<CartItem>()  
                };

                _context.Carts.Add(cart);

                await _context.SaveChangesAsync();  
            }


            var response = new CartResponseDTO
            {
                CartId = cart.CartId,
                Items = cart.Items.Select(i => new CartItemDTO
                {
                    CartItemId = i.CartItemId,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    Price = i.Product.ListPrice,
                    Quantity = i.Quantity,
                    ThumbnailPhotoFileName = i.Product.ThumbNailPhoto,
                    AddedDate = i.AddedDate

                }).ToList()
            };

            return response;     
        }

        public async Task<bool> IsProductInCartAsync(int customerId, int productId)
        {

            if (customerId <= 0)
                throw new ArgumentException("Invalid Customer ID.");

            var exists = await _context.CartItems
                .AnyAsync(ci => ci.Cart.CustomerId== customerId && ci.ProductId == productId);

            if (exists)
                return true;
            else
                return false;

        }

        public async Task<CartResponseDTO> RemoveFromCartAsync(int customerId, RemoveFromCartDto removeFromCartDto)
        {

            if (customerId <= 0)           
                throw new ArgumentException("Invalid Customer ID.");

            if (removeFromCartDto.CartItemId <= 0)           
                throw new ArgumentException("Invalid Cart Item ID.");   



            var cart =  await  _context.Carts
             .Include(c => c.Items)
             .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart == null)
                throw new KeyNotFoundException("Cart not found for the customer.");

            var itemToRemove = cart.Items
                .FirstOrDefault(i => i.CartItemId == removeFromCartDto.CartItemId);

            if (itemToRemove == null)
                throw new KeyNotFoundException($"Item {removeFromCartDto.CartItemId} not found in your cart.");



            _context.CartItems.Remove(itemToRemove);

            cart.ModifiedDate = DateTime.UtcNow;

            _context.Carts.Update(cart);

            await _context.SaveChangesAsync();


            return await GetOrCreateCartAsync(customerId);


        }

        public async Task<CartResponseDTO> UpdateCartItemAsync(int customerId, int cartItemId, UpdateCartDto updateCartDto)
        {
            if (customerId <= 0)
                throw new ArgumentException("Invalid Customer ID.");
            if (cartItemId <= 0)
                throw new ArgumentException("Invalid Cart Item ID.");

            if (updateCartDto.Quantity < 0)
                throw new ArgumentException("Quantity cannot be negative.");

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart == null)
                throw new KeyNotFoundException("Cart not found for the customer.");

            var itemToUpdate = cart.Items
                .FirstOrDefault(i => i.CartItemId == cartItemId);

            if (itemToUpdate == null)
                throw new KeyNotFoundException($"Item {cartItemId} not found in your cart.");

            if (updateCartDto.Quantity == 0)
            {
                _context.CartItems.Remove(itemToUpdate);
            }
            else
            {
                itemToUpdate.Quantity = updateCartDto.Quantity;
                _context.CartItems.Update(itemToUpdate);
            }

            cart.ModifiedDate = DateTime.UtcNow;
            _context.Carts.Update(cart);

            await _context.SaveChangesAsync();

            return await GetOrCreateCartAsync(customerId);
        }

    }
}
