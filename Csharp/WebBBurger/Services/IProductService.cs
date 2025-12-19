using System.Collections.Generic;
using System.Threading.Tasks;
using WebBBurger.Models;
using WebBBurger.Models.ViewModels;

namespace WebBBurger.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<IEnumerable<Product>> GetBurgersAsync();
        Task<IEnumerable<Product>> GetComplementsAsync();
        Task<IEnumerable<Product>> GetMenusAsync();
        Task<Product?> GetProductByIdAsync(int id);
        Task<ProductViewModel?> GetProductViewModelAsync(int id);
        Task<IEnumerable<Product>> SearchProductsAsync(string query, string? type = null);
        
        Task<IEnumerable<Product>> GetProductsByTypeAsync(string type);
        
        Task<IEnumerable<Product>> GetBoissonsAsync();
        Task<IEnumerable<Product>> GetFritesAsync();
    }
}