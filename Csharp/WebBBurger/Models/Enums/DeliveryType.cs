namespace WebBBurger.Models.Enums
{
    public enum DeliveryType
    {
        SUR_PLACE,
        A_EMPORTER,
        LIVRAISON
    }

    public static class DeliveryTypeExtensions
    {
        public static string ToDatabaseValue(this DeliveryType type)
        {
            return type.ToString();
        }

        public static DeliveryType FromDatabaseValue(string value)
        {
            return Enum.Parse<DeliveryType>(value);
        }
    }
}