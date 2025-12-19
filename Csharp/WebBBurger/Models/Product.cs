using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBBurger.Models
{
    [Table("product")]
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string Libelle { get; set; }

        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Prix { get; set; } = 0;

        public string? CloudinaryUrl { get; set; }
        public string? CloudinaryPublicId { get; set; }

        public bool IsArchived { get; set; } = false;

        [Required]
        [StringLength(20)]
        public required string TypeProduct { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        
        public virtual Burger? Burger { get; set; }
        public virtual Complement? Complement { get; set; }
        public virtual Menu? Menu { get; set; }
        public virtual ICollection<LigneCommande> LigneCommandes { get; set; } = new List<LigneCommande>();
    }
}