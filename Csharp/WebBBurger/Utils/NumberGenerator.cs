using System;

namespace WebBBurger.Utils
{
    public static class NumberGenerator
    {
        public static string GenerateOrderNumber()
        {
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var uniqueSuffix = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
            return $"CMD-{date}-{uniqueSuffix}";
        }

        public static string GenerateOrderNumber(int nextId)
        {
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            return $"CMD-{date}-{nextId:D4}";
        }

        public static string GeneratePaymentReference()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var uniqueSuffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            return $"PAY-{timestamp}-{uniqueSuffix}";
        }
    }
}