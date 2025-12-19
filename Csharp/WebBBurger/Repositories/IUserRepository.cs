using System.Threading.Tasks;
using WebBBurger.Models;
using System.Collections.Generic;

namespace WebBBurger.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByEmailAndPasswordAsync(string email, string password);
        Task<User?> GetUserByPhoneAsync(string phone);
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<bool> DeleteAsync(int id);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> PhoneExistsAsync(string phone);
        Task<IEnumerable<User>> GetAllAsync();
        Task<int> CountAsync();
    }
}