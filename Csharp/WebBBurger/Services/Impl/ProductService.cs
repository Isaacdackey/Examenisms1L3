using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBBurger.Models;
using WebBBurger.Models.ViewModels;
using WebBBurger.Repositories;
using WebBBurger.Services;
using Microsoft.Extensions.Logging;

namespace WebBBurger.Services.Impl
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IProductRepository productRepository,
            ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _productRepository.GetActiveProductsAsync();
        }

        public async Task<IEnumerable<Product>> GetBurgersAsync()
        {
            return await _productRepository.GetBurgersAsync();
        }

        public async Task<IEnumerable<Product>> GetComplementsAsync()
        {
            return await _productRepository.GetComplementsAsync();
        }

        public async Task<IEnumerable<Product>> GetMenusAsync()
        {
            return await _productRepository.GetMenusAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _productRepository.GetByIdAsync(id);
        }

        public async Task<ProductViewModel?> GetProductViewModelAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return null;

            var viewModel = new ProductViewModel
            {
                Id = product.Id,
                Libelle = product.Libelle,
                Description = product.Description ?? "",
                Prix = product.Prix,
                ImageUrl = product.CloudinaryUrl,
                TypeProduct = product.TypeProduct,
                IsArchived = product.IsArchived
            };

            switch (product.TypeProduct)
            {
                case "BURGER":
                    if (product.Burger != null)
                    {
                        viewModel.TypeBurger = product.Burger.TypeBurger;
                        viewModel.Calories = product.Burger.Calories;
                        viewModel.TempsPreparation = product.Burger.TempsPreparation;
                    }
                    break;

                case "COMPLEMENT":
                    if (product.Complement != null)
                    {
                        viewModel.TypeComplement = product.Complement.TypeComplement;
                        viewModel.Volume = product.Complement.Volume;
                    }
                    break;

                case "MENU":
                    if (product.Menu != null)
                    {
                        viewModel.ReductionPourcentage = product.Menu.ReductionPourcentage;
                        
                        var burgerPrice = product.Menu.Burger?.Product?.Prix ?? 0;
                        var boissonPrice = product.Menu.Boisson?.Product?.Prix ?? 0;
                        var fritesPrice = product.Menu.Frites?.Product?.Prix ?? 0;
                        viewModel.PrixOriginal = burgerPrice + boissonPrice + fritesPrice;
                        
                        viewModel.BurgerName = product.Menu.Burger?.Product?.Libelle;
                        viewModel.BoissonName = product.Menu.Boisson?.Product?.Libelle;
                        viewModel.FritesName = product.Menu.Frites?.Product?.Libelle;
                    }
                    break;
            }

            return viewModel;
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string query, string? type = null)
        {
            var products = await _productRepository.GetActiveProductsAsync();
            
            if (!string.IsNullOrEmpty(query))
            {
                query = query.ToLower();
                products = products.Where(p => 
                    p.Libelle.ToLower().Contains(query) || 
                    (p.Description != null && p.Description.ToLower().Contains(query)))
                    .ToList();
            }

            if (!string.IsNullOrEmpty(type))
            {
                products = products.Where(p => p.TypeProduct == type).ToList();
            }

            return products;
        }

        
        public async Task<IEnumerable<Product>> GetProductsByTypeAsync(string type)
        {
            _logger.LogInformation("GetProductsByTypeAsync appelé avec type: {Type}", type);
            
            var products = await _productRepository.GetByTypeAsync(type);
            
            _logger.LogInformation("Retour de {Count} produits de type {Type}", products.Count(), type);
            
            return products;
        }

        public async Task<IEnumerable<Product>> GetBoissonsAsync()
        {
            var complements = await GetComplementsAsync();
            return complements.Where(p => p.Complement?.TypeComplement == "BOISSON");
        }

        public async Task<IEnumerable<Product>> GetFritesAsync()
        {
            var complements = await GetComplementsAsync();
            return complements.Where(p => p.Complement?.TypeComplement == "FRITE");
        }
    }
}