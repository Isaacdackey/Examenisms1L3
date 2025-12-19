using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace WebBBurger.Models.ViewModels
{
    public class BurgerDetailsViewModel
    {
        public int Id { get; set; }
        
        
        public string Libelle { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
    
        public decimal Prix { get; set; }
        
        public string? ImageUrl { get; set; }
        public string? TypeBurger { get; set; }
        public int? Calories { get; set; }
        public int? TempsPreparation { get; set; }
        
        
        public List<ProductViewModel> Boissons { get; set; } = new List<ProductViewModel>();
        public List<ProductViewModel> Frites { get; set; } = new List<ProductViewModel>();
        
        
        public int? SelectedBoissonId { get; set; }
        public int? SelectedFritesId { get; set; }
        
        
        [Required(ErrorMessage = "La quantité est requise")]
        [Range(1, 10, ErrorMessage = "La quantité doit être entre 1 et 10")]
        public int Quantity { get; set; } = 1;
        
        
        public decimal PrixTotal
        {
            get
            {
                var total = Prix * Quantity;
                
                if (SelectedBoissonId.HasValue && Boissons != null)
                {
                    var boisson = Boissons.FirstOrDefault(b => b.Id == SelectedBoissonId.Value);
                    if (boisson != null) 
                    {
                        total += boisson.Prix * Quantity;
                    }
                }
                
                if (SelectedFritesId.HasValue && Frites != null)
                {
                    var frites = Frites.FirstOrDefault(f => f.Id == SelectedFritesId.Value);
                    if (frites != null) 
                    {
                        total += frites.Prix * Quantity;
                    }
                }
                
                return total;
            }
        }
    }
}