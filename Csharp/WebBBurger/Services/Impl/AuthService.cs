using System;
using System.Threading.Tasks;
using WebBBurger.Models;
using WebBBurger.Models.ViewModels;
using WebBBurger.Repositories;
using WebBBurger.Services;
using Microsoft.AspNetCore.Http;

namespace WebBBurger.Services.Impl
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string UserIdSessionKey = "UserId";
        private const string UserNameSessionKey = "UserName";
        private const string UserRoleSessionKey = "UserRole";
        private const string UserEmailSessionKey = "UserEmail";

        public AuthService(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(bool Success, string Message, User? User)> RegisterAsync(RegisterViewModel model)
        {
            
            if (await _userRepository.EmailExistsAsync(model.Email ?? ""))
            {
                return (false, "Cet email est déjà utilisé.", null);
            }

            
            if (await _userRepository.PhoneExistsAsync(model.Tel ?? ""))
            {
                return (false, "Ce numéro de téléphone est déjà utilisé.", null);
            }


            if (!model.AcceptTerms)
            {
                return (false, "Vous devez accepter les conditions d'utilisation.", null);
            }

            var user = new User
            {
                Nom = model.Nom ?? "",
                Prenom = model.Prenom ?? "",
                Tel = model.Tel ?? "",
                Email = model.Email?.ToLower() ?? "",
                Password = model.Password ?? "",
                Adresse = model.Adresse,
                Quartier = model.Quartier,
                Role = "CLIENT",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createdUser = await _userRepository.CreateAsync(user);
            return (true, "Inscription réussie ! Vous pouvez maintenant vous connecter.", createdUser);
        }

        public async Task<(bool Success, string Message, User? User)> LoginAsync(LoginViewModel model)
        {
            var email = model.Email?.ToLower() ?? "";
            var user = await _userRepository.GetByEmailAsync(email);
            
            if (user == null)
            {
                return (false, "Email ou mot de passe incorrect.", null);
            }

            if (!user.IsActive)
            {
                return (false, "Votre compte est désactivé. Contactez l'administrateur.", null);
            }

            var password = model.Password ?? "";
            if (password != user.Password)
            {
                return (false, "Email ou mot de passe incorrect.", null);
            }

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null && httpContext.Session != null)
            {
                httpContext.Session.SetInt32(UserIdSessionKey, user.Id);
                
                var userName = user.FullName ?? $"{user.Prenom} {user.Nom}".Trim();
                httpContext.Session.SetString(UserNameSessionKey, userName);
                
                httpContext.Session.SetString(UserRoleSessionKey, user.Role ?? "CLIENT");
                httpContext.Session.SetString(UserEmailSessionKey, user.Email ?? "");
            }

            return (true, "Connexion réussie !", user);
        }

        public void Logout()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null && httpContext.Session != null)
            {
                httpContext.Session.Remove(UserIdSessionKey);
                httpContext.Session.Remove(UserNameSessionKey);
                httpContext.Session.Remove(UserRoleSessionKey);
                httpContext.Session.Remove(UserEmailSessionKey);
                httpContext.Session.Clear();
            }
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null || httpContext.Session == null) 
                return null;

            var userId = httpContext.Session.GetInt32(UserIdSessionKey);
            if (!userId.HasValue) 
                return null;

            return await _userRepository.GetByIdAsync(userId.Value);
        }

        public bool IsAuthenticated()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null || httpContext.Session == null) 
                return false;

            return httpContext.Session.GetInt32(UserIdSessionKey).HasValue;
        }

        public string? GetCurrentUserRole()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext?.Session?.GetString(UserRoleSessionKey);
        }

        public bool HasRole(string role)
        {
            var currentRole = GetCurrentUserRole();
            return currentRole != null && currentRole.Equals(role, StringComparison.OrdinalIgnoreCase);
        }

        public string? GetCurrentUserName()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext?.Session?.GetString(UserNameSessionKey);
        }

        public string? GetCurrentUserEmail()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext?.Session?.GetString(UserEmailSessionKey);
        }

        public int? GetCurrentUserId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext?.Session?.GetInt32(UserIdSessionKey);
        }

        
        public async Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return (false, "Utilisateur non trouvé.");

            
            if (currentPassword != user.Password)
                return (false, "Mot de passe actuel incorrect.");

            
            user.Password = newPassword;
            user.UpdatedAt = DateTime.UtcNow;
            
            try
            {
                await _userRepository.UpdateAsync(user);
                return (true, "Mot de passe changé avec succès.");
            }
            catch (Exception ex)
            {
                return (false, $"Erreur lors du changement de mot de passe: {ex.Message}");
            }
        }
    }
}