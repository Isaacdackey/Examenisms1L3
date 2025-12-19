using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace WebBBurger.Models
{
    [Table("user")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public required string Nom { get; set; }

        [Required]
        [StringLength(50)]
        public required string Prenom { get; set; }

        [Required]
        [StringLength(20)]
        public required string Tel { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }

        public string? Adresse { get; set; }

        [StringLength(100)]
        public string? Quartier { get; set; }

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "CLIENT";

        [StringLength(50)]
        [Column("vehicule")]
        public string? Vehicule { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

    
        public virtual ICollection<Commande> Commandes { get; set; } = new List<Commande>();

        public virtual ICollection<Commande> Livraisons { get; set; } = new List<Commande>();

        
        [NotMapped]
        public string FullName => $"{Prenom} {Nom}";
    }
}