using WebBBurger.Models.ViewModels;
using WebBBurger.Services;
using WebBBurger.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using WebBBurger.Models;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace WebBBurger.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IOrderService _orderService;
        private readonly IUserRepository _userRepository;

        public ProfileController(
            IAuthService authService,
            IOrderService orderService,
            IUserRepository userRepository)
        {
            _authService = authService;
            _orderService = orderService;
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!_authService.IsAuthenticated())
            {
                TempData["ErrorMessage"] = "Veuillez vous connecter pour accéder à votre profil.";
                return RedirectToAction("Login", "Account");
            }

            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                _authService.Logout();
                return RedirectToAction("Login", "Account");
            }

            
            try
            {
                var orders = await _orderService.GetUserOrdersAsync(user.Id);
                ViewBag.OrderCount = orders?.Count() ?? 0;
                ViewBag.TotalSpent = orders?.Sum(o => o.MontantTotal) ?? 0;
                ViewBag.LastOrderDate = orders?.OrderByDescending(o => o.CreatedAt)
                    .FirstOrDefault()?.CreatedAt;
                ViewBag.LastOrderStatus = orders?.OrderByDescending(o => o.CreatedAt)
                    .FirstOrDefault()?.Statut;
            }
            catch (Exception ex)
            {
                
                ViewBag.OrderCount = 0;
                ViewBag.TotalSpent = 0;
                ViewBag.LastOrderDate = null;
                ViewBag.LastOrderStatus = null;
                Console.WriteLine($"Erreur récupération commandes: {ex.Message}");
            }

            ViewData["Title"] = "Mon Profil - Brasil Burger";
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
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

            var model = new EditProfileViewModel
            {
                Nom = user.Nom,
                Prenom = user.Prenom,
                Tel = user.Tel,
                Email = user.Email,
                Adresse = user.Adresse,
                Quartier = user.Quartier
            };

            ViewData["Title"] = "Modifier mon profil - Brasil Burger";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (!_authService.IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Modifier mon profil - Brasil Burger";
                return View(model);
            }

            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            
            if (user.Email != model.Email)
            {
                var existingUser = await _userRepository.GetByEmailAsync(model.Email ?? "");
                if (existingUser != null && existingUser.Id != user.Id)
                {
                    ModelState.AddModelError("Email", "Cet email est déjà utilisé.");
                    ViewData["Title"] = "Modifier mon profil - Brasil Burger";
                    return View(model);
                }
            }


            if (user.Tel != model.Tel)
            {
                var existingUser = await _userRepository.GetUserByPhoneAsync(model.Tel ?? "");
                if (existingUser != null && existingUser.Id != user.Id)
                {
                    ModelState.AddModelError("Tel", "Ce numéro de téléphone est déjà utilisé.");
                    ViewData["Title"] = "Modifier mon profil - Brasil Burger";
                    return View(model);
                }
            }

            
            user.Nom = model.Nom ?? user.Nom;
            user.Prenom = model.Prenom ?? user.Prenom;
            user.Tel = model.Tel ?? user.Tel;
            user.Email = model.Email?.ToLower() ?? user.Email;
            user.Adresse = model.Adresse;
            user.Quartier = model.Quartier;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);


            var httpContext = HttpContext;
            if (httpContext != null && httpContext.Session != null)
            {
                var userName = user.FullName ?? $"{user.Prenom} {user.Nom}".Trim();
                httpContext.Session.SetString("UserName", userName);
                httpContext.Session.SetString("UserEmail", user.Email ?? "");
            }

            TempData["SuccessMessage"] = "Votre profil a été mis à jour avec succès.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Orders()
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

            ViewData["Title"] = "Mes Commandes - Brasil Burger";
            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
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

            if (!ModelState.IsValid)
            {
            
                try
                {
                    var orders = await _orderService.GetUserOrdersAsync(user.Id);
                    ViewBag.OrderCount = orders?.Count() ?? 0;
                    ViewBag.TotalSpent = orders?.Sum(o => o.MontantTotal) ?? 0;
                    ViewBag.LastOrderDate = orders?.OrderByDescending(o => o.CreatedAt)
                        .FirstOrDefault()?.CreatedAt;
                }
                catch
                {
                    ViewBag.OrderCount = 0;
                    ViewBag.TotalSpent = 0;
                    ViewBag.LastOrderDate = null;
                }
                
                ViewData["Title"] = "Mon Profil - Brasil Burger";
                return View("Index", user);
            }

        
            if (model.OldPassword != user.Password)
            {
                ModelState.AddModelError("OldPassword", "L'ancien mot de passe est incorrect.");
                
                
                try
                {
                    var orders = await _orderService.GetUserOrdersAsync(user.Id);
                    ViewBag.OrderCount = orders?.Count() ?? 0;
                    ViewBag.TotalSpent = orders?.Sum(o => o.MontantTotal) ?? 0;
                    ViewBag.LastOrderDate = orders?.OrderByDescending(o => o.CreatedAt)
                        .FirstOrDefault()?.CreatedAt;
                }
                catch
                {
                    ViewBag.OrderCount = 0;
                    ViewBag.TotalSpent = 0;
                    ViewBag.LastOrderDate = null;
                }
                
                ViewData["Title"] = "Mon Profil - Brasil Burger";
                return View("Index", user);
            }

            
            user.Password = model.NewPassword ?? "";
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            TempData["SuccessMessage"] = "Votre mot de passe a été changé avec succès.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Privacy()
        {
            ViewData["Title"] = "Confidentialité - Brasil Burger";
            return View();
        }

        [HttpGet]
        public IActionResult Help()
        {
            ViewData["Title"] = "Aide - Brasil Burger";
            return View();
        }
    }

    
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Le nom est requis")]
        [Display(Name = "Nom")]
        [StringLength(50, ErrorMessage = "Le nom ne peut pas dépasser 50 caractères")]
        public string? Nom { get; set; }

        [Required(ErrorMessage = "Le prénom est requis")]
        [Display(Name = "Prénom")]
        [StringLength(50, ErrorMessage = "Le prénom ne peut pas dépasser 50 caractères")]
        public string? Prenom { get; set; }

        [Required(ErrorMessage = "Le téléphone est requis")]
        [Display(Name = "Téléphone")]
        [Phone(ErrorMessage = "Numéro de téléphone invalide")]
        [StringLength(20, ErrorMessage = "Le téléphone ne peut pas dépasser 20 caractères")]
        public string? Tel { get; set; }

        [Required(ErrorMessage = "L'email est requis")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Adresse email invalide")]
        [StringLength(100, ErrorMessage = "L'email ne peut pas dépasser 100 caractères")]
        public string? Email { get; set; }

        [Display(Name = "Adresse")]
        public string? Adresse { get; set; }

        [Display(Name = "Quartier")]
        [StringLength(100, ErrorMessage = "Le quartier ne peut pas dépasser 100 caractères")]
        public string? Quartier { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "L'ancien mot de passe est requis")]
        [Display(Name = "Ancien mot de passe")]
        [DataType(DataType.Password)]
        public string? OldPassword { get; set; }

        [Required(ErrorMessage = "Le nouveau mot de passe est requis")]
        [Display(Name = "Nouveau mot de passe")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
        public string? NewPassword { get; set; }

        [Required(ErrorMessage = "La confirmation du mot de passe est requise")]
        [Display(Name = "Confirmer le nouveau mot de passe")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Les mots de passe ne correspondent pas")]
        public string? ConfirmNewPassword { get; set; }
    }
}