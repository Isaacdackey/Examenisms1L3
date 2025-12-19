using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBBurger.Models
{
    [Table("zone_livraison")]
    public class ZoneLivraison
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public required string Nom { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrixLivraison { get; set; } = 0;

        [Required]
        public required string QuartiersCouverts { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

    
        public virtual ICollection<Commande> Commandes { get; set; } = new List<Commande>();
    }
}