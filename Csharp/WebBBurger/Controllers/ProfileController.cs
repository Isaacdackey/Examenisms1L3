using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebBBurger.Models;
using WebBBurger.Models.ViewModels;
using WebBBurger.Services;

namespace WebBBurger.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IOrderService _orderService;

        public ProfileController(
            IAuthService authService,
            IOrderService orderService)
        {
            _authService = authService;
            _orderService = orderService;
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
                await _authService.LogoutAsync();
                return RedirectToAction("Login", "Account");
            }

            await PopulateOrderStatisticsAsync(user.Id);

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

            var result = await _authService.UpdateProfileAsync(user.Id, model);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                ViewData["Title"] = "Modifier mon profil - Brasil Burger";
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
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
                await PopulateOrderStatisticsAsync(user.Id);
                ViewData["Title"] = "Mon Profil - Brasil Burger";
                return View("Index", user);
            }

            var result = await _authService.ChangePasswordAsync(
                user.Id,
                model.OldPassword ?? string.Empty,
                model.NewPassword ?? string.Empty);

            if (!result.Success)
            {
                ModelState.AddModelError("OldPassword", result.Message);
                await PopulateOrderStatisticsAsync(user.Id);
                ViewData["Title"] = "Mon Profil - Brasil Burger";
                return View("Index", user);
            }

            TempData["SuccessMessage"] = result.Message;
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

        private async Task PopulateOrderStatisticsAsync(int userId)
        {
            try
            {
                var orders = (await _orderService.GetUserOrdersAsync(userId))?.ToList();
                ViewBag.OrderCount = orders?.Count ?? 0;
                ViewBag.TotalSpent = orders?.Sum(o => o.MontantTotal) ?? 0;
                ViewBag.LastOrderDate = orders?.OrderByDescending(o => o.CreatedAt).FirstOrDefault()?.CreatedAt;
                ViewBag.LastOrderStatus = orders?.OrderByDescending(o => o.CreatedAt).FirstOrDefault()?.Statut;
            }
            catch
            {
                ViewBag.OrderCount = 0;
                ViewBag.TotalSpent = 0;
                ViewBag.LastOrderDate = null;
                ViewBag.LastOrderStatus = null;
            }
        }
    }
}