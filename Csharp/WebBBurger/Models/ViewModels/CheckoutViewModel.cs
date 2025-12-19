using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBBurger.Models.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "L'adresse est requise")]
        [Display(Name = "Adresse de livraison")]
        public string Adresse { get; set; } = string.Empty;

        [Required(ErrorMessage = "Veuillez sélectionner un mode de retrait")]
        [Display(Name = "Mode de retrait")]
        public string TypeRetrait { get; set; } = "SUR_PLACE";

        [Display(Name = "Zone de livraison")]
        public int? ZoneId { get; set; }

        [Display(Name = "Notes pour le restaurant")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "Veuillez sélectionner un moyen de paiement")]
        [Display(Name = "Moyen de paiement")]
        public string MoyenPaiement { get; set; } = "ESPECES";

        public decimal TotalCommande { get; set; }
        public decimal FraisLivraison { get; set; }
        
        [NotMapped]
        public decimal TotalAPayer => TotalCommande + FraisLivraison;

        public List<ZoneLivraisonViewModel> Zones { get; set; } = new List<ZoneLivraisonViewModel>();
        public CartViewModel Panier { get; set; } = new CartViewModel();
    }
}