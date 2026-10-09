using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebBBurger.Data;
using WebBBurger.Models;
using WebBBurger.Models.ViewModels;
using WebBBurger.Repositories;
using WebBBurger.Services;
using WebBBurger.Utils;

namespace WebBBurger.Services.Impl
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICommandeRepository _commandeRepository;
        private readonly IProductRepository _productRepository;
        private readonly IZoneLivraisonRepository _zoneRepository;
        private readonly IPaymentService _paymentService;
        private readonly ICartService _cartService;
        private readonly IAuthService _authService;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            ApplicationDbContext context,
            ICommandeRepository commandeRepository,
            IProductRepository productRepository,
            IZoneLivraisonRepository zoneRepository,
            IPaymentService paymentService,
            ICartService cartService,
            IAuthService authService,
            IPaymentRepository paymentRepository,
            ILogger<OrderService> logger)
        {
            _context = context;
            _commandeRepository = commandeRepository;
            _productRepository = productRepository;
            _zoneRepository = zoneRepository;
            _paymentService = paymentService;
            _cartService = cartService;
            _authService = authService;
            _paymentRepository = paymentRepository;
            _logger = logger;
        }

        public async Task<Commande?> CreateOrderAsync(CheckoutViewModel model, int userId)
        {
            _logger.LogInformation("Création d'une commande pour l'utilisateur ID={UserId}", userId);

            var cart = await _cartService.GetCartAsync();
            if (cart == null || cart.Items == null || cart.Items.Count == 0)
            {
                _logger.LogWarning("Tentative de commande avec un panier vide pour l'utilisateur ID={UserId}", userId);
                return null;
            }

            decimal fraisLivraison = 0;
            if (model.TypeRetrait == "LIVRAISON" && model.ZoneId.HasValue)
            {
                var zone = await _zoneRepository.GetByIdAsync(model.ZoneId.Value);
                fraisLivraison = zone?.PrixLivraison ?? 0;
            }

            var orderNumber = NumberGenerator.GenerateOrderNumber();

            var commande = new Commande
            {
                UserId = userId,
                Adresse = model.Adresse ?? string.Empty,
                MontantTotal = cart.Total + fraisLivraison,
                Statut = "EN_ATTENTE",
                TypeRetrait = model.TypeRetrait ?? "EMPORTER",
                Numero = orderNumber,
                ZoneId = model.TypeRetrait == "LIVRAISON" ? model.ZoneId : null,
                Notes = model.Notes ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var item in cart.Items)
            {
                commande.LigneCommandes.Add(new LigneCommande
                {
                    ProduitId = item.ProductId,
                    Quantite = item.Quantite,
                    PrixUnitaire = item.Prix,
                    CreatedAt = DateTime.UtcNow
                });
            }

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.Commandes.Add(commande);
                    await _context.SaveChangesAsync();

                    var paymentMethod = model.MoyenPaiement ?? "ESPECES";
                    var payment = await _paymentService.CreatePaymentAsync(
                        commande.Id,
                        commande.MontantTotal,
                        paymentMethod);

                    if (payment == null)
                    {
                        throw new InvalidOperationException("Échec de la création du paiement associé.");
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    await _cartService.ClearCartAsync();
                    _logger.LogInformation("Commande ID={OrderId} (Réf={OrderNumber}) créée avec succès.", commande.Id, commande.Numero);

                    return commande;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Erreur transactionnelle lors de la création de la commande pour ID={UserId}", userId);
                    return null;
                }
            });
        }

        public async Task<IEnumerable<Commande>> GetUserOrdersAsync(int userId)
        {
            return await _commandeRepository.GetByUserIdAsync(userId);
        }

        public async Task<Commande?> GetOrderByIdAsync(int id)
        {
            return await _commandeRepository.GetByIdAsync(id);
        }

        public async Task<bool> CancelOrderAsync(int orderId, int userId)
        {
            var order = await _commandeRepository.GetByIdAsync(orderId);
            if (order == null || order.UserId != userId)
            {
                return false;
            }

            if (order.Statut == "EN_ATTENTE")
            {
                var payments = await _paymentRepository.GetPaymentsByOrderIdAsync(orderId);
                var alreadyPaid = payments?.Any(p => p.StatutPaiement == "PAYE") ?? false;

                if (alreadyPaid)
                {
                    return false;
                }

                return await _commandeRepository.CancelAsync(orderId);
            }

            return false;
        }

        public async Task<bool> ProcessPaymentAsync(int orderId, string paymentMethod)
        {
            var order = await _commandeRepository.GetByIdAsync(orderId);
            if (order == null || order.Statut == "ANNULEE" || order.Statut != "EN_ATTENTE")
            {
                return false;
            }

            var existingPayments = await _paymentRepository.GetPaymentsByOrderIdAsync(orderId);
            var alreadyPaid = existingPayments?.Any(p => p.StatutPaiement == "PAYE") ?? false;

            if (alreadyPaid)
            {
                return false;
            }

            var payment = await _paymentService.CreatePaymentAsync(
                orderId,
                order.MontantTotal,
                paymentMethod);

            if (payment == null)
            {
                return false;
            }

            order.Statut = "VALIDEE";
            await _commandeRepository.UpdateAsync(order);

            return true;
        }

        public async Task<bool> CanProcessPaymentAsync(int orderId, int userId)
        {
            var order = await _commandeRepository.GetByIdAsync(orderId);
            if (order == null || order.UserId != userId || order.Statut != "EN_ATTENTE")
            {
                return false;
            }

            var payments = await _paymentRepository.GetPaymentsByOrderIdAsync(orderId);
            var alreadyPaid = payments?.Any(p => p.StatutPaiement == "PAYE") ?? false;

            return !alreadyPaid;
        }

        public async Task<bool> CanCancelOrderAsync(int orderId, int userId)
        {
            var order = await _commandeRepository.GetByIdAsync(orderId);
            if (order == null || order.UserId != userId || order.Statut != "EN_ATTENTE")
            {
                return false;
            }

            var payments = await _paymentRepository.GetPaymentsByOrderIdAsync(orderId);
            var alreadyPaid = payments?.Any(p => p.StatutPaiement == "PAYE") ?? false;

            return !alreadyPaid;
        }
    }
}