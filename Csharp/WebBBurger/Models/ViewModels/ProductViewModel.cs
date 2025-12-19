namespace WebBBurger.Models.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string? Libelle { get; set; }
        public string? Description { get; set; }
        public decimal Prix { get; set; }
        public string? ImageUrl { get; set; }
        public string? TypeProduct { get; set; }
        public bool IsArchived { get; set; }
        
        
        public string? TypeBurger { get; set; }
        public int? Calories { get; set; }
        public int? TempsPreparation { get; set; }
        
        
        public string? TypeComplement { get; set; }
        public string? Volume { get; set; }
        
    
        public decimal PrixOriginal { get; set; }
        public decimal ReductionPourcentage { get; set; }
        public string? BurgerName { get; set; }
        public string? BoissonName { get; set; }
        public string? FritesName { get; set; }
    }
}
