using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBBurger.Data;
using WebBBurger.Models;
using WebBBurger.Repositories;
using Microsoft.EntityFrameworkCore;

namespace WebBBurger.Repositories.Impl
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Burger)
                .Include(p => p.Complement)
                .Include(p => p.Menu)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Burger)
                .Include(p => p.Complement)
                .Include(p => p.Menu)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetByTypeAsync(string type)
        {
            return await _context.Products
                .Include(p => p.Burger)
                .Include(p => p.Complement)
                .Include(p => p.Menu)
                .Where(p => p.TypeProduct == type && !p.IsArchived)
                .OrderBy(p => p.Libelle)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetActiveProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Burger)
                .Include(p => p.Complement)
                .Include(p => p.Menu)
                .Where(p => !p.IsArchived)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetBurgersAsync()
        {
            return await GetByTypeAsync("BURGER");
        }

        public async Task<IEnumerable<Product>> GetComplementsAsync()
        {
            return await GetByTypeAsync("COMPLEMENT");
        }

        public async Task<IEnumerable<Product>> GetMenusAsync()
        {
            return await _context.Products
                .Include(p => p.Menu)
                .ThenInclude(m => m!.Burger)
                .ThenInclude(b => b!.Product)
                .Include(p => p.Menu)
                .ThenInclude(m => m!.Boisson)
                .ThenInclude(c => c!.Product)
                .Include(p => p.Menu)
                .ThenInclude(m => m!.Frites)
                .ThenInclude(c => c!.Product)
                .Where(p => p.TypeProduct == "MENU" && !p.IsArchived && p.Menu != null)
                .ToListAsync();
        }

        public async Task<Product> CreateAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> UpdateAsync(Product product)
        {
            product.UpdatedAt = DateTime.Now;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<bool> ArchiveAsync(int id)
        {
            var product = await GetByIdAsync(id);
            if (product == null) return false;

            product.IsArchived = true;
            product.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnarchiveAsync(int id)
        {
            var product = await GetByIdAsync(id);
            if (product == null) return false;

            product.IsArchived = false;
            product.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}