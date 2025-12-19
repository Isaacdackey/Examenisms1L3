using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBBurger.Models
{
    [Table("commande")]
    public class Commande
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [StringLength(30)]
        public string? Numero { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string Adresse { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal MontantTotal { get; set; } = 0;

        [Required]
        [StringLength(20)]
        public string Statut { get; set; } = "EN_ATTENTE";

        [Required]
        [StringLength(20)]
        public string TypeRetrait { get; set; } = "LIVRAISON";

        public int? ZoneId { get; set; }
        public int? LivreurId { get; set; }
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }


        
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        
        [ForeignKey("ZoneId")]
        public virtual ZoneLivraison? Zone { get; set; }
        
        [ForeignKey("LivreurId")]
        public virtual User? Livreur { get; set; }
        
        public virtual ICollection<LigneCommande> LigneCommandes { get; set; } = new List<LigneCommande>();
        
        
        public virtual Payment? Payment { get; set; }

    
        [NotMapped]
        public User Client => User;

        
        [NotMapped]
        public string StatutDisplay => GetStatutDisplay(Statut);

        
        [NotMapped]
        public string MoyenPaiement => Payment?.MoyenPaiement ?? "ESPECES";

        private static string GetStatutDisplay(string statut)
        {
            return statut switch
            {
                "EN_ATTENTE" => "En attente",
                "VALIDEE" => "Validée",
                "EN_PREPARATION" => "En préparation",
                "PRETE" => "Prête",
                "EN_LIVRAISON" => "En livraison",
                "LIVREE" => "Livrée",
                "TERMINEE" => "Terminée",
                "ANNULEE" => "Annulée",
                _ => statut
            };
        }

        
        public void GenerateNumero()
        {
            if (string.IsNullOrEmpty(Numero))
            {
                Numero = $"CMD{DateTime.Now:yyyyMMddHHmmss}{Id:0000}";
            }
        }
    }
}