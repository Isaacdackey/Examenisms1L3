using System.Collections.Generic;
using System.Threading.Tasks;
using WebBBurger.Models;

namespace WebBBurger.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<IEnumerable<Product>> GetByTypeAsync(string type);
        Task<IEnumerable<Product>> GetActiveProductsAsync();
        Task<IEnumerable<Product>> GetBurgersAsync();
        Task<IEnumerable<Product>> GetComplementsAsync();
        Task<IEnumerable<Product>> GetMenusAsync();
        Task<Product> CreateAsync(Product product);
        Task<Product> UpdateAsync(Product product);
        Task<bool> ArchiveAsync(int id);
        Task<bool> UnarchiveAsync(int id);
    }
}