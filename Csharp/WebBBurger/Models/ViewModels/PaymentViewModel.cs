using System.ComponentModel.DataAnnotations;

namespace WebBBurger.Models.ViewModels
{
    public class PaymentViewModel
    {
        [Required]
        public int CommandeId { get; set; }
        
        [Required]
        [DataType(DataType.Currency)]
        public decimal Total { get; set; }
        
        [Required]
        public string NumeroCommande { get; set; } = "";
        
        [Required]
        public string MoyenPaiement { get; set; } = "ESPECES";
    }
}