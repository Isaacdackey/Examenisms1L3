using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace WebBBurger.Models
{
    [Table("burger")]
    public class Burger
    {
        [Key]
        [ForeignKey("Product")]
        public int Id { get; set; }

        [StringLength(20)]
        public string? TypeBurger { get; set; }

        public int? Calories { get; set; }
        public int? TempsPreparation { get; set; }

        
        public required  Product Product { get; set; }
    }
}