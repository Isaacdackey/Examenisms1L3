using System.Threading.Tasks;
using WebBBurger.Models;
using WebBBurger.Models.ViewModels;

namespace WebBBurger.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, User? User)> RegisterAsync(RegisterViewModel model);
        Task<(bool Success, string Message, User? User)> LoginAsync(LoginViewModel model);
        Task LogoutAsync();
        void Logout();
        Task<User?> GetCurrentUserAsync();
        bool IsAuthenticated();
        string? GetCurrentUserRole();
        bool HasRole(string role);
        string? GetCurrentUserName();
        string? GetCurrentUserEmail();
        int? GetCurrentUserId();
        Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<(bool Success, string Message)> UpdateProfileAsync(int userId, EditProfileViewModel model);
    }
}