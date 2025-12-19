using System.Threading.Tasks;
using WebBBurger.Models.ViewModels;

namespace WebBBurger.Services
{
    public interface ICartService
    {

        Task<CartViewModel> GetCartAsync();
        Task AddToCartAsync(CartItemViewModel item);
        Task AddToCartAsync(int productId, int quantity = 1);
        Task RemoveFromCartAsync(int productId);
        Task UpdateQuantityAsync(int productId, int quantity);
        Task ClearCartAsync();
        int GetCartItemCount();
        decimal GetCartTotal();
        
        Task AddBurgerWithComplementsAsync(int burgerId, int quantity, 
            int? boissonId = null, int? fritesId = null);
    }
}