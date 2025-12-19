using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBBurger.Models
{
    [Table("payment")]
    public class Payment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int CommandeId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Montant { get; set; }

        [Required]
        [StringLength(100)]
        public string ReferenceTransaction { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string MoyenPaiement { get; set; } = "ESPECES";

        [Required]
        [StringLength(20)]
        public string StatutPaiement { get; set; } = "EN_ATTENTE";

        public DateTime? DatePaiement { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        
        [ForeignKey("CommandeId")]
        public virtual Commande Commande { get; set; } = null!;
    }
}