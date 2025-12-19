using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBBurger.Models
{
    [Table("ligne_commande")]
    public class LigneCommande
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int CommandeId { get; set; }

        [Required]
        public int ProduitId { get; set; }

        [Required]
        public int Quantite { get; set; } = 1;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrixUnitaire { get; set; }

        
        [Column(TypeName = "decimal(10,2)")]
        public decimal? SousTotal { get; set; } = null;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        
        [ForeignKey("CommandeId")]
        public Commande Commande { get; set; } = null!;
        
        [ForeignKey("ProduitId")]
        public Product Produit { get; set; } = null!;

        
        [NotMapped]
        public decimal CalculatedSousTotal => Quantite * PrixUnitaire;
    }
}