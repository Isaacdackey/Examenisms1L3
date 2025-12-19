using WebBBurger.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebBBurger.Repositories
{
    public interface IComplementRepository
    {
        Task<Complement?> GetByIdAsync(int id);
        Task<IEnumerable<Complement>> GetAllAsync();
        Task<Complement> CreateAsync(Complement complement);
        Task<Complement> UpdateAsync(Complement complement);
        Task<bool> DeleteAsync(int id);
    }
}