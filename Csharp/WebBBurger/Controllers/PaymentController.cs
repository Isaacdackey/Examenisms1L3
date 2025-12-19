using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebBBurger.Services;
using WebBBurger.Models;
using WebBBurger.Repositories;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace WebBBurger.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly ICommandeRepository _commandeRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IAuthService _authService;

        public PaymentController(
            IPaymentService paymentService,
            ICommandeRepository commandeRepository,
            IPaymentRepository paymentRepository,
            IAuthService authService) 
        {
            _paymentService = paymentService;
            _commandeRepository = commandeRepository;
            _paymentRepository = paymentRepository;
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> Process(int commandeId, string? moyenPaiement = null)
        {
            Console.WriteLine($"=== PROCESS Paiement pour commande {commandeId} ===");
            
            if (!_authService.IsAuthenticated())
            {
                Console.WriteLine("Non authentifié");
                TempData["ErrorMessage"] = "Veuillez vous connecter.";
                return RedirectToAction("Login", "Account");
            }

            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                Console.WriteLine("Utilisateur null");
                TempData["ErrorMessage"] = "Utilisateur non trouvé.";
                return RedirectToAction("Login", "Account");
            }
            
            Console.WriteLine($"Utilisateur: {user.Email} (ID: {user.Id})");

            var commande = await _commandeRepository.GetByIdAsync(commandeId);
            
            if (commande == null)
            {
                Console.WriteLine($"Commande {commandeId} non trouvée");
                TempData["ErrorMessage"] = "Commande non trouvée.";
                return RedirectToAction("Index", "Home");
            }
            
            Console.WriteLine($"Commande trouvée: ID={commande.Id}, Statut={commande.Statut}, Montant={commande.MontantTotal}");

            if (commande.UserId != user.Id)
            {
                Console.WriteLine($"Commande appartient à {commande.UserId}, utilisateur est {user.Id}");
                TempData["ErrorMessage"] = "Cette commande ne vous appartient pas.";
                return RedirectToAction("Orders", "Profile");
            }

            if (commande.Statut == "TERMINEE")
            {
                Console.WriteLine($"Commande déjà TERMINEE");
                TempData["WarningMessage"] = "Cette commande est déjà payée.";
                return RedirectToAction("Details", "Order", new { id = commandeId });
            }

            if (commande.Statut == "ANNULEE" || commande.Statut == "LIVREE")
            {
                Console.WriteLine($"Commande déjà traitée: {commande.Statut}");
                TempData["WarningMessage"] = "Cette commande a déjà été traitée.";
                return RedirectToAction("Details", "Order", new { id = commandeId });
            }

            if (commande.Statut != "EN_ATTENTE")
            {
                Console.WriteLine($"Mauvais statut: {commande.Statut} (attendu: EN_ATTENTE)");
                TempData["ErrorMessage"] = "Cette commande ne peut plus être payée.";
                return RedirectToAction("Details", "Order", new { id = commandeId });
            }

            if (string.IsNullOrEmpty(moyenPaiement))
            {
                moyenPaiement = "ESPECES";
            }
            
            Console.WriteLine($"Moyen de paiement: {moyenPaiement}");
            
            var existingPayments = await _paymentRepository.GetPaymentsByOrderIdAsync(commandeId);
            var alreadyPaid = existingPayments?.Any(p => p.StatutPaiement == "PAYE") ?? false;
            
            if (alreadyPaid)
            {
                Console.WriteLine($"Paiement déjà existant et PAYE - Synchronisation de la commande");
                
                
                if (commande.Statut != "TERMINEE")
                {
                    commande.Statut = "TERMINEE";
                    commande.UpdatedAt = DateTime.UtcNow;
                    await _commandeRepository.UpdateAsync(commande);
                    Console.WriteLine($"Commande synchronisée: EN_ATTENTE -> TERMINEE");
                }
                
                TempData["SuccessMessage"] = "Cette commande était déjà payée. Statut synchronisé.";
                return RedirectToAction("Details", "Order", new { id = commandeId });
            }

            Console.WriteLine($"Création du paiement via PaymentService...");
            var payment = await _paymentService.CreatePaymentAsync(commandeId, commande.MontantTotal, moyenPaiement);
            if (payment == null)
            {
                Console.WriteLine($"PaymentService a retourné null");
                TempData["ErrorMessage"] = "Erreur lors de la création du paiement.";
                return RedirectToAction("Details", "Order", new { id = commandeId });
            }

            Console.WriteLine($"Paiement créé: ID={payment.Id}, Réf={payment.ReferenceTransaction}, Statut={payment.StatutPaiement}");

            TempData["PaymentId"] = payment.Id;
            TempData["CommandeId"] = commandeId;

            Console.WriteLine($"Redirection selon moyen de paiement: {moyenPaiement}");
            return RedirectBasedOnPaymentMethod(moyenPaiement, payment, commande);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Confirm(string referenceTransaction)
        {
            Console.WriteLine($"=== CONFIRM Paiement Réf: {referenceTransaction} ===");
            
            if (string.IsNullOrEmpty(referenceTransaction))
            {
                Console.WriteLine("Référence vide");
                TempData["ErrorMessage"] = "Référence de transaction invalide.";
                return RedirectToAction("Index", "Home");
            }

            Console.WriteLine($"🔍 Validation du paiement...");
            var success = await _paymentService.ValidatePaymentAsync(referenceTransaction);
            
            if (success)
            {
                Console.WriteLine($"Paiement validé avec succès");
                var payment = await _paymentRepository.GetByReferenceAsync(referenceTransaction);
                if (payment != null)
                {
                    Console.WriteLine($"Paiement trouvé: ID={payment.Id}, Commande={payment.CommandeId}");
                    var commande = await _commandeRepository.GetByIdAsync(payment.CommandeId);
                    
                    
                    if (commande != null && commande.Statut != "TERMINEE")
                    {
                        Console.WriteLine($"Mise à jour commande: {commande.Id} ({commande.Statut}) -> TERMINEE");
                        commande.Statut = "TERMINEE";
                        commande.UpdatedAt = DateTime.UtcNow;
                        await _commandeRepository.UpdateAsync(commande);
                    }

                    Console.WriteLine($"Mise à jour paiement: DatePaiement, Statut=PAYE");
                    payment.DatePaiement = DateTime.UtcNow;
                    payment.StatutPaiement = "PAYE";
                    await _paymentRepository.UpdateAsync(payment);
                }

                TempData["SuccessMessage"] = "Paiement confirmé avec succès ! Votre commande est terminée.";
                return RedirectToAction("Details", "Order", new { id = payment?.CommandeId ?? 0 });
            }
            else
            {
                Console.WriteLine($"Échec validation paiement");
                TempData["ErrorMessage"] = "Erreur lors de la confirmation du paiement.";
                return View("PaymentError");
            }
        }

        [HttpGet]
        public async Task<IActionResult> CancelPayment(int paymentId)
        {
            Console.WriteLine($"=== CANCEL Paiement ID: {paymentId} ===");
            
            if (!_authService.IsAuthenticated())
            {
                Console.WriteLine("Non authentifié");
                TempData["ErrorMessage"] = "Veuillez vous connecter.";
                return RedirectToAction("Login", "Account");
            }

            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                Console.WriteLine("Utilisateur null");
                TempData["ErrorMessage"] = "Utilisateur non trouvé.";
                return RedirectToAction("Login", "Account");
            }

            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            
            if (payment == null)
            {
                Console.WriteLine($"Paiement {paymentId} non trouvé");
                TempData["ErrorMessage"] = "Paiement non trouvé.";
                return RedirectToAction("Index", "Home");
            }

            Console.WriteLine($"Paiement trouvé: ID={payment.Id}, Commande={payment.CommandeId}, Statut={payment.StatutPaiement}");

            var commande = await _commandeRepository.GetByIdAsync(payment.CommandeId);
            if (commande == null || commande.UserId != user.Id)
            {
                Console.WriteLine($"Commande n'appartient pas à l'utilisateur");
                TempData["ErrorMessage"] = "Ce paiement ne vous appartient pas.";
                return RedirectToAction("Orders", "Profile");
            }

            if (payment.StatutPaiement == "PAYE")
            {
                Console.WriteLine($"Paiement déjà PAYE, impossible d'annuler");
                TempData["ErrorMessage"] = "Ce paiement a déjà été validé.";
                return RedirectToAction("Details", "Order", new { id = payment.CommandeId });
            }

            Console.WriteLine($"Mise à jour paiement: Statut=ECHEC");
            payment.StatutPaiement = "ECHEC";
            await _paymentRepository.UpdateAsync(payment);

            TempData["InfoMessage"] = "Paiement annulé. Vous pouvez réessayer ultérieurement.";
            return RedirectToAction("Payment", "Order", new { orderId = payment.CommandeId });
        }

        [HttpGet]
        public async Task<IActionResult> Status(int paymentId)
        {
            Console.WriteLine($"=== STATUS Paiement ID: {paymentId} ===");
            
            if (!_authService.IsAuthenticated())
            {
                Console.WriteLine("Non authentifié");
                TempData["ErrorMessage"] = "Veuillez vous connecter.";
                return RedirectToAction("Login", "Account");
            }

            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                Console.WriteLine("Utilisateur null");
                TempData["ErrorMessage"] = "Utilisateur non trouvé.";
                return RedirectToAction("Login", "Account");
            }

            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            
            if (payment == null)
            {
                Console.WriteLine($"Paiement {paymentId} non trouvé");
                return NotFound();
            }

            var commande = await _commandeRepository.GetByIdAsync(payment.CommandeId);
            if (commande == null || commande.UserId != user.Id)
            {
                Console.WriteLine($"Commande n'appartient pas à l'utilisateur");
                return Forbid("Ce paiement ne vous appartient pas.");
            }

            Console.WriteLine($"Affichage statut pour paiement {paymentId}");
            ViewData["Title"] = "Statut du paiement - Brasil Burger";
            return View(payment);
        }

        [HttpGet]
        public async Task<IActionResult> ManualPayment(int commandeId)
        {
            Console.WriteLine($"=== MANUAL Paiement pour commande {commandeId} ===");
            
            if (!_authService.IsAuthenticated())
            {
                Console.WriteLine("Non authentifié");
                TempData["ErrorMessage"] = "Veuillez vous connecter.";
                return RedirectToAction("Login", "Account");
            }

            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                Console.WriteLine("Utilisateur null");
                TempData["ErrorMessage"] = "Utilisateur non trouvé.";
                return RedirectToAction("Login", "Account");
            }

            var commande = await _commandeRepository.GetByIdAsync(commandeId);
            
            if (commande == null)
            {
                Console.WriteLine($"Commande {commandeId} non trouvée");
                TempData["ErrorMessage"] = "Commande non trouvée.";
                return RedirectToAction("Index", "Home");
            }

            if (commande.UserId != user.Id)
            {
                Console.WriteLine($"Commande n'appartient pas à l'utilisateur");
                TempData["ErrorMessage"] = "Cette commande ne vous appartient pas.";
                return RedirectToAction("Orders", "Profile");
            }

            if (commande.Statut != "EN_ATTENTE")
            {
                Console.WriteLine($"Mauvais statut: {commande.Statut}");
                TempData["ErrorMessage"] = "Cette commande ne peut plus être payée.";
                return RedirectToAction("Details", "Order", new { id = commandeId });
            }

            var reference = _paymentService.GeneratePaymentReference();
            Console.WriteLine($"Référence générée: {reference}");

            ViewData["Title"] = "Instructions de paiement - Brasil Burger";
            ViewData["Commande"] = commande;
            ViewData["Reference"] = reference;

            return View("ManualPayment");
        }

        [HttpGet]
        public async Task<IActionResult> CashPayment(int commandeId)
        {
            Console.WriteLine($"=== CASH Paiement pour commande {commandeId} ===");
            
            if (!_authService.IsAuthenticated())
            {
                Console.WriteLine("Non authentifié");
                TempData["ErrorMessage"] = "Veuillez vous connecter.";
                return RedirectToAction("Login", "Account");
            }

            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                Console.WriteLine("Utilisateur null");
                TempData["ErrorMessage"] = "Utilisateur non trouvé.";
                return RedirectToAction("Login", "Account");
            }

            var commande = await _commandeRepository.GetByIdAsync(commandeId);
            
            if (commande == null)
            {
                Console.WriteLine($"Commande {commandeId} non trouvée");
                TempData["ErrorMessage"] = "Commande non trouvée.";
                return RedirectToAction("Index", "Home");
            }

            if (commande.UserId != user.Id)
            {
                Console.WriteLine($"Commande n'appartient pas à l'utilisateur");
                TempData["ErrorMessage"] = "Cette commande ne vous appartient pas.";
                return RedirectToAction("Orders", "Profile");
            }

            if (commande.Statut != "EN_ATTENTE")
            {
                Console.WriteLine($"Mauvais statut: {commande.Statut}");
                TempData["ErrorMessage"] = "Cette commande ne peut plus être payée.";
                return RedirectToAction("Details", "Order", new { id = commandeId });
            }

            var existingPayments = await _paymentRepository.GetPaymentsByOrderIdAsync(commandeId);
            var alreadyPaid = existingPayments?.Any(p => p.StatutPaiement == "PAYE") ?? false;
            
            if (alreadyPaid)
            {
                Console.WriteLine($"Paiement déjà existant - Synchronisation");
                
                
                if (commande.Statut != "TERMINEE")
                {
                    commande.Statut = "TERMINEE";
                    commande.UpdatedAt = DateTime.UtcNow;
                    await _commandeRepository.UpdateAsync(commande);
                    Console.WriteLine($"Commande synchronisée: TERMINEE");
                }
                
                TempData["SuccessMessage"] = "Cette commande était déjà payée. Statut synchronisé.";
                return RedirectToAction("Details", "Order", new { id = commandeId });
            }

            Console.WriteLine($"Affichage page paiement espèces");
            ViewData["Title"] = "Paiement en espèces - Brasil Burger";
            ViewData["CommandeId"] = commandeId;
            ViewData["Commande"] = commande;
            ViewData["MontantTotal"] = commande.MontantTotal;
            
            return View(commandeId);
        }

        [HttpGet]
        public async Task<IActionResult> CardPayment(int commandeId)
        {
            Console.WriteLine($"=== CARD Paiement pour commande {commandeId} ===");
            
            if (!_authService.IsAuthenticated())
            {
                Console.WriteLine("Non authentifié");
                TempData["ErrorMessage"] = "Veuillez vous connecter.";
                return RedirectToAction("Login", "Account");
            }

            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                Console.WriteLine("Utilisateur null");
                TempData["ErrorMessage"] = "Utilisateur non trouvé.";
                return RedirectToAction("Login", "Account");
            }

            var commande = await _commandeRepository.GetByIdAsync(commandeId);
            
            if (commande == null)
            {
                Console.WriteLine($"Commande {commandeId} non trouvée");
                TempData["ErrorMessage"] = "Commande non trouvée.";
                return RedirectToAction("Index", "Home");
            }

            if (commande.UserId != user.Id)
            {
                Console.WriteLine($"Commande n'appartient pas à l'utilisateur");
                TempData["ErrorMessage"] = "Cette commande ne vous appartient pas.";
                return RedirectToAction("Orders", "Profile");
            }

            if (commande.Statut != "EN_ATTENTE")
            {
                Console.WriteLine($"Mauvais statut: {commande.Statut}");
                TempData["ErrorMessage"] = "Cette commande ne peut plus être payée.";
                return RedirectToAction("Details", "Order", new { id = commandeId });
            }

            var existingPayments = await _paymentRepository.GetPaymentsByOrderIdAsync(commandeId);
            var alreadyPaid = existingPayments?.Any(p => p.StatutPaiement == "PAYE") ?? false;
            
            if (alreadyPaid)
            {
                Console.WriteLine($"Paiement déjà existant - Synchronisation");
                
                
                if (commande.Statut != "TERMINEE")
                {
                    commande.Statut = "TERMINEE";
                    commande.UpdatedAt = DateTime.UtcNow;
                    await _commandeRepository.UpdateAsync(commande);
                    Console.WriteLine($"Commande synchronisée: TERMINEE");
                }
                
                TempData["SuccessMessage"] = "Cette commande était déjà payée. Statut synchronisé.";
                return RedirectToAction("Details", "Order", new { id = commandeId });
            }

            Console.WriteLine($"Affichage page paiement carte");
            ViewData["Title"] = "Paiement par carte - Brasil Burger";
            ViewData["CommandeId"] = commandeId;
            ViewData["Commande"] = commande;
            ViewData["MontantTotal"] = commande.MontantTotal;
            
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> MobilePayment(int commandeId, string provider)
        {
            Console.WriteLine($"=== MOBILE Paiement {provider} pour commande {commandeId} ===");
            
            if (!_authService.IsAuthenticated())
            {
                Console.WriteLine("Non authentifié");
                TempData["ErrorMessage"] = "Veuillez vous connecter.";
                return RedirectToAction("Login", "Account");
            }

            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                Console.WriteLine("Utilisateur null");
                TempData["ErrorMessage"] = "Utilisateur non trouvé.";
                return RedirectToAction("Login", "Account");
            }

            var commande = await _commandeRepository.GetByIdAsync(commandeId);
            
            if (commande == null)
            {
                Console.WriteLine($"Commande {commandeId} non trouvée");
                TempData["ErrorMessage"] = "Commande non trouvée.";
                return RedirectToAction("Index", "Home");
            }

            if (commande.UserId != user.Id)
            {
                Console.WriteLine($"Commande n'appartient pas à l'utilisateur");
                TempData["ErrorMessage"] = "Cette commande ne vous appartient pas.";
                return RedirectToAction("Orders", "Profile");
            }

            if (commande.Statut != "EN_ATTENTE")
            {
                Console.WriteLine($"Mauvais statut: {commande.Statut}");
                TempData["ErrorMessage"] = "Cette commande ne peut plus être payée.";
                return RedirectToAction("Details", "Order", new { id = commandeId });
            }

            var existingPayments = await _paymentRepository.GetPaymentsByOrderIdAsync(commandeId);
            var alreadyPaid = existingPayments?.Any(p => p.StatutPaiement == "PAYE") ?? false;
            
            if (alreadyPaid)
            {
                Console.WriteLine($"Paiement déjà existant - Synchronisation");
                
                
                if (commande.Statut != "TERMINEE")
                {
                    commande.Statut = "TERMINEE";
                    commande.UpdatedAt = DateTime.UtcNow;
                    await _commandeRepository.UpdateAsync(commande);
                    Console.WriteLine($"Commande synchronisée: TERMINEE");
                }
                
                TempData["SuccessMessage"] = "Cette commande était déjà payée. Statut synchronisé.";
                return RedirectToAction("Details", "Order", new { id = commandeId });
            }

            Console.WriteLine($"Affichage page paiement mobile {provider}");
            ViewData["Title"] = $"Paiement {provider} - Brasil Burger";
            ViewData["CommandeId"] = commandeId;
            ViewData["Commande"] = commande;
            ViewData["Provider"] = provider;
            ViewData["MontantTotal"] = commande.MontantTotal;
            
            return View("MobilePayment");
        }

    
        [HttpGet]
        public async Task<IActionResult> CompleteCashPayment(int commandeId)
        {
            try
            {
                Console.WriteLine($"=== COMPLETE CASH Paiement pour commande {commandeId} ===");
                Console.WriteLine($"Début: {DateTime.Now:HH:mm:ss}");
                
                
                if (!_authService.IsAuthenticated())
                {
                    Console.WriteLine($"Non authentifié");
                    TempData["ErrorMessage"] = "Veuillez vous connecter.";
                    return RedirectToAction("Login", "Account");
                }

                var user = await _authService.GetCurrentUserAsync();
                if (user == null)
                {
                    Console.WriteLine($"Utilisateur null");
                    TempData["ErrorMessage"] = "Utilisateur non trouvé.";
                    return RedirectToAction("Login", "Account");
                }
                
                Console.WriteLine($"Utilisateur: {user.Email} (ID: {user.Id})");

                var commande = await _commandeRepository.GetByIdAsync(commandeId);
                
                if (commande == null)
                {
                    Console.WriteLine($"Commande {commandeId} non trouvée");
                    TempData["ErrorMessage"] = "Commande non trouvée.";
                    return RedirectToAction("Index", "Home");
                }
                
                Console.WriteLine($"Commande trouvée: ID={commande.Id}, Statut={commande.Statut}, UserId={commande.UserId}, Montant={commande.MontantTotal}");

                
                if (commande.UserId != user.Id)
                {
                    Console.WriteLine($"Commande appartient à {commande.UserId}, utilisateur est {user.Id}");
                    TempData["ErrorMessage"] = "Cette commande ne vous appartient pas.";
                    return RedirectToAction("Orders", "Profile");
                }

                
                if (commande.Statut == "TERMINEE")
                {
                    Console.WriteLine($"Commande déjà TERMINEE");
                    TempData["WarningMessage"] = "Cette commande est déjà payée.";
                    return RedirectToAction("Details", "Order", new { id = commandeId });
                }

                if (commande.Statut == "ANNULEE" || commande.Statut == "LIVREE")
                {
                    Console.WriteLine($"Commande déjà traitée: {commande.Statut}");
                    TempData["WarningMessage"] = "Cette commande a déjà été traitée.";
                    return RedirectToAction("Details", "Order", new { id = commandeId });
                }

                if (commande.Statut != "EN_ATTENTE")
                {
                    Console.WriteLine($"Mauvais statut: {commande.Statut} (attendu: EN_ATTENTE)");
                    TempData["ErrorMessage"] = "Cette commande ne peut plus être payée.";
                    return RedirectToAction("Details", "Order", new { id = commandeId });
                }

                
                var existingPayments = await _paymentRepository.GetPaymentsByOrderIdAsync(commandeId);
                var paidPayment = existingPayments?.FirstOrDefault(p => p.StatutPaiement == "PAYE");
                var pendingPayment = existingPayments?.FirstOrDefault(p => p.StatutPaiement == "EN_ATTENTE");

                
                if (paidPayment != null)
                {
                    Console.WriteLine($"Paiement déjà PAYE trouvé: ID={paidPayment.Id}, Réf={paidPayment.ReferenceTransaction}");
                    Console.WriteLine($"Synchronisation nécessaire: Commande={commande.Statut} -> TERMINEE");
                    
                    commande.Statut = "TERMINEE";
                    commande.UpdatedAt = DateTime.UtcNow;
                    await _commandeRepository.UpdateAsync(commande);
                    
                    Console.WriteLine($"Commande synchronisée: TERMINEE");
                    TempData["SuccessMessage"] = "Cette commande était déjà payée. Statut synchronisé !";
                    return RedirectToAction("Details", "Order", new { id = commandeId });
                }
                
                
                if (pendingPayment != null)
                {
                    Console.WriteLine($"Paiement en attente trouvé: ID={pendingPayment.Id}");
                    
                    
                    pendingPayment.StatutPaiement = "PAYE";
                    pendingPayment.DatePaiement = DateTime.UtcNow;
                    pendingPayment.MoyenPaiement = "ESPECES";
                    await _paymentRepository.UpdateAsync(pendingPayment);
                    
                    
                    commande.Statut = "TERMINEE";
                    commande.UpdatedAt = DateTime.UtcNow;
                    await _commandeRepository.UpdateAsync(commande);
                    
                    Console.WriteLine($"Paiement en attente mis à jour et commande TERMINEE");
                    Console.WriteLine($"Référence: {pendingPayment.ReferenceTransaction}");
                    
                    TempData["SuccessMessage"] = "Paiement en espèces enregistré ! Votre commande est terminée.";
                    return RedirectToAction("Details", "Order", new { id = commandeId });
                }
                
            
                Console.WriteLine($"Aucun paiement existant, création d'un nouveau...");
                
                var payment = new Payment
                {
                    CommandeId = commandeId,
                    Montant = commande.MontantTotal,
                    MoyenPaiement = "ESPECES",
                    ReferenceTransaction = _paymentService.GeneratePaymentReference(),
                    StatutPaiement = "PAYE",
                    DatePaiement = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                
                Console.WriteLine($"Création nouveau paiement: Réf={payment.ReferenceTransaction}");
                
                try
                {
                    await _paymentRepository.CreateAsync(payment);
                    Console.WriteLine($"Paiement créé avec succès: ID={payment.Id}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERREUR création paiement: {ex.Message}");
                    Console.WriteLine($"StackTrace: {ex.StackTrace}");
                    throw;
                }


                Console.WriteLine($"Mise à jour du statut de la commande...");
                Console.WriteLine($"Ancien statut: {commande.Statut}");
                
                commande.Statut = "TERMINEE";
                commande.UpdatedAt = DateTime.UtcNow;
                
                try
                {
                    await _commandeRepository.UpdateAsync(commande);
                    Console.WriteLine($"Commande {commandeId} marquée comme TERMINEE");
                    Console.WriteLine($"Nouveau statut: {commande.Statut}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERREUR mise à jour commande: {ex.Message}");
                    throw;
                }

                Console.WriteLine($"Paiement terminé avec succès pour commande {commandeId}");
                Console.WriteLine($"Fin: {DateTime.Now:HH:mm:ss}");
                Console.WriteLine($"=== FIN COMPLETE CASH Paiement ===");

                TempData["SuccessMessage"] = "Paiement en espèces enregistré ! Votre commande est terminée.";
                return RedirectToAction("Details", "Order", new { id = commandeId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERREUR CRITIQUE CompleteCashPayment: {ex.Message}");
                Console.WriteLine($"Heure erreur: {DateTime.Now:HH:mm:ss}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"INNER EXCEPTION: {ex.InnerException.Message}");
                    Console.WriteLine($"INNER StackTrace: {ex.InnerException.StackTrace}");
                }
                
                TempData["ErrorMessage"] = $"Une erreur est survenue : {ex.Message}";
                return RedirectToAction("Details", "Order", new { id = commandeId });
            }
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteCashPaymentPost(int commandeId)
        {
            Console.WriteLine($"=== COMPLETE CASH POST pour commande {commandeId} ===");
            return await CompleteCashPayment(commandeId);
        }

        
        [HttpGet]
        public IActionResult QuickPayment(int commandeId)
        {
            Console.WriteLine($"=== QUICK PAYMENT pour commande {commandeId} ===");
            return RedirectToAction("CompleteCashPayment", new { commandeId });
        }

        
        [HttpGet]
        public IActionResult Test()
        {
            Console.WriteLine("=== TEST PaymentController ===");
            return Content("PaymentController fonctionne !");
        }

        

        private IActionResult RedirectBasedOnPaymentMethod(string moyenPaiement, Payment payment, Commande commande)
        {
            Console.WriteLine($"RedirectionBasedOnPaymentMethod: {moyenPaiement}");
            
            switch (moyenPaiement.ToUpper())
            {
                case "WAVE":
                case "ORANGE_MONEY":
                    Console.WriteLine($"📱 Redirection vers MobilePayment: {moyenPaiement}");
                    return RedirectToAction("MobilePayment", new {
                        commandeId = commande.Id, 
                        provider = moyenPaiement 
                    });

                case "CARTE":
                    Console.WriteLine($"Redirection vers CardPayment");
                    return RedirectToAction("CardPayment", new { commandeId = commande.Id });

                case "ESPECES":
                    Console.WriteLine($"Redirection vers CashPayment");
                    return RedirectToAction("CashPayment", new { commandeId = commande.Id });

                default:
                    Console.WriteLine($"Redirection vers ManualPayment");
                    return RedirectToAction("ManualPayment", new { commandeId = commande.Id });
            }
        }

        
        [HttpGet]
        public async Task<IActionResult> SimulateSuccess(int commandeId)
        {
            Console.WriteLine($"=== SIMULATE SUCCESS pour commande {commandeId} ===");
            
            if (!_authService.IsAuthenticated())
            {
                Console.WriteLine("Non authentifié");
                TempData["ErrorMessage"] = "Veuillez vous connecter.";
                return RedirectToAction("Login", "Account");
            }

            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                Console.WriteLine("Utilisateur null");
                TempData["ErrorMessage"] = "Utilisateur non trouvé.";
                return RedirectToAction("Login", "Account");
            }

            var commande = await _commandeRepository.GetByIdAsync(commandeId);
            
            if (commande == null)
            {
                Console.WriteLine($"Commande {commandeId} non trouvée");
                TempData["ErrorMessage"] = "Commande non trouvée.";
                return RedirectToAction("Index", "Home");
            }

            if (commande.UserId != user.Id)
            {
                Console.WriteLine($"Commande n'appartient pas à l'utilisateur");
                TempData["ErrorMessage"] = "Cette commande ne vous appartient pas.";
                return RedirectToAction("Orders", "Profile");
            }

            if (commande.Statut != "EN_ATTENTE")
            {
                Console.WriteLine($"Mauvais statut: {commande.Statut}");
                TempData["ErrorMessage"] = "Cette commande ne peut plus être payée.";
                return RedirectToAction("Details", "Order", new { id = commandeId });
            }

            
            var existingPayments = await _paymentRepository.GetPaymentsByOrderIdAsync(commandeId);
            var alreadyPaid = existingPayments?.Any(p => p.StatutPaiement == "PAYE") ?? false;
            
            if (alreadyPaid)
            {
                Console.WriteLine($"Paiement déjà existant - Synchronisation");
                
                if (commande.Statut != "TERMINEE")
                {
                    commande.Statut = "TERMINEE";
                    commande.UpdatedAt = DateTime.UtcNow;
                    await _commandeRepository.UpdateAsync(commande);
                    Console.WriteLine($"Commande synchronisée: TERMINEE");
                }
                
                TempData["SuccessMessage"] = "Cette commande était déjà payée. Statut synchronisé.";
                return RedirectToAction("Details", "Order", new { id = commandeId });
            }

            
            var reference = _paymentService.GeneratePaymentReference();
            Console.WriteLine($"Création paiement simulé: {reference}");
            
            var payment = new Payment
            {
                CommandeId = commandeId,
                Montant = commande.MontantTotal,
                MoyenPaiement = "ESPECES",
                ReferenceTransaction = reference,
                StatutPaiement = "PAYE",
                DatePaiement = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _paymentRepository.CreateAsync(payment);
            Console.WriteLine($"Paiement simulé créé: ID={payment.Id}");

            
            commande.Statut = "TERMINEE";
            commande.UpdatedAt = DateTime.UtcNow;
            
            await _commandeRepository.UpdateAsync(commande);
            Console.WriteLine($"Commande mise à jour: TERMINEE");

            TempData["SuccessMessage"] = "Paiement simulé avec succès ! Votre commande est terminée.";
            return RedirectToAction("Details", "Order", new { id = commandeId });
        }
    }
}