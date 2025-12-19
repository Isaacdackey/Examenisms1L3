using WebBBurger.Models.ViewModels;
using WebBBurger.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebBBurger.Models;

namespace WebBBurger.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICartService _cartService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IProductService productService,
            ICartService cartService,
            ILogger<HomeController> logger)
        {
            _productService = productService;
            _cartService = cartService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? type = null, string? search = null)
        {
            _logger.LogInformation("Accès à la page d'accueil - type='{Type}', search='{Search}'", type, search);

            try
            {
                IEnumerable<Product> allProducts;
                
                if (!string.IsNullOrEmpty(type))
                {
                    _logger.LogInformation("Filtrage par type: {Type}", type);
                    
                    string dbType = type.ToUpper() switch
                    {
                        "MENU" => "MENU",
                        "BURGER" => "BURGER",
                        "COMPLEMENT" => "COMPLEMENT",
                        "BOISSON" => "COMPLEMENT",
                        "FRITE" => "COMPLEMENT",
                        _ => type.ToUpper()
                    };
                    
                    allProducts = await _productService.GetProductsByTypeAsync(dbType);
                }
                else if (!string.IsNullOrEmpty(search))
                {
                    _logger.LogInformation("Recherche par texte: {Search}", search);
                    
                    var all = await _productService.GetAllProductsAsync();
                    allProducts = all.Where(p => 
                        (p.Libelle != null && p.Libelle.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                        (p.Description != null && p.Description.Contains(search, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }
                else
                {
                    allProducts = await _productService.GetAllProductsAsync();
                }

                var productViewModels = allProducts.Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Libelle = p.Libelle,
                    Description = p.Description ?? "",
                    Prix = p.Prix,
                    ImageUrl = p.CloudinaryUrl,
                    TypeProduct = p.TypeProduct
                }).ToList();

                
                if (string.IsNullOrEmpty(type) && string.IsNullOrEmpty(search))
                {
                    var burgers = productViewModels.Where(p => p.TypeProduct == "BURGER").ToList();
                    var menus = productViewModels.Where(p => p.TypeProduct == "MENU").ToList();
                    var complements = productViewModels.Where(p => p.TypeProduct == "COMPLEMENT").ToList();

                    ViewBag.Burgers = burgers;
                    ViewBag.Menus = menus;
                    ViewBag.Complements = complements;
                }

                ViewData["Title"] = "Catalogue - Brasil Burger";
                
                
                return View(productViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement de la page d'accueil");
                return View("Error");
            }
        }

        public IActionResult About()
        {
            ViewData["Title"] = "À propos - Brasil Burger";
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Title"] = "Contact - Brasil Burger";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Details(int id)
        {
            _logger.LogInformation("Accès aux détails du produit ID: {ProductId}", id);

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null || product.IsArchived)
            {
                _logger.LogWarning("Produit ID {ProductId} non trouvé ou archivé", id);
                return NotFound();
            }

            if (product.TypeProduct == "BURGER")
            {
                return RedirectToAction("AddBurgerWithComplements", new { id = id });
            }
            else
            {
                var productViewModel = await _productService.GetProductViewModelAsync(id);
                if (productViewModel == null)
                {
                    return NotFound();
                }

                ViewData["Title"] = $"{productViewModel.Libelle} - Brasil Burger";
                return View(productViewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddBurgerWithComplements(int id)
        {
            _logger.LogInformation("GET AddBurgerWithComplements pour ID: {ProductId}", id);

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null || product.IsArchived || product.TypeProduct != "BURGER")
            {
                _logger.LogWarning("Burger ID {ProductId} non trouvé ou non disponible", id);
                TempData["ErrorMessage"] = "Burger non trouvé ou non disponible.";
                return RedirectToAction("Index");
            }

            var complements = await _productService.GetComplementsAsync();
            var boissons = complements.Where(p => p.Complement?.TypeComplement == "BOISSON").ToList();
            var frites = complements.Where(p => p.Complement?.TypeComplement == "FRITE").ToList();

            var viewModel = new BurgerDetailsViewModel
            {
                Id = product.Id,
                Libelle = product.Libelle ?? "Burger",
                Description = product.Description ?? "",
                Prix = product.Prix,
                ImageUrl = product.CloudinaryUrl,
                TypeBurger = product.Burger?.TypeBurger,
                Calories = product.Burger?.Calories,
                TempsPreparation = product.Burger?.TempsPreparation,
                Quantity = 1
            };

            
            viewModel.Boissons = boissons.Select(b => new ProductViewModel
            {
                Id = b.Id,
                Libelle = b.Libelle,
                Description = b.Description ?? "",
                Prix = b.Prix,
                ImageUrl = b.CloudinaryUrl,
                TypeProduct = b.TypeProduct
            }).ToList();

            viewModel.Frites = frites.Select(f => new ProductViewModel
            {
                Id = f.Id,
                Libelle = f.Libelle,
                Description = f.Description ?? "",
                Prix = f.Prix,
                ImageUrl = f.CloudinaryUrl,
                TypeProduct = f.TypeProduct
            }).ToList();

            ViewData["Title"] = $"Personnaliser {viewModel.Libelle} - Brasil Burger";
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBurgerWithComplements([FromForm] BurgerDetailsViewModel model)
        {
            _logger.LogInformation("POST AddBurgerWithComplements démarré");

            
            if (model.Id <= 0 || model.Quantity < 1 || model.Quantity > 10)
            {
                ModelState.AddModelError("", "Données invalides");
            }

        
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState invalide. Erreurs:");
                foreach (var key in ModelState.Keys)
                {
                    if (ModelState.TryGetValue(key, out var state) && state != null && state.Errors.Count > 0)
                    {
                        _logger.LogWarning($"  {key}: {string.Join(", ", state.Errors.Select(e => e.ErrorMessage))}");
                    }
                }
                
                
                await ReloadBurgerData(model);
                return View(model);
            }

            try
            {
                
                var product = await _productService.GetProductByIdAsync(model.Id);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Burger non trouvé.";
                    return RedirectToAction("Index");
                }

                
                await _cartService.AddToCartAsync(model.Id, model.Quantity);

                
                if (model.SelectedBoissonId.HasValue)
                {
                    await _cartService.AddToCartAsync(model.SelectedBoissonId.Value, model.Quantity);
                }

                
                if (model.SelectedFritesId.HasValue)
                {
                    await _cartService.AddToCartAsync(model.SelectedFritesId.Value, model.Quantity);
                }

                TempData["SuccessMessage"] = $"{product.Libelle} ajouté au panier !";
                return RedirectToAction("Index", "Cart");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'ajout au panier");
                TempData["ErrorMessage"] = "Erreur: " + ex.Message;
                await ReloadBurgerData(model);
                return View(model);
            }
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBurgerWithComplementsSimple(int id, int quantity, int? selectedBoissonId, int? selectedFritesId)
        {
            _logger.LogInformation("POST AddBurgerWithComplementsSimple avec paramètres simples");

            try
            {
                
                if (id <= 0 || quantity < 1 || quantity > 10)
                {
                    TempData["ErrorMessage"] = "Données invalides";
                    return RedirectToAction("AddBurgerWithComplements", new { id = id });
                }

                
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null || product.IsArchived || product.TypeProduct != "BURGER")
                {
                    TempData["ErrorMessage"] = "Burger non disponible";
                    return RedirectToAction("Index");
                }

                
                await _cartService.AddToCartAsync(id, quantity);

                
                if (selectedBoissonId.HasValue)
                {
                    await _cartService.AddToCartAsync(selectedBoissonId.Value, quantity);
                }

                
                if (selectedFritesId.HasValue)
                {
                    await _cartService.AddToCartAsync(selectedFritesId.Value, quantity);
                }

                TempData["SuccessMessage"] = $"{product.Libelle} ajouté au panier !";
                return RedirectToAction("Index", "Cart");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'ajout au panier");
                TempData["ErrorMessage"] = "Erreur: " + ex.Message;
                return RedirectToAction("AddBurgerWithComplements", new { id = id });
            }
        }

        private async Task ReloadBurgerData(BurgerDetailsViewModel model)
        {
            var product = await _productService.GetProductByIdAsync(model.Id);
            if (product != null)
            {
                model.Libelle = product.Libelle ?? "Burger";
                model.Prix = product.Prix;
                model.ImageUrl = product.CloudinaryUrl;
                model.Description = product.Description ?? "";
                model.TypeBurger = product.Burger?.TypeBurger;
                model.Calories = product.Burger?.Calories;
                model.TempsPreparation = product.Burger?.TempsPreparation;
            }

            var complements = await _productService.GetComplementsAsync();
            var boissons = complements.Where(p => p.Complement?.TypeComplement == "BOISSON");
            var frites = complements.Where(p => p.Complement?.TypeComplement == "FRITE");

            model.Boissons = boissons.Select(b => new ProductViewModel
            {
                Id = b.Id,
                Libelle = b.Libelle,
                Description = b.Description ?? "",
                Prix = b.Prix,
                ImageUrl = b.CloudinaryUrl,
                TypeProduct = b.TypeProduct
            }).ToList();

            model.Frites = frites.Select(f => new ProductViewModel
            {
                Id = f.Id,
                Libelle = f.Libelle,
                Description = f.Description ?? "",
                Prix = f.Prix,
                ImageUrl = f.CloudinaryUrl,
                TypeProduct = f.TypeProduct
            }).ToList();
        }
    }
}