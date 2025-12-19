using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using WebBBurger.Models;
using WebBBurger.Models.ViewModels;
using WebBBurger.Services;

namespace WebBBurger.Services.Impl
{
    public class CartService : ICartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProductService _productService;
        private const string CartSessionKey = "Cart";


        private class CartItemSession
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
            
            var cartItems = GetCartItems();
            var existingItem = cartItems.FirstOrDefault(i => i.ProductId == item.ProductId);
            
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantite;
            }
            else
            {
                cartItems.Add(new CartItemSession
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantite
                });
            }
            
            SaveCartItems(cartItems);
            await Task.CompletedTask;
        }

        public async Task AddToCartAsync(int productId, int quantity = 1)
        {
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

        public async Task AddBurgerWithComplementsAsync(int burgerId, int quantity, 
            int? boissonId = null, int? fritesId = null)
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

        public async Task RemoveFromCartAsync(int productId)
        {
            var cartItems = GetCartItems();
            var itemToRemove = cartItems.FirstOrDefault(item => item.ProductId == productId);
            
            if (itemToRemove != null)
            {
                cartItems.Remove(itemToRemove);
                SaveCartItems(cartItems);
            }
            
            await Task.CompletedTask;
        }

        public async Task UpdateQuantityAsync(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                await RemoveFromCartAsync(productId);
                return;
            }

            var cartItems = GetCartItems();
            var existingItem = cartItems.FirstOrDefault(item => item.ProductId == productId);
            
            if (existingItem != null)
            {
                existingItem.Quantity = quantity;
                SaveCartItems(cartItems);
            }
            
            await Task.CompletedTask;
        }

        public async Task ClearCartAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                httpContext.Session.Remove(CartSessionKey);
            }
            
            await Task.CompletedTask;
        }

        public int GetCartItemCount()
        {
            var cartItems = GetCartItems();
            return cartItems.Sum(item => item.Quantity);
        }

        public decimal GetCartTotal()
        {
            var cartItems = GetCartItems();
            decimal total = 0;
            
            foreach (var item in cartItems)
            {
                var product = _productService.GetProductByIdAsync(item.ProductId).GetAwaiter().GetResult();
                if (product != null && !product.IsArchived)
                {
                    total += product.Prix * item.Quantity;
                }
            }
            
            return total;
        }

        
        private List<CartItemSession> GetCartItems()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null || httpContext.Session == null) 
                return new List<CartItemSession>();

            var cartJson = httpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson))
                return new List<CartItemSession>();

            return JsonSerializer.Deserialize<List<CartItemSession>>(cartJson) ?? new List<CartItemSession>();
        }

        private void SaveCartItems(List<CartItemSession> cartItems)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null && httpContext.Session != null)
            {
                var cartJson = JsonSerializer.Serialize(cartItems);
                httpContext.Session.SetString(CartSessionKey, cartJson);
            }
        }

        
        private List<CartItemViewModel> GetCartFromSession()
        {
            var cartItems = GetCartItems();
            var result = new List<CartItemViewModel>();
            
            foreach (var item in cartItems)
            {
                var product = _productService.GetProductByIdAsync(item.ProductId).GetAwaiter().GetResult();
                if (product != null)
                {
                    result.Add(new CartItemViewModel
                    {
                        ProductId = item.ProductId,
                        Quantite = item.Quantity,
                        Libelle = product.Libelle ?? "Produit sans nom",
                        Prix = product.Prix
                    });
                }
            }
            
            return result;
        }

        private void SaveCartToSession(List<CartItemViewModel> cartItems)
        {
            
            var sessionItems = cartItems.Select(item => new CartItemSession
            {
                ProductId = item.ProductId,
                Quantity = item.Quantite
            }).ToList();
            
            SaveCartItems(sessionItems);
        }
    }
}