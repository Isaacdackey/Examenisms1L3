using System.ComponentModel.DataAnnotations;

namespace WebBBurger.Models.ViewModels
{
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Le nom est requis")]
        [Display(Name = "Nom")]
        [StringLength(50, ErrorMessage = "Le nom ne peut pas dépasser 50 caractères")]
        public string? Nom { get; set; }

        [Required(ErrorMessage = "Le prénom est requis")]
        [Display(Name = "Prénom")]
        [StringLength(50, ErrorMessage = "Le prénom ne peut pas dépasser 50 caractères")]
        public string? Prenom { get; set; }

        [Required(ErrorMessage = "Le téléphone est requis")]
        [Display(Name = "Téléphone")]
        [Phone(ErrorMessage = "Numéro de téléphone invalide")]
        [StringLength(20, ErrorMessage = "Le téléphone ne peut pas dépasser 20 caractères")]
        public string? Tel { get; set; }

        [Required(ErrorMessage = "L'email est requis")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Adresse email invalide")]
        [StringLength(100, ErrorMessage = "L'email ne peut pas dépasser 100 caractères")]
        public string? Email { get; set; }

        [Display(Name = "Adresse")]
        public string? Adresse { get; set; }

        [Display(Name = "Quartier")]
        [StringLength(100, ErrorMessage = "Le quartier ne peut pas dépasser 100 caractères")]
        public string? Quartier { get; set; }
    }
}