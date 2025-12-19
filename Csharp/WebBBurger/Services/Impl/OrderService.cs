using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBBurger.Models;
using WebBBurger.Models.ViewModels;
using WebBBurger.Repositories;
using WebBBurger.Services;
using System.Linq;

namespace WebBBurger.Services.Impl
{
    public class OrderService : IOrderService
    {
        private readonly ICommandeRepository _commandeRepository;
        private readonly IProductRepository _productRepository;
        private readonly IZoneLivraisonRepository _zoneRepository;
        private readonly IPaymentService _paymentService;
        private readonly ICartService _cartService;
        private readonly IAuthService _authService;
        private readonly IPaymentRepository _paymentRepository;

        public OrderService(
            ICommandeRepository commandeRepository,
            IProductRepository productRepository,
            IZoneLivraisonRepository zoneRepository,
            IPaymentService paymentService,
            ICartService cartService,
            IAuthService authService,
            IPaymentRepository paymentRepository)
        {
            _commandeRepository = commandeRepository;
            _productRepository = productRepository;
            _zoneRepository = zoneRepository;
            _paymentService = paymentService;
            _cartService = cartService;
            _authService = authService;
            _paymentRepository = paymentRepository;
        }

        public async Task<Commande?> CreateOrderAsync(CheckoutViewModel model, int userId)
        {
            Console.WriteLine($"=== CRÉATION COMMANDE POUR USER {userId} ===");
            
            
            var cart = await _cartService.GetCartAsync();
            if (cart == null || cart.Items == null || cart.Items.Count == 0)
            {
                Console.WriteLine("Panier vide");
                return null;
            }

            Console.WriteLine($"Panier: {cart.Items.Count} articles, Total: {cart.Total}");

            
            decimal fraisLivraison = 0;
            if (model.TypeRetrait == "LIVRAISON" && model.ZoneId.HasValue)
            {
                var zone = await _zoneRepository.GetByIdAsync(model.ZoneId.Value);
                fraisLivraison = zone?.PrixLivraison ?? 0;
                Console.WriteLine($"Frais livraison: {fraisLivraison}");
            }

            
            var lastOrder = await _commandeRepository.GetLastOrderAsync();
            Console.WriteLine($"Dernière commande: ID={lastOrder?.Id}, Numéro={lastOrder?.Numero}, Date={lastOrder?.CreatedAt}");
            
            var orderNumber = GenerateOrderNumber(lastOrder);
            Console.WriteLine($"Numéro généré: {orderNumber}");

            
            if (string.IsNullOrEmpty(orderNumber))
            {
                orderNumber = GenerateFallbackOrderNumber();
                Console.WriteLine($"Numéro fallback généré: {orderNumber}");
            }


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

            Console.WriteLine($"Commande créée: Numéro={commande.Numero}, Montant={commande.MontantTotal}, Statut={commande.Statut}");

            
            var createdCommande = await _commandeRepository.CreateAsync(commande);
            if (createdCommande == null)
            {
                Console.WriteLine("Erreur création commande");
                return null;
            }

            Console.WriteLine($"Commande sauvegardée: ID={createdCommande.Id}");

            
            var payment = await _paymentService.CreatePaymentAsync(
                createdCommande.Id,
                createdCommande.MontantTotal,
                model.MoyenPaiement ?? "ESPECES");

            Console.WriteLine($"Paiement créé: Réf={payment?.ReferenceTransaction}");

            
            foreach (var item in cart.Items)
            {
                var ligneCommande = new LigneCommande
                {
                    CommandeId = createdCommande.Id,
                    ProduitId = item.ProductId,
                    Quantite = item.Quantite,
                    PrixUnitaire = item.Prix,
                    CreatedAt = DateTime.UtcNow
                };
                
                createdCommande.LigneCommandes.Add(ligneCommande);
                Console.WriteLine($"Ligne: Produit={item.ProductId}, Qte={item.Quantite}, Prix={item.Prix}");
            }

         
            await _commandeRepository.UpdateAsync(createdCommande);
            Console.WriteLine($"Lignes commande ajoutées");

        
            await _cartService.ClearCartAsync();
            Console.WriteLine($"Panier vidé");

            Console.WriteLine($"Commande #{createdCommande.Id} créée avec succès!");
            return createdCommande;
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
            if (order == null || order.Statut == "ANNULEE")
            {
                return false;
            }

            
            if (order.Statut != "EN_ATTENTE")
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
            if (order == null || order.UserId != userId)
            {
                return false;
            }

            
            if (order.Statut != "EN_ATTENTE")
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
            if (order == null || order.UserId != userId)
            {
                return false;
            }

            
            if (order.Statut != "EN_ATTENTE")
            {
                return false;
            }

            
            var payments = await _paymentRepository.GetPaymentsByOrderIdAsync(orderId);
            var alreadyPaid = payments?.Any(p => p.StatutPaiement == "PAYE") ?? false;
            
            return !alreadyPaid;
        }

        
        private string GenerateOrderNumber(Commande? lastOrder)
        {
            var now = DateTime.UtcNow;
            var datePart = now.ToString("yyyyMMdd");
            
            Console.WriteLine($"GenerateOrderNumber: Date={datePart}, LastOrder ID={lastOrder?.Id}");

            
            if (lastOrder == null)
            {
                Console.WriteLine($"Première commande du jour: CMD-{datePart}-0001");
                return $"CMD-{datePart}-0001";
            }

            
            if (string.IsNullOrEmpty(lastOrder.Numero))
            {
                Console.WriteLine($"Dernière commande sans numéro, on commence à 0001");
                return $"CMD-{datePart}-0001";
            }

            if (lastOrder.CreatedAt.Date == now.Date)
            {

                var lastNumber = ExtractOrderNumber(lastOrder.Numero);
                if (lastNumber > 0)
                {
                    var nextNumber = lastNumber + 1;
                    Console.WriteLine($"Incrémentation: {lastNumber} → {nextNumber}");
                    return $"CMD-{datePart}-{nextNumber:0000}";
                }
                else
                {
                    Console.WriteLine($"Impossible d'extraire le numéro, on commence à 0001");
                    return $"CMD-{datePart}-0001";
                }
            }

            
            Console.WriteLine($"Nouveau jour: CMD-{datePart}-0001");
            return $"CMD-{datePart}-0001";
        }

        
        private int ExtractOrderNumber(string? orderNumber)
        {
            if (string.IsNullOrEmpty(orderNumber))
            {
                Console.WriteLine($"ExtractOrderNumber: Numéro vide");
                return 0;
            }

            try
            {
                Console.WriteLine($"ExtractOrderNumber: {orderNumber}");
                
            
                var parts = orderNumber.Split('-');
                if (parts.Length == 3)
                {
                    if (int.TryParse(parts[2], out int number))
                    {
                        Console.WriteLine($"Numéro extrait: {number}");
                        return number;
                    }
                }
                
                Console.WriteLine($"Format invalide: {orderNumber}");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERREUR ExtractOrderNumber: {ex.Message}");
                return 0;
            }
        }

        
        private string GenerateFallbackOrderNumber()
        {
            var now = DateTime.UtcNow;
            var datePart = now.ToString("yyyyMMdd");
            var timestamp = now.ToString("HHmmss");
            var random = new Random().Next(100, 999);
            
            var fallbackNumber = $"CMD-{datePart}-{timestamp}{random}";
            Console.WriteLine($"Fallback généré: {fallbackNumber}");
            
            return fallbackNumber;
        }
    }
}