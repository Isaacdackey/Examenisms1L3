namespace WebBBurger.Models
{
    public class AppSettings
    {
        public required string Name { get; set; }
        public required string Version { get; set; }
        public required string Author { get; set; }
        public required string Description { get; set; }
        public required string DefaultLocale { get; set; }
        public required string Currency { get; set; }
        public required string Timezone { get; set; }
        public required string DateFormat { get; set; }
        
        public required OrderSettings Order { get; set; }
        public required ProductSettings Product { get; set; }
        public required LimitSettings Limits { get; set; }
        public required SecuritySettings Security { get; set; }
        public required DeliverySettings Delivery { get; set; }
        public required PaymentSettings Payment { get; set; }
        public required string Environment { get; set; }
        public required DebugSettings Debug { get; set; }
    }

    public class OrderSettings
    {
        public required string DefaultStatus { get; set; }
        public bool AutoValidate { get; set; }
        public decimal DeliveryDefaultFee { get; set; }
        public required string NumberPrefix { get; set; }
        public required string NumberFormat { get; set; }
        
        
        public OrderSettings()
        {
            DefaultStatus = "EN_ATTENTE";
            AutoValidate = false;
            DeliveryDefaultFee = 500;
            NumberPrefix = "CMD-";
            NumberFormat = "0000";
        }
    }

    public class ProductSettings
    {
        public decimal MenuDiscountPercentage { get; set; }
        public decimal TaxRate { get; set; }
        public bool DefaultArchived { get; set; }
        public int BurgerDefaultPrepTime { get; set; }
        public required string ComplementDefaultVolume { get; set; }
        
        public ProductSettings()
        {
            MenuDiscountPercentage = 10.0m;
            TaxRate = 18.0m;
            DefaultArchived = false;
            BurgerDefaultPrepTime = 15;
            ComplementDefaultVolume = "500ml";
        }
    }

    public class LimitSettings
    {
        public int MaxItemsPerOrder { get; set; }
        public int MaxOrdersPerDay { get; set; }
        public int MaxProductsPerPage { get; set; }
        public int MaxUsersPerPage { get; set; }
        public int MaxCommandesPerPage { get; set; }
        
        public LimitSettings()
        {
            MaxItemsPerOrder = 10;
            MaxOrdersPerDay = 5;
            MaxProductsPerPage = 20;
            MaxUsersPerPage = 50;
            MaxCommandesPerPage = 20;
        }
    }

    public class SecuritySettings
    {
        public int PasswordMinLength { get; set; }
        public int PasswordMaxLength { get; set; }
        public int SessionTimeoutMinutes { get; set; }
        public int JwtExpirationHours { get; set; }
        
        public SecuritySettings()
        {
            PasswordMinLength = 6;
            PasswordMaxLength = 100;
            SessionTimeoutMinutes = 30;
            JwtExpirationHours = 24;
        }
    }

    public class DeliverySettings
    {
        public int ZoneDefaultId { get; set; }
        public int RadiusKm { get; set; }
        public int EstimatedTimeMinutes { get; set; }
        public decimal FreeThreshold { get; set; }
        
        public DeliverySettings()
        {
            ZoneDefaultId = 1;
            RadiusKm = 5;
            EstimatedTimeMinutes = 30;
            FreeThreshold = 10000;
        }
    }

    public class PaymentSettings
    {
        public bool WaveEnabled { get; set; }
        public bool OrangeEnabled { get; set; }
        public bool CashEnabled { get; set; }
        public bool CardEnabled { get; set; }
        public required string DefaultMethod { get; set; }
        public int TimeoutMinutes { get; set; }
        
        public PaymentSettings()
        {
            WaveEnabled = true;
            OrangeEnabled = true;
            CashEnabled = true;
            CardEnabled = false;
            DefaultMethod = "CASH";
            TimeoutMinutes = 15;
        }
    }

    public class DebugSettings
    {
        public bool Enabled { get; set; }
        public required string LoggingLevel { get; set; }
        public bool ShowSqlQueries { get; set; }
        public bool TestDataLoad { get; set; }
        
        public DebugSettings()
        {
            Enabled = false;
            LoggingLevel = "Information";
            ShowSqlQueries = false;
            TestDataLoad = false;
        }
    }
}