namespace WebBBurger.Models.Enums
{
    public enum OrderStatus
    {
        EN_ATTENTE,
        VALIDEE,
        EN_PREPARATION,
        PRETE,
        EN_LIVRAISON,
        LIVREE,
        TERMINEE,
        ANNULEE
    }

    public static class OrderStatusExtensions
    {
        public static string ToDatabaseValue(this OrderStatus status)
        {
            return status.ToString();
        }

        public static OrderStatus FromDatabaseValue(string value)
        {
            return Enum.Parse<OrderStatus>(value);
        }
    }
}