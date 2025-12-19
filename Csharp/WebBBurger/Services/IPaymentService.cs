using System.Threading.Tasks;
using WebBBurger.Models;
using WebBBurger.Models.ViewModels;

namespace WebBBurger.Services
{
    public interface IPaymentService
    {
        Task<Payment?> CreatePaymentAsync(int commandeId, decimal montant, string moyenPaiement);
        Task<bool> ValidatePaymentAsync(string referenceTransaction);
        string GeneratePaymentReference();
    }
}