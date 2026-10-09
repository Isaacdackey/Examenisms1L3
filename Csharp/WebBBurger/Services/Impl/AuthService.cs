using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WebBBurger.Models;
using WebBBurger.Models.ViewModels;
using WebBBurger.Repositories;
using WebBBurger.Services;

namespace WebBBurger.Services.Impl
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthService> _logger;

        private const string UserIdSessionKey = "UserId";
        private const string UserNameSessionKey = "UserName";
        private const string UserRoleSessionKey = "UserRole";
        private const string UserEmailSessionKey = "UserEmail";

        public AuthService(
            IUserRepository userRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<(bool Success, string Message, User? User)> RegisterAsync(RegisterViewModel model)
        {
            var email = (model.Email ?? string.Empty).Trim().ToLowerInvariant();
            var phone = (model.Tel ?? string.Empty).Trim();

            if (await _userRepository.EmailExistsAsync(email))
            {
                return (false, "Cet email est déjà utilisé.", null);
            }

            if (await _userRepository.PhoneExistsAsync(phone))
            {
                return (false, "Ce numéro de téléphone est déjà utilisé.", null);
            }

            if (!model.AcceptTerms)
            {
                return (false, "Vous devez accepter les conditions d'utilisation.", null);
            }

            var passwordRaw = model.Password ?? string.Empty;
            if (string.IsNullOrWhiteSpace(passwordRaw) || passwordRaw.Length < 6)
            {
                return (false, "Le mot de passe doit comporter au moins 6 caractères.", null);
            }

            var hashedPassword = BCrypt.Net.BCrypt.EnhancedHashPassword(passwordRaw, 11);

            var user = new User
            {
                Nom = (model.Nom ?? string.Empty).Trim(),
                Prenom = (model.Prenom ?? string.Empty).Trim(),
                Tel = phone,
                Email = email,
                Password = hashedPassword,
                Adresse = model.Adresse?.Trim(),
                Quartier = model.Quartier?.Trim(),
                Role = "CLIENT",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createdUser = await _userRepository.CreateAsync(user);
            _logger.LogInformation("Nouvel utilisateur créé avec succès : ID={UserId}, Email={Email}", createdUser.Id, createdUser.Email);

            return (true, "Inscription réussie ! Vous pouvez maintenant vous connecter.", createdUser);
        }

        public async Task<(bool Success, string Message, User? User)> LoginAsync(LoginViewModel model)
        {
            var email = (model.Email ?? string.Empty).Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
            {
                return (false, "Email ou mot de passe incorrect.", null);
            }

            if (!user.IsActive)
            {
                return (false, "Votre compte est désactivé. Contactez l'administrateur.", null);
            }

            var password = model.Password ?? string.Empty;
            bool isPasswordValid = false;

            // Vérification BCrypt avec migration automatique des anciens mots de passe en clair
            try
            {
                if (user.Password.StartsWith("$2") && BCrypt.Net.BCrypt.EnhancedVerify(password, user.Password))
                {
                    isPasswordValid = true;
                }
                else if (user.Password == password) // Rétro-compatibilité : mot de passe encore en clair
                {
                    isPasswordValid = true;
                    // Mise à niveau transparente du mot de passe en BCrypt
                    user.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(password, 11);
                    user.UpdatedAt = DateTime.UtcNow;
                    await _userRepository.UpdateAsync(user);
                    _logger.LogInformation("Mot de passe migré vers BCrypt pour l'utilisateur ID={UserId}", user.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification du mot de passe pour l'utilisateur ID={UserId}", user.Id);
                if (user.Password == password)
                {
                    isPasswordValid = true;
                }
            }

            if (!isPasswordValid)
            {
                return (false, "Email ou mot de passe incorrect.", null);
            }

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                var userName = user.FullName ?? $"{user.Prenom} {user.Nom}".Trim();
                var userRole = user.Role ?? "CLIENT";

                // 1. Session ASP.NET Core
                if (httpContext.Session != null)
                {
                    httpContext.Session.SetInt32(UserIdSessionKey, user.Id);
                    httpContext.Session.SetString(UserNameSessionKey, userName);
                    httpContext.Session.SetString(UserRoleSessionKey, userRole);
                    httpContext.Session.SetString(UserEmailSessionKey, user.Email);
                }

                // 2. Cookie Authentication Claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, userName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, userRole)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(14) : DateTimeOffset.UtcNow.AddHours(2)
                };

                await httpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);
            }

            _logger.LogInformation("Connexion réussie pour l'utilisateur ID={UserId}", user.Id);
            return (true, "Connexion réussie !", user);
        }

        public async Task LogoutAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                if (httpContext.Session != null)
                {
                    httpContext.Session.Remove(UserIdSessionKey);
                    httpContext.Session.Remove(UserNameSessionKey);
                    httpContext.Session.Remove(UserRoleSessionKey);
                    httpContext.Session.Remove(UserEmailSessionKey);
                    httpContext.Session.Clear();
                }

                await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        }

        public void Logout()
        {
            LogoutAsync().GetAwaiter().GetResult();
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return null;

            return await _userRepository.GetByIdAsync(userId.Value);
        }

        public bool IsAuthenticated()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
                return true;

            return httpContext?.Session?.GetInt32(UserIdSessionKey).HasValue ?? false;
        }

        public string? GetCurrentUserRole()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var roleClaim = httpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
            if (!string.IsNullOrEmpty(roleClaim)) return roleClaim;

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
            var nameClaim = httpContext?.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(nameClaim)) return nameClaim;

            return httpContext?.Session?.GetString(UserNameSessionKey);
        }

        public string? GetCurrentUserEmail()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var emailClaim = httpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(emailClaim)) return emailClaim;

            return httpContext?.Session?.GetString(UserEmailSessionKey);
        }

        public int? GetCurrentUserId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var idClaim = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(idClaim) && int.TryParse(idClaim, out int id))
            {
                return id;
            }

            return httpContext?.Session?.GetInt32(UserIdSessionKey);
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return (false, "Utilisateur non trouvé.");
            }

            bool isCurrentValid = false;
            try
            {
                if (user.Password.StartsWith("$2") && BCrypt.Net.BCrypt.EnhancedVerify(currentPassword, user.Password))
                {
                    isCurrentValid = true;
                }
                else if (user.Password == currentPassword)
                {
                    isCurrentValid = true;
                }
            }
            catch
            {
                isCurrentValid = (user.Password == currentPassword);
            }

            if (!isCurrentValid)
            {
                return (false, "Mot de passe actuel incorrect.");
            }

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                return (false, "Le nouveau mot de passe doit comporter au moins 6 caractères.");
            }

            user.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(newPassword, 11);
            user.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _userRepository.UpdateAsync(user);
                _logger.LogInformation("Mot de passe mis à jour avec succès pour l'utilisateur ID={UserId}", userId);
                return (true, "Mot de passe changé avec succès.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour du mot de passe pour ID={UserId}", userId);
                return (false, $"Erreur lors du changement de mot de passe: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UpdateProfileAsync(int userId, EditProfileViewModel model)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return (false, "Utilisateur introuvable.");
            }

            var newEmail = (model.Email ?? string.Empty).Trim().ToLowerInvariant();
            var newPhone = (model.Tel ?? string.Empty).Trim();

            if (!string.Equals(user.Email, newEmail, StringComparison.OrdinalIgnoreCase))
            {
                var existingEmail = await _userRepository.GetByEmailAsync(newEmail);
                if (existingEmail != null && existingEmail.Id != user.Id)
                {
                    return (false, "Cet email est déjà utilisé par un autre compte.");
                }
            }

            if (!string.Equals(user.Tel, newPhone, StringComparison.OrdinalIgnoreCase))
            {
                var existingPhone = await _userRepository.GetUserByPhoneAsync(newPhone);
                if (existingPhone != null && existingPhone.Id != user.Id)
                {
                    return (false, "Ce numéro de téléphone est déjà utilisé par un autre compte.");
                }
            }

            user.Nom = (model.Nom ?? user.Nom).Trim();
            user.Prenom = (model.Prenom ?? user.Prenom).Trim();
            user.Tel = newPhone;
            user.Email = newEmail;
            user.Adresse = model.Adresse?.Trim();
            user.Quartier = model.Quartier?.Trim();
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Session != null)
            {
                var fullName = user.FullName ?? $"{user.Prenom} {user.Nom}".Trim();
                httpContext.Session.SetString(UserNameSessionKey, fullName);
                httpContext.Session.SetString(UserEmailSessionKey, user.Email);
            }

            return (true, "Profil mis à jour avec succès.");
        }
    }
}