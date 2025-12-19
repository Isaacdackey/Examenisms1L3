namespace WebBBurger.Models.Enums
{
    public enum PaymentMethod
    {
        WAVE,
        ORANGE_MONEY,
        CARTE,
        ESPECES
    }

    public static class PaymentMethodExtensions
    {
        public static string ToDatabaseValue(this PaymentMethod method)
        {
            return method.ToString();
        }

        public static PaymentMethod FromDatabaseValue(string value)
        {
            return Enum.Parse<PaymentMethod>(value);
        }
    }
}