namespace WebBBurger.Models.ViewModels
{
    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string Libelle { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public decimal Prix { get; set; }
        public int Quantite { get; set; } 
        public string? ImageUrl { get; set; }
        public string? TypeProduct { get; set; } = string.Empty;
        public decimal SousTotal => Prix * Quantite;
    }
}