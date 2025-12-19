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
    public class ZoneLivraisonRepository : IZoneLivraisonRepository
    {
        private readonly ApplicationDbContext _context;

        public ZoneLivraisonRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ZoneLivraison?> GetByIdAsync(int id)
        {
            return await _context.ZoneLivraisons.FindAsync(id);
        }

        public async Task<IEnumerable<ZoneLivraison>> GetAllAsync()
        {
            return await _context.ZoneLivraisons
                .OrderBy(z => z.PrixLivraison)
                .ToListAsync();
        }

        public async Task<ZoneLivraison?> GetByQuartierAsync(string quartier)
        {
            return await _context.ZoneLivraisons
                .FirstOrDefaultAsync(z => z.QuartiersCouverts.Contains(quartier));
        }
    }
}