namespace WebBBurger.Models.Enums
{
    public enum ProductType
    {
        BURGER,
        COMPLEMENT,
        MENU
    }

    public static class ProductTypeExtensions
    {
        public static string ToDatabaseValue(this ProductType type)
        {
            return type.ToString();
        }

        public static ProductType FromDatabaseValue(string value)
        {
            return Enum.Parse<ProductType>(value);
        }
    }
}