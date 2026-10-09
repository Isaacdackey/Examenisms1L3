using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WebBBurger.Models.ViewModels;
using WebBBurger.Services;
using WebBBurger.Utils;

namespace WebBBurger.Services.Impl
{
    public class CartService : ICartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProductService _productService;
        private const string CartSessionKey = "Cart";

        public class CartItemSession
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }

        public CartService(IHttpContextAccessor httpContextAccessor, IProductService productService)
        {
            _httpContextAccessor = httpContextAccessor;
            _productService = productService;
        }

        public async Task AddToCartAsync(CartItemViewModel item)
        {
            await AddToCartAsync(item.ProductId, item.Quantite);
        }

        public async Task AddToCartAsync(int productId, int quantity = 1)
        {
            if (quantity <= 0) return;

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.IsArchived)
                return;

            var cartItems = GetCartItems();
            var existingItem = cartItems.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cartItems.Add(new CartItemSession
                {
                    ProductId = productId,
                    Quantity = quantity
                });
            }

            SaveCartItems(cartItems);
        }

        public async Task AddBurgerWithComplementsAsync(
            int burgerId,
            int quantity,
            int? boissonId = null,
            int? fritesId = null)
        {
            await AddToCartAsync(burgerId, quantity);

            if (boissonId.HasValue)
            {
                await AddToCartAsync(boissonId.Value, quantity);
            }

            if (fritesId.HasValue)
            {
                await AddToCartAsync(fritesId.Value, quantity);
            }
        }

        public async Task<CartViewModel> GetCartAsync()
        {
            var cartItems = GetCartItems();
            var cartViewModel = new CartViewModel
            {
                Items = new List<CartItemViewModel>()
            };

            foreach (var item in cartItems)
            {
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                if (product != null && !product.IsArchived)
                {
                    cartViewModel.Items.Add(new CartItemViewModel
                    {
                        ProductId = product.Id,
                        Libelle = product.Libelle ?? "Produit sans nom",
                        Prix = product.Prix,
                        Quantite = item.Quantity,
                        ImageUrl = product.CloudinaryUrl ?? "/images/default-product.jpg",
                        TypeProduct = product.TypeProduct ?? "PRODUIT",
                        Description = product.Description ?? string.Empty
                    });
                }
            }

            return cartViewModel;
        }

        public Task RemoveFromCartAsync(int productId)
        {
            var cartItems = GetCartItems();
            var itemToRemove = cartItems.FirstOrDefault(item => item.ProductId == productId);

            if (itemToRemove != null)
            {
                cartItems.Remove(itemToRemove);
                SaveCartItems(cartItems);
            }

            return Task.CompletedTask;
        }

        public Task UpdateQuantityAsync(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                return RemoveFromCartAsync(productId);
            }

            var cartItems = GetCartItems();
            var existingItem = cartItems.FirstOrDefault(item => item.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity = quantity;
                SaveCartItems(cartItems);
            }

            return Task.CompletedTask;
        }

        public Task ClearCartAsync()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                session.Remove(CartSessionKey);
            }

            return Task.CompletedTask;
        }

        public int GetCartItemCount()
        {
            var cartItems = GetCartItems();
            return cartItems.Sum(item => item.Quantity);
        }

        public async Task<decimal> GetCartTotalAsync()
        {
            var cart = await GetCartAsync();
            return cart.Total;
        }

        public decimal GetCartTotal()
        {
            // Calcul rapide synchrone sans bloquer avec GetAwaiter si possible,
            // ou délégation asynchrone si le contexte l'exige.
            var cartItems = GetCartItems();
            if (cartItems.Count == 0) return 0;

            return GetCartTotalAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private List<CartItemSession> GetCartItems()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return new List<CartItemSession>();

            return session.Get<List<CartItemSession>>(CartSessionKey) ?? new List<CartItemSession>();
        }

        private void SaveCartItems(List<CartItemSession> cartItems)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                session.Set(CartSessionKey, cartItems);
            }
        }
    }
}