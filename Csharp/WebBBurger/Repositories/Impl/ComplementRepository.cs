using Microsoft.EntityFrameworkCore;
using WebBBurger.Models;
using WebBBurger.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using WebBBurger.Data;

namespace WebBBurger.Repositories.Impl
{
    public class ComplementRepository : IComplementRepository
    {
        private readonly ApplicationDbContext _context;

        public ComplementRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Complement?> GetByIdAsync(int id)
        {
            return await _context.Complements
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Complement>> GetAllAsync()
        {
            return await _context.Complements
                .Include(c => c.Product)
                .Where(c => !c.Product.IsArchived && c.Product.TypeProduct == "COMPLEMENT")
                .ToListAsync();
        }

        public async Task<Complement> CreateAsync(Complement complement)
        {
            
            var product = await _context.Products.FindAsync(complement.Id);
            if (product == null)
            {
                throw new ArgumentException($"Le produit avec l'ID {complement.Id} n'existe pas.");
            }
            
            if (product.TypeProduct != "COMPLEMENT")
            {
                throw new ArgumentException($"Le produit avec l'ID {complement.Id} n'est pas de type COMPLEMENT.");
            }

            _context.Complements.Add(complement);
            await _context.SaveChangesAsync();
            return complement;
        }

        public async Task<Complement> UpdateAsync(Complement complement)
        {
            _context.Complements.Update(complement);
            await _context.SaveChangesAsync();
            return complement;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var complement = await GetByIdAsync(id);
            if (complement == null) return false;
            
            _context.Complements.Remove(complement);
            await _context.SaveChangesAsync();
            return true;
        }

        
        public async Task<IEnumerable<Complement>> GetByTypeAsync(string typeComplement)
        {
            return await _context.Complements
                .Include(c => c.Product)
                .Where(c => c.TypeComplement == typeComplement && !c.Product.IsArchived)
                .ToListAsync();
        }

        
        public async Task<IEnumerable<Complement>> GetBoissonsAsync()
        {
            return await GetByTypeAsync("BOISSON");
        }

    
        public async Task<IEnumerable<Complement>> GetFritesAsync()
        {
            return await GetByTypeAsync("FRITE");
        }
    }
}