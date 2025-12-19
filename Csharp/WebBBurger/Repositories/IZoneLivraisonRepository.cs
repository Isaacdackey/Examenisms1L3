using System.Collections.Generic;
using System.Threading.Tasks;
using WebBBurger.Models;

namespace WebBBurger.Repositories
{
    public interface IZoneLivraisonRepository
    {
        Task<ZoneLivraison?> GetByIdAsync(int id);
        Task<IEnumerable<ZoneLivraison>> GetAllAsync();
        Task<ZoneLivraison?> GetByQuartierAsync(string quartier);
    }
}