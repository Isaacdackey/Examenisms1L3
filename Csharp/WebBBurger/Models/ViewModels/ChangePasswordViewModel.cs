using System.ComponentModel.DataAnnotations;

namespace WebBBurger.Models.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "L'ancien mot de passe est requis")]
        [Display(Name = "Ancien mot de passe")]
        [DataType(DataType.Password)]
        public string? OldPassword { get; set; }

        [Required(ErrorMessage = "Le nouveau mot de passe est requis")]
        [Display(Name = "Nouveau mot de passe")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
        public string? NewPassword { get; set; }

        [Required(ErrorMessage = "La confirmation du mot de passe est requise")]
        [Display(Name = "Confirmer le nouveau mot de passe")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Les mots de passe ne correspondent pas")]
        public string? ConfirmPassword { get; set; }
    }
}