using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace WebBBurger.Models.ViewModels
{
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public decimal Total => Items.Sum(i => i.SousTotal);
        public int TotalItems => Items.Sum(i => i.Quantite);
        public decimal? FraisLivraison { get; set; }
        public decimal TotalAvecLivraison => Total + (FraisLivraison ?? 0);

    
    }
}