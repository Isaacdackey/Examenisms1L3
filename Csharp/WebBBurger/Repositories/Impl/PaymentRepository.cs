using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebBBurger.Data;
using WebBBurger.Models;

namespace WebBBurger.Repositories.Impl
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _context.Payments
                .Include(p => p.Commande)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Payment?> GetByReferenceAsync(string reference)
        {
            return await _context.Payments
                .Include(p => p.Commande)
                .FirstOrDefaultAsync(p => p.ReferenceTransaction == reference);
        }

        public async Task<Payment> CreateAsync(Payment payment)
        {
        
            NormalizeDateTimeToUtc(payment);
            
            
            if (await ReferenceExistsAsync(payment.ReferenceTransaction))
            {
                throw new InvalidOperationException($"La référence de paiement '{payment.ReferenceTransaction}' existe déjà.");
            }


            var commande = await _context.Commandes.FindAsync(payment.CommandeId);
            if (commande == null)
            {
                throw new InvalidOperationException($"La commande avec l'ID {payment.CommandeId} n'existe pas.");
            }

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            
            
            await _context.Entry(payment)
                .Reference(p => p.Commande)
                .LoadAsync();

            return payment;
        }

        public async Task UpdateAsync(Payment payment)
        {
            var existingPayment = await GetByIdAsync(payment.Id);
            if (existingPayment == null)
            {
                throw new KeyNotFoundException($"Paiement avec l'ID {payment.Id} non trouvé.");
            }

            NormalizeDateTimeToUtc(payment);
            
            
            if (existingPayment.ReferenceTransaction != payment.ReferenceTransaction)
            {
                if (await ReferenceExistsAsync(payment.ReferenceTransaction))
                {
                    throw new InvalidOperationException($"La référence de paiement '{payment.ReferenceTransaction}' existe déjà.");
                }
            }

           
            existingPayment.Montant = payment.Montant;
            existingPayment.MoyenPaiement = payment.MoyenPaiement;
            existingPayment.ReferenceTransaction = payment.ReferenceTransaction;
            existingPayment.StatutPaiement = payment.StatutPaiement;
            existingPayment.DatePaiement = payment.DatePaiement;
            
            if (payment.CreatedAt != default)
            {
                existingPayment.CreatedAt = payment.CreatedAt;
            }
            
            NormalizeDateTimeToUtc(existingPayment);
            
            _context.Payments.Update(existingPayment);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var payment = await GetByIdAsync(id);
            if (payment == null)
            {
                return false;
            }

            if (payment.StatutPaiement == "PAYE")
            {
                throw new InvalidOperationException("Impossible de supprimer un paiement réussi.");
            }

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Payment>> GetByCommandeIdAsync(int commandeId)
        {
            return await _context.Payments
                .Where(p => p.CommandeId == commandeId)
                .Include(p => p.Commande)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetByStatutAsync(string statut)
        {
            return await _context.Payments
                .Where(p => p.StatutPaiement == statut)
                .Include(p => p.Commande)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetByMoyenPaiementAsync(string moyenPaiement)
        {
            return await _context.Payments
                .Where(p => p.MoyenPaiement == moyenPaiement)
                .Include(p => p.Commande)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _context.Payments
                .Include(p => p.Commande)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Payments.AnyAsync(p => p.Id == id);
        }

        public async Task<bool> ReferenceExistsAsync(string reference)
        {
            return await _context.Payments.AnyAsync(p => p.ReferenceTransaction == reference);
        }


        public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync()
        {
            return await GetByStatutAsync("EN_ATTENTE");
        }

        public async Task<IEnumerable<Payment>> GetSuccessfulPaymentsAsync()
        {
            return await GetByStatutAsync("PAYE");
        }

        public async Task<IEnumerable<Payment>> GetFailedPaymentsAsync()
        {
            return await GetByStatutAsync("ECHEC");
        }

        public async Task<IEnumerable<Payment>> GetCanceledPaymentsAsync()
        {
            return await GetByStatutAsync("ECHEC");
        }

        public async Task<decimal> GetTotalAmountByDateAsync(DateTime date)
        {
            var dateUtc = date.ToUniversalTime();
            var nextDayUtc = date.AddDays(1).ToUniversalTime();
            
            return await _context.Payments
                .Where(p => p.DatePaiement.HasValue &&
                           p.DatePaiement.Value >= dateUtc &&
                           p.DatePaiement.Value < nextDayUtc &&
                           p.StatutPaiement == "PAYE")
                .SumAsync(p => p.Montant);
        }

        public async Task<decimal> GetTotalAmountByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            var startUtc = startDate.ToUniversalTime();
            var endUtc = endDate.AddDays(1).ToUniversalTime();
            
            return await _context.Payments
                .Where(p => p.DatePaiement.HasValue &&
                           p.DatePaiement.Value >= startUtc &&
                           p.DatePaiement.Value < endUtc &&
                           p.StatutPaiement == "PAYE")
                .SumAsync(p => p.Montant);
        }

        public async Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, string? statut = null)
        {
            var startUtc = startDate.ToUniversalTime();
            var endUtc = endDate.AddDays(1).ToUniversalTime();
            
            var query = _context.Payments
                .Include(p => p.Commande)
                .ThenInclude(c => c.Client)
                .Where(p => p.DatePaiement.HasValue &&
                           p.DatePaiement.Value >= startUtc &&
                           p.DatePaiement.Value < endUtc);

            if (!string.IsNullOrEmpty(statut))
            {
                query = query.Where(p => p.StatutPaiement == statut);
            }

            return await query.OrderByDescending(p => p.DatePaiement).ToListAsync();
        }

    
        private void NormalizeDateTimeToUtc(Payment payment)
        {
            if (payment == null) return;

        
            if (payment.CreatedAt.Kind != DateTimeKind.Utc)
            {
                payment.CreatedAt = payment.CreatedAt.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(payment.CreatedAt, DateTimeKind.Utc)
                    : payment.CreatedAt.ToUniversalTime();
            }

        
            if (payment.DatePaiement.HasValue)
            {
                var datePaiement = payment.DatePaiement.Value;
                if (datePaiement.Kind != DateTimeKind.Utc)
                {
                    payment.DatePaiement = datePaiement.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(datePaiement, DateTimeKind.Utc)
                        : datePaiement.ToUniversalTime();
                }
            }
        }
        public async Task<IEnumerable<Payment>> GetPaymentsByOrderIdAsync(int orderId)
        {
            return await GetByCommandeIdAsync(orderId);
        }
    }
}