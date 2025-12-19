using System.ComponentModel.DataAnnotations;

namespace WebBBurger.Models.ViewModels
{
    public class SimulatePaymentViewModel
    {
        [Required]
        public int OrderId { get; set; }
        
        [Required]
        [DataType(DataType.Currency)]
        public decimal Total { get; set; }
        
        [Required]
        public string OrderNumber { get; set; } = "";
        
        [Required]
        public string PaymentMethod { get; set; } = "ESPECES";
    }
}