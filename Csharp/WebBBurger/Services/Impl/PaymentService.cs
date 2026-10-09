using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebBBurger.Models;
using WebBBurger.Repositories;
using WebBBurger.Services;
using WebBBurger.Utils;

namespace WebBBurger.Services.Impl
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ICommandeRepository _commandeRepository;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IPaymentRepository paymentRepository,
            ICommandeRepository commandeRepository,
            ILogger<PaymentService> logger)
        {
            _paymentRepository = paymentRepository;
            _commandeRepository = commandeRepository;
            _logger = logger;
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

                if (commande.Statut == "TERMINEE")
                {
                    throw new InvalidOperationException("Cette commande est déjà terminée.");
                }

                var normalizedMoyen = (moyenPaiement ?? "ESPECES").Trim().ToUpperInvariant();
                var validMoyens = new HashSet<string> { "WAVE", "ORANGE_MONEY", "CARTE", "ESPECES" };
                if (!validMoyens.Contains(normalizedMoyen))
                {
                    normalizedMoyen = "ESPECES";
                }

                var existingPayments = await _paymentRepository.GetByCommandeIdAsync(commandeId);
                foreach (var existingPayment in existingPayments)
                {
                    if (existingPayment.StatutPaiement == "EN_ATTENTE")
                    {
                        return existingPayment;
                    }
                }

                var payment = new Payment
                {
                    CommandeId = commandeId,
                    Montant = montant,
                    MoyenPaiement = normalizedMoyen,
                    ReferenceTransaction = GeneratePaymentReference(),
                    StatutPaiement = "EN_ATTENTE", // Reste en attente même pour les espèces jusqu'à encaissement réel
                    CreatedAt = DateTime.UtcNow
                };

                var createdPayment = await _paymentRepository.CreateAsync(payment);
                _logger.LogInformation("Paiement créé : ID={PaymentId}, Réf={Reference}, Méthode={Method}",
                    createdPayment.Id, createdPayment.ReferenceTransaction, createdPayment.MoyenPaiement);

                return createdPayment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création du paiement pour la commande {OrderId}", commandeId);
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
                    throw new InvalidOperationException($"Le paiement est déjà {payment.StatutPaiement.ToLower()}.");
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
                        commande.Statut = "VALIDEE";
                        commande.UpdatedAt = DateTime.UtcNow;
                        await _commandeRepository.UpdateAsync(commande);
                    }

                    _logger.LogInformation("Paiement validé avec succès pour Réf={Reference}", referenceTransaction);
                    return true;
                }
                else
                {
                    payment.StatutPaiement = "ECHEC";
                    await _paymentRepository.UpdateAsync(payment);
                    _logger.LogWarning("Échec de la validation du paiement pour Réf={Reference}", referenceTransaction);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la validation du paiement Réf={Reference}", referenceTransaction);
                return false;
            }
        }

        public string GeneratePaymentReference()
        {
            return NumberGenerator.GeneratePaymentReference();
        }

        private async Task<bool> SimulatePaymentValidation(Payment payment)
        {
            await Task.Delay(100);
            return true; // Simulation réussie
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
                if (payment == null) return false;

                if (payment.StatutPaiement == "PAYE")
                {
                    throw new InvalidOperationException("Impossible d'annuler un paiement déjà validé.");
                }

                payment.StatutPaiement = "ANNULE";
                await _paymentRepository.UpdateAsync(payment);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'annulation du paiement ID={PaymentId}", paymentId);
                return false;
            }
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByCommandeIdAsync(int commandeId)
        {
            return await _paymentRepository.GetByCommandeIdAsync(commandeId);
        }
    }
}