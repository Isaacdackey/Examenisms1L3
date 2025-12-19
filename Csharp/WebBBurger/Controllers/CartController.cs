using WebBBurger.Models.ViewModels;
using WebBBurger.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebBBurger.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IProductService _productService;

        public CartController(ICartService cartService, IProductService productService)
        {
            _cartService = cartService;
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Panier - Brasil Burger";
            var cart = await _cartService.GetCartAsync();
            return View(cart);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Cart/AddToCart")]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.IsArchived)
            {
                TempData["ErrorMessage"] = "Produit non disponible.";
                return RedirectToAction("Index", "Home");
            }

            
            if (product.TypeProduct == "BURGER")
            {
                TempData["ErrorMessage"] = "Pour les burgers, utilisez la page de personnalisation.";
                return RedirectToAction("AddBurgerWithComplements", "Home", new { id = productId });
            }

            await _cartService.AddToCartAsync(productId, quantity);
            TempData["SuccessMessage"] = $"{product.Libelle} ajouté au panier.";
            
            return RedirectToAction("Index");
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Cart/Add")]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            return await AddToCart(productId, quantity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int productId)
        {
            await _cartService.RemoveFromCartAsync(productId);
            TempData["SuccessMessage"] = "Produit retiré du panier.";
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int productId, int quantity)
        {
            if (quantity < 1)
            {
                await _cartService.RemoveFromCartAsync(productId);
            }
            else
            {
                await _cartService.UpdateQuantityAsync(productId, quantity);
            }
            TempData["SuccessMessage"] = "Quantité mise à jour.";
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            await _cartService.ClearCartAsync();
            TempData["SuccessMessage"] = "Panier vidé.";
            
            return RedirectToAction("Index");
        }

        [HttpGet]
        public JsonResult GetCartSummary()
        {
            var count = _cartService.GetCartItemCount();
            var total = _cartService.GetCartTotal();
            
            return Json(new { count, total });
        }
    }
}