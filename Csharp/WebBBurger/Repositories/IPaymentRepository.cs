using System.Collections.Generic;
using System.Threading.Tasks;
using WebBBurger.Models;

namespace WebBBurger.Repositories
{
    public interface IPaymentRepository
    {
        
        Task<Payment?> GetByIdAsync(int id);
        Task<Payment?> GetByReferenceAsync(string reference);
        Task<Payment> CreateAsync(Payment payment);
        Task UpdateAsync(Payment payment);
        Task<bool> DeleteAsync(int id);
        
        
        Task<IEnumerable<Payment>> GetByCommandeIdAsync(int commandeId);
        Task<IEnumerable<Payment>> GetByStatutAsync(string statut);
        Task<IEnumerable<Payment>> GetByMoyenPaiementAsync(string moyenPaiement);
        Task<IEnumerable<Payment>> GetAllAsync();
        
        
        Task<bool> ExistsAsync(int id);
        Task<bool> ReferenceExistsAsync(string reference);
        
        
        Task<IEnumerable<Payment>> GetPaymentsByOrderIdAsync(int orderId);
    }
}