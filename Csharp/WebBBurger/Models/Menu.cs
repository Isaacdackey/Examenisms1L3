using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBBurger.Models
{
    [Table("menu")]
    public class Menu
    {
        [Key]
        [ForeignKey("Product")]
        public int Id { get; set; }

        [Required]
        public int BurgerId { get; set; }

        [Required]
        public int BoissonId { get; set; }

        [Required]
        public int FritesId { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal ReductionPourcentage { get; set; } = 10.00m;

        
        public required  Product Product { get; set; }
        
        [ForeignKey("BurgerId")]
        public required  Burger Burger { get; set; }
        
        [ForeignKey("BoissonId")]
        public required  Complement Boisson { get; set; }
        
        [ForeignKey("FritesId")]
        public required  Complement Frites { get; set; }

        [NotMapped]
        public decimal PrixMenu
        {
            get
            {
                if (Product == null || Burger?.Product == null || 
                    Boisson?.Product == null || Frites?.Product == null)
                    return 0;

                var total = Burger.Product.Prix + Boisson.Product.Prix + Frites.Product.Prix;
                return total - (total * ReductionPourcentage / 100);
            }
        }
    }
}