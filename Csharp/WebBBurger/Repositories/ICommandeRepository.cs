using System.Collections.Generic;
using System.Threading.Tasks;
using WebBBurger.Models;

namespace WebBBurger.Repositories
{
    public interface ICommandeRepository
    {
        Task<Commande?> GetByIdAsync(int id);
        Task<Commande?> GetByNumeroAsync(string numero);
        Task<IEnumerable<Commande>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Commande>> GetRecentOrdersAsync(int limit = 10);
        Task<Commande> CreateAsync(Commande commande);
        Task<Commande> UpdateAsync(Commande commande);
        Task<bool> CancelAsync(int id);
        Task<bool> UpdateStatusAsync(int id, string status);
        Task<Commande?> GetLastOrderAsync();
        
    }
}