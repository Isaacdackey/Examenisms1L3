using WebBBurger.Models.ViewModels;
using WebBBurger.Services;
using WebBBurger.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using WebBBurger.Models;

namespace WebBBurger.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly IAuthService _authService;
        private readonly IZoneLivraisonRepository _zoneRepository;
        private readonly IPaymentService _paymentService;

        public OrderController(
            IOrderService orderService,
            ICartService cartService,
            IAuthService authService,
            IZoneLivraisonRepository zoneRepository,
            IPaymentService paymentService)
        {
            _orderService = orderService;
            _cartService = cartService;
            _authService = authService;
            _zoneRepository = zoneRepository;
            _paymentService = paymentService;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            if (!_authService.IsAuthenticated())
            {
                TempData["ErrorMessage"] = "Veuillez vous connecter pour passer commande.";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Checkout", "Order") });
            }

            var cart = await _cartService.GetCartAsync();
            if (cart == null || cart.Items == null || !cart.Items.Any())
            {
                TempData["ErrorMessage"] = "Votre panier est vide.";
                return RedirectToAction("Index", "Cart");
            }

            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                _authService.Logout();
                return RedirectToAction("Login", "Account");
            }

            var zones = await _zoneRepository.GetAllAsync();

            var model = new CheckoutViewModel
            {
                Panier = cart,
                TotalCommande = cart.Total,
                Adresse = user.Adresse ?? "",
                Zones = zones.Select(z => new ZoneLivraisonViewModel
                {
                    Id = z.Id,
                    Nom = z.Nom,
                    PrixLivraison = z.PrixLivraison,
                    QuartiersCouverts = z.QuartiersCouverts
                }).ToList()
            };

            if (!string.IsNullOrEmpty(user.Adresse))
            {
                model.TypeRetrait = "LIVRAISON";
            }

            ViewData["Title"] = "Validation de commande - Brasil Burger";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            if (!_authService.IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                _authService.Logout();
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                await ReloadCheckoutData(model);
                return View(model);
            }

            var cart = await _cartService.GetCartAsync();
            if (cart == null || cart.Items == null || !cart.Items.Any())
            {
                TempData["ErrorMessage"] = "Votre panier est vide.";
                return RedirectToAction("Index", "Cart");
            }

            
            if (model.TypeRetrait == "LIVRAISON" && model.ZoneId.HasValue)
            {
                var zone = await _zoneRepository.GetByIdAsync(model.ZoneId.Value);
                if (zone != null)
                {
                    model.FraisLivraison = zone.PrixLivraison;
                }
            }
            else
            {
                model.FraisLivraison = 0;
            }

            model.Panier = cart;
            model.TotalCommande = cart.Total;

            var order = await _orderService.CreateOrderAsync(model, user.Id);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Erreur lors de la création de la commande.";
                await ReloadCheckoutData(model);
                return View(model);
            }

            return RedirectBasedOnPaymentMethod(order, model.MoyenPaiement);
        }

        [HttpGet]
        public async Task<IActionResult> Payment(int orderId)
        {
            if (!_authService.IsAuthenticated())
            {
                TempData["ErrorMessage"] = "Veuillez vous connecter.";
                return RedirectToAction("Login", "Account", new { 
                    returnUrl = $"/Order/Payment?orderId={orderId}" 
                });
            }

            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                _authService.Logout();
                TempData["ErrorMessage"] = "Session expirée. Veuillez vous reconnecter.";
                return RedirectToAction("Login", "Account");
            }

            var order = await _orderService.GetOrderByIdAsync(orderId);
            
            if (order == null)
            {
                return NotFound();
            }

            if (order.UserId != user.Id)
            {
                return NotFound();
            }

            
            if (order.Statut == "TERMINEE" || order.Statut == "PAYE")
            {
                TempData["InfoMessage"] = "Cette commande a déjà été payée.";
                return RedirectToAction("Details", new { id = orderId });
            }

            
            if (order.Statut != "EN_ATTENTE")
            {
                TempData["ErrorMessage"] = "Cette commande ne peut plus être payée.";
                return RedirectToAction("Details", new { id = orderId });
            }

            ViewData["Title"] = "Paiement - Brasil Burger";
            
            var orderNumber = !string.IsNullOrEmpty(order.Numero) ? order.Numero : $"CMD{order.Id:D6}";
            var paymentViewModel = new PaymentViewModel
            {
                CommandeId = orderId,
                Total = order.MontantTotal,
                NumeroCommande = orderNumber,
                MoyenPaiement = order.MoyenPaiement
            };
            
            return View("PaymentSelection", paymentViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(int orderId, string paymentMethod)
        {
            if (!_authService.IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _orderService.GetOrderByIdAsync(orderId);
            
            if (order == null)
            {
                return NotFound();
            }

            if (order.UserId != user.Id)
            {
                return NotFound();
            }

            var success = await _orderService.ProcessPaymentAsync(orderId, paymentMethod);
            if (success)
            {
                await _cartService.ClearCartAsync();
                TempData["SuccessMessage"] = "Paiement effectué avec succès ! Votre commande est en cours de préparation.";
                return RedirectToAction("Details", new { id = orderId });
            }

            TempData["ErrorMessage"] = "Erreur lors du traitement du paiement. Veuillez réessayer.";
            return RedirectToAction("Payment", new { orderId });
        }

        
        [HttpGet]
        public async Task<IActionResult> SimulatePayment(int orderId)
        {
            if (!_authService.IsAuthenticated())
            {
                TempData["ErrorMessage"] = "Veuillez vous connecter.";
                return RedirectToAction("Login", "Account");
            }

            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                _authService.Logout();
                return RedirectToAction("Login", "Account");
            }

            var order = await _orderService.GetOrderByIdAsync(orderId);
            
            if (order == null || order.UserId != user.Id)
            {
                return NotFound();
            }

            if (order.Statut == "TERMINEE" || order.Statut == "PAYE")
            {
                TempData["InfoMessage"] = "Cette commande a déjà été payée.";
                return RedirectToAction("Details", new { id = orderId });
            }

            ViewData["Title"] = "Simulation de paiement - Brasil Burger";
            
            var model = new SimulatePaymentViewModel
            {
                OrderId = orderId,
                Total = order.MontantTotal,
                OrderNumber = order.Numero ?? $"CMD{order.Id:D6}",
                PaymentMethod = order.MoyenPaiement
            };
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SimulatePaymentConfirmed(int orderId, string paymentMethod)
        {
            if (!_authService.IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.UserId != user.Id)
            {
                return NotFound();
            }

            return RedirectToAction("CompleteCashPayment", "Payment", new { commandeId = orderId });
        }

        [HttpGet]
        public async Task<IActionResult> PaymentFailed(int orderId)
        {
            if (!_authService.IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _orderService.GetOrderByIdAsync(orderId);
            
            if (order == null || order.UserId != user.Id)
            {
                return NotFound();
            }

            TempData["ErrorMessage"] = "Paiement échoué. Veuillez réessayer ou choisir un autre moyen de paiement.";
            return RedirectToAction("Payment", new { orderId });
        }

        [HttpGet]
        public async Task<IActionResult> TestPayment(int orderId)
        {
            if (!_authService.IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var canPay = await _orderService.CanProcessPaymentAsync(orderId, user.Id);
            if (!canPay)
            {
                TempData["ErrorMessage"] = "Cette commande ne peut pas être payée.";
                return RedirectToAction("Details", new { id = orderId });
            }

            return RedirectToAction("CompleteCashPayment", "Payment", new { commandeId = orderId });
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            if (!_authService.IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = await _orderService.GetUserOrdersAsync(user.Id);
            ViewData["Title"] = "Historique des commandes - Brasil Burger";
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (!_authService.IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _orderService.GetOrderByIdAsync(id);
            
            if (order == null)
            {
                return NotFound();
            }

            if (order.UserId != user.Id)
            {
                return NotFound();
            }

            var orderNumber = !string.IsNullOrEmpty(order.Numero) ? order.Numero : $"CMD{order.Id:D6}";
            ViewData["Title"] = $"Commande {orderNumber} - Brasil Burger";
            
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            if (!_authService.IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var canCancel = await _orderService.CanCancelOrderAsync(id, user.Id);
            if (!canCancel)
            {
                TempData["ErrorMessage"] = "Impossible d'annuler cette commande.";
                return RedirectToAction("Details", new { id });
            }

            var success = await _orderService.CancelOrderAsync(id, user.Id);
            if (success)
            {
                TempData["SuccessMessage"] = "Commande annulée avec succès.";
            }
            else
            {
                TempData["ErrorMessage"] = "Impossible d'annuler cette commande.";
            }

            return RedirectToAction("Details", new { id });
        }

        

        private async Task ReloadCheckoutData(CheckoutViewModel model)
        {
            var zones = await _zoneRepository.GetAllAsync();
            model.Zones = zones.Select(z => new ZoneLivraisonViewModel
            {
                Id = z.Id,
                Nom = z.Nom ?? "",
                PrixLivraison = z.PrixLivraison,
                QuartiersCouverts = z.QuartiersCouverts ?? ""
            }).ToList();

            var cart = await _cartService.GetCartAsync();
            model.Panier = cart;
            model.TotalCommande = cart?.Total ?? 0;
        }

        private IActionResult RedirectBasedOnPaymentMethod(Commande order, string paymentMethod)
        {
            if (paymentMethod == "EN_LIGNE" || paymentMethod == "WAVE" || paymentMethod == "ORANGE_MONEY" || paymentMethod == "CARTE")
            {
                return RedirectToAction("Payment", "Order", new { orderId = order.Id });
            }
            else
            {
                TempData["SuccessMessage"] = "Commande validée ! Rendez-vous au restaurant pour finaliser votre paiement.";
                return RedirectToAction("Details", new { id = order.Id });
            }
        }
    }
}