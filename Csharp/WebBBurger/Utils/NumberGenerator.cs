using System;


namespace WebBBurger.Utils
{
    public static class NumberGenerator
    {
        public static string GenerateOrderNumber(int nextId)
        {
            var date = DateTime.Now.ToString("yyyyMMdd");
            return $"CMD-{date}-{nextId.ToString().PadLeft(4, '0')}";
        }

        public static string GeneratePaymentReference()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return $"PAY-{timestamp}-{random}";
        }
    }
}