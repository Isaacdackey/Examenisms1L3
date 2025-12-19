namespace WebBBurger.Models.Enums
{
    public enum UserRole
    {
        GESTIONNAIRE,
        CLIENT,
        LIVREUR
    }

    public static class UserRoleExtensions
    {
        public static string ToDatabaseValue(this UserRole role)
        {
            return role.ToString();
        }

        public static UserRole FromDatabaseValue(string value)
        {
            return Enum.Parse<UserRole>(value);
        }
    }
}