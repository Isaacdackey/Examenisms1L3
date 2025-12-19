using System.Collections.Generic;
using System.Threading.Tasks;
using WebBBurger.Models;
using WebBBurger.Models.ViewModels;

namespace WebBBurger.Services
{
    public interface IOrderService
    {
        Task<Commande?> CreateOrderAsync(CheckoutViewModel model, int userId);
        Task<IEnumerable<Commande>> GetUserOrdersAsync(int userId);
        Task<Commande?> GetOrderByIdAsync(int id);
        Task<bool> CancelOrderAsync(int orderId, int userId);
        Task<bool> ProcessPaymentAsync(int orderId, string paymentMethod);
        
        
        Task<bool> CanProcessPaymentAsync(int orderId, int userId);
        Task<bool> CanCancelOrderAsync(int orderId, int userId);
    }
}