using System.ComponentModel.DataAnnotations.Schema;

namespace WebBBurger.Models.ViewModels
{
    public class ZoneLivraisonViewModel
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public decimal PrixLivraison { get; set; }
        public string QuartiersCouverts { get; set; } = string.Empty;
        
        [NotMapped]
        public string DisplayText => $"{Nom} (+{PrixLivraison:C0} FCFA)";
    }
}