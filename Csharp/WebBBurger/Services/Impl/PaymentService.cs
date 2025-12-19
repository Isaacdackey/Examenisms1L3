using System;
using System.Threading.Tasks;
using WebBBurger.Models;
using WebBBurger.Repositories;

namespace WebBBurger.Services.Impl
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ICommandeRepository _commandeRepository;

        public PaymentService(
            IPaymentRepository paymentRepository,
            ICommandeRepository commandeRepository)
        {
            _paymentRepository = paymentRepository;
            _commandeRepository = commandeRepository;
        }

        public async Task<Payment?> CreatePaymentAsync(int commandeId, decimal montant, string moyenPaiement)
        {
            try
            {
    
                var commande = await _commandeRepository.GetByIdAsync(commandeId);
                if (commande == null)
                {
                    throw new ArgumentException($"Commande avec l'ID {commandeId} non trouvée.");
                }

                if (commande.Statut == "PAYE" || commande.Statut == "TERMINEE")
                {
                    throw new InvalidOperationException("Cette commande est déjà payée.");
                }

                var validMoyens = new[] { "WAVE", "ORANGE_MONEY", "CARTE", "ESPECES" };
                if (!Array.Exists(validMoyens, m => m.Equals(moyenPaiement, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ArgumentException($"Moyen de paiement '{moyenPaiement}' invalide.");
                }

                var existingPayments = await _paymentRepository.GetByCommandeIdAsync(commandeId);
                foreach (var existingPayment in existingPayments)
                {
                    if (existingPayment.StatutPaiement == "EN_ATTENTE")
                    {
                        throw new InvalidOperationException("Un paiement est déjà en attente pour cette commande.");
                    }
                }

                
                var payment = new Payment
                {
                    CommandeId = commandeId,
                    Montant = montant,
                    MoyenPaiement = moyenPaiement.ToUpper(),
                    ReferenceTransaction = GeneratePaymentReference(),
                    StatutPaiement = "EN_ATTENTE",
                    CreatedAt = DateTime.UtcNow
                };


                if (moyenPaiement.ToUpper() == "ESPECES")
                {
                    payment.StatutPaiement = "PAYE";
                    payment.DatePaiement = DateTime.UtcNow;
                }

                var createdPayment = await _paymentRepository.CreateAsync(payment);

                return createdPayment;
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Erreur lors de la création du paiement: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> ValidatePaymentAsync(string referenceTransaction)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(referenceTransaction))
                {
                    throw new ArgumentException("La référence de transaction est requise.");
                }

                var payment = await _paymentRepository.GetByReferenceAsync(referenceTransaction);
                if (payment == null)
                {
                    throw new KeyNotFoundException($"Paiement avec la référence '{referenceTransaction}' non trouvé.");
                }

                
                if (payment.StatutPaiement == "PAYE")
                {
                    return true;
                }

                
                if (payment.StatutPaiement == "ANNULE" || payment.StatutPaiement == "ECHEC")
                {
                    throw new InvalidOperationException($"Le paiement est {payment.StatutPaiement.ToLower()}.");
                }

                
                bool isPaymentSuccessful = await SimulatePaymentValidation(payment);

                if (isPaymentSuccessful)
                {
                    
                    payment.StatutPaiement = "PAYE";
                    payment.DatePaiement = DateTime.UtcNow;
                    await _paymentRepository.UpdateAsync(payment);


                    var commande = await _commandeRepository.GetByIdAsync(payment.CommandeId);
                    if (commande != null)
                    {
                        commande.Statut = "PAYE";
                        commande.UpdatedAt = DateTime.UtcNow;
                        await _commandeRepository.UpdateAsync(commande);
                    }

                    return true;
                }
                else
                {
                    payment.StatutPaiement = "ECHEC";
                    await _paymentRepository.UpdateAsync(payment);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la validation du paiement: {ex.Message}");
                return false;
            }
        }

        public string GeneratePaymentReference()
        {

            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var guidPart = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            return $"PAY-{timestamp}-{guidPart}";
        }

        

        private async Task<bool> SimulatePaymentValidation(Payment payment)
        {
            
            
            await Task.Delay(100);
            
            
            var random = new Random();
            return random.Next(1, 11) > 1;
        }

        

        public async Task<Payment?> GetPaymentByReferenceAsync(string reference)
        {
            return await _paymentRepository.GetByReferenceAsync(reference);
        }

        public async Task<bool> CancelPaymentAsync(int paymentId)
        {
            try
            {
                var payment = await _paymentRepository.GetByIdAsync(paymentId);
                if (payment == null)
                {
                    return false;
                }

                
                if (payment.StatutPaiement == "PAYE")
                {
                    throw new InvalidOperationException("Impossible d'annuler un paiement réussi.");
                }


                payment.StatutPaiement = "ANNULE";
                await _paymentRepository.UpdateAsync(payment);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'annulation du paiement: {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByCommandeIdAsync(int commandeId)
        {
            return await _paymentRepository.GetByCommandeIdAsync(commandeId);
        }
    }
}