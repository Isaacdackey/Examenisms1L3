using System.ComponentModel.DataAnnotations;

namespace WebBBurger.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "L'email est requis")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Adresse email invalide")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Le mot de passe est requis")]
        [Display(Name = "Mot de passe")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Display(Name = "Se souvenir de moi")]
        public bool RememberMe { get; set; }
    }
}
