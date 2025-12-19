using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WebBBurger.Data;
using WebBBurger.Models;

namespace WebBBurger.Repositories.Impl
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User?> GetByEmailAndPasswordAsync(string email, string password)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.Password == password);
        }

        public async Task<User?> GetUserByPhoneAsync(string phone)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Tel == phone);
        }

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await GetByIdAsync(id);
            if (user == null)
                return false;

            user.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<bool> PhoneExistsAsync(string phone)
        {
            return await _context.Users
                .AnyAsync(u => u.Tel == phone);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                .OrderBy(u => u.Nom)
                .ThenBy(u => u.Prenom)
                .ToListAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _context.Users.CountAsync();
        }


        public async Task<IEnumerable<User>> GetClientsAsync()
        {
            return await _context.Users
                .Where(u => u.Role == "CLIENT" && u.IsActive)
                .OrderBy(u => u.Nom)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetLivreursAsync()
        {
            return await _context.Users
                .Where(u => u.Role == "LIVREUR" && u.IsActive && !string.IsNullOrEmpty(u.Vehicule))
                .OrderBy(u => u.Nom)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _context.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.Nom)
                .ToListAsync();
        }

        public async Task<bool> ActivateUserAsync(int id)
        {
            var user = await GetByIdAsync(id);
            if (user == null)
                return false;

            user.IsActive = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateUserAsync(int id)
        {
            var user = await GetByIdAsync(id);
            if (user == null)
                return false;

            user.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string newPasswordHash)
        {
            var user = await GetByIdAsync(userId);
            if (user == null)
                return false;

            user.Password = newPasswordHash;
            user.UpdatedAt = System.DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}