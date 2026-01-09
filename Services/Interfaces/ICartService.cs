using cl_be.Models.Dto.CartDto;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cl_be.Services.Interfaces
{
    /// <summary>
    /// Service interface for cart operations working directly with DbContext
    /// Handles business logic, validation, and data mapping
    /// </summary>
    public interface ICartService
    {
        // ========================================
        // CUSTOMER CART OPERATIONS
        // ========================================


        Task<CartResponseDTO> GetOrCreateCartAsync(int customerId);  

        Task<CartResponseDTO> AddToCartAsync(int customerId, AddToCartDto addToCartDto);

       
        Task<CartResponseDTO> UpdateCartItemAsync(int customerId, int cartItemId, UpdateCartDto updateCartDto);

     
        Task<CartResponseDTO> RemoveFromCartAsync(int customerId, RemoveFromCartDto removeFromCartDto);

      
        Task<CartResponseDTO> ClearCartAsync(int customerId);


        Task<bool> DeleteCartAsync(int customerId);

        // ========================================
        // CART INFORMATION & VALIDATION
        // ========================================

      
        Task<decimal> GetCartTotalAsync(int customerId);

       
        Task<int> GetCartItemsCountAsync(int customerId);

       
        Task<bool> IsProductInCartAsync(int customerId, int productId);

        // ========================================
        // ADMIN OPERATIONS
        // ========================================

       
        Task<CartResponseDTO?> GetCartByIdAsync(int cartId);

       
        Task<List<CartResponseDTO>> GetAllActiveCartsAsync();

       
        Task<List<CartResponseDTO>> GetCustomerCartsAsync(int customerId);

       
        Task<bool> DeleteCartByIdAsync(int cartId);
    }
}
