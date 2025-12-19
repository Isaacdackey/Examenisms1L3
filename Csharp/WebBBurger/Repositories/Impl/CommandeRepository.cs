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
    public class CommandeRepository : ICommandeRepository
    {
        private readonly ApplicationDbContext _context;

        public CommandeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Commande?> GetByIdAsync(int id)
        {
            return await _context.Commandes
                .Include(c => c.User)
                .Include(c => c.Zone)
                .Include(c => c.Livreur)
                .Include(c => c.LigneCommandes)
                    .ThenInclude(lc => lc.Produit)
                .Include(c => c.Payment)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Commande?> GetByNumeroAsync(string numero)
        {
            return await _context.Commandes
                .Include(c => c.User)
                .Include(c => c.Zone)
                .Include(c => c.LigneCommandes)
                    .ThenInclude(lc => lc.Produit)
                .FirstOrDefaultAsync(c => c.Numero == numero);
        }

        public async Task<IEnumerable<Commande>> GetByUserIdAsync(int userId)
        {
            return await _context.Commandes
                .Include(c => c.User)
                .Include(c => c.Zone)
                .Include(c => c.LigneCommandes)
                    .ThenInclude(lc => lc.Produit)
                .Include(c => c.Payment)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Commande>> GetRecentOrdersAsync(int limit = 10)
        {
            return await _context.Commandes
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<Commande?> GetLastOrderAsync()
        {
            try
            {
                Console.WriteLine("GetLastOrderAsync appelé");

                var lastOrder = await _context.Commandes
                    .Where(c => c.Numero != null && c.Numero != "")
                    .OrderByDescending(c => c.CreatedAt)
                    .FirstOrDefaultAsync();

                if (lastOrder == null)
                {

                    lastOrder = await _context.Commandes
                        .OrderByDescending(c => c.CreatedAt)
                        .FirstOrDefaultAsync();
                }

                Console.WriteLine($"Résultat: ID={lastOrder?.Id}, Numéro={lastOrder?.Numero ?? "NULL"}, Date={lastOrder?.CreatedAt}");

                return lastOrder;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERREUR GetLastOrderAsync: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<Commande> CreateAsync(Commande commande)
        {
            try
            {
                Console.WriteLine($"CommandeRepository.CreateAsync: Numéro={commande.Numero}, UserId={commande.UserId}");


                NormalizeDateTimeToUtc(commande);


                if (string.IsNullOrEmpty(commande.Numero))
                {
                    commande.Numero = $"TEMP-{DateTime.UtcNow:yyyyMMddHHmmss}";
                    Console.WriteLine($"Numéro vide, temporaire: {commande.Numero}");
                }

                _context.Commandes.Add(commande);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Commande créée: ID={commande.Id}, Numéro={commande.Numero}");
                return commande;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERREUR CreateAsync: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<Commande> UpdateAsync(Commande commande)
        {

            NormalizeDateTimeToUtc(commande);


            commande.UpdatedAt = DateTime.UtcNow;

            _context.Commandes.Update(commande);
            await _context.SaveChangesAsync();
            return commande;
        }

        public async Task<bool> CancelAsync(int id)
        {
            var commande = await GetByIdAsync(id);
            if (commande == null) return false;

            if (commande.Statut == "EN_ATTENTE" || commande.Statut == "VALIDEE")
            {
                commande.Statut = "ANNULEE";
                commande.UpdatedAt = DateTime.UtcNow;


                NormalizeDateTimeToUtc(commande);

                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var commande = await GetByIdAsync(id);
            if (commande == null) return false;

            commande.Statut = status;
            commande.UpdatedAt = DateTime.UtcNow;


            NormalizeDateTimeToUtc(commande);

            await _context.SaveChangesAsync();
            return true;
        }


        private void NormalizeDateTimeToUtc(Commande commande)
        {
            if (commande == null) return;


            if (commande.CreatedAt.Kind != DateTimeKind.Utc)
            {
                commande.CreatedAt = commande.CreatedAt.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(commande.CreatedAt, DateTimeKind.Utc)
                    : commande.CreatedAt.ToUniversalTime();
            }


            if (commande.UpdatedAt.HasValue)
            {
                if (commande.UpdatedAt.Value.Kind != DateTimeKind.Utc)
                {
                    commande.UpdatedAt = commande.UpdatedAt.Value.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(commande.UpdatedAt.Value, DateTimeKind.Utc)
                        : commande.UpdatedAt.Value.ToUniversalTime();
                }
            }
        }
    }
}