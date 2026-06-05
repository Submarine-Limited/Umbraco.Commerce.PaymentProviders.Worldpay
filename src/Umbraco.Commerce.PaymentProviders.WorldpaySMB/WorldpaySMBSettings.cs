using Umbraco.Commerce.Core.PaymentProviders;

namespace Umbraco.Commerce.PaymentProviders.WorldpaySMB
{
    public class WorldpaySMBSettings
    {
        [PaymentProviderSetting(SortOrder = 1000)]
        public string ContinueUrl { get; set; }

        [PaymentProviderSetting(SortOrder = 2000)]
        public string CancelUrl { get; set; }

        [PaymentProviderSetting(SortOrder = 3000)]
        public string ErrorUrl { get; set; }

        [PaymentProviderSetting(SortOrder = 4000)]
        public string BillingFirstNamePropertyAlias { get; set; }

        [PaymentProviderSetting(SortOrder = 5000)]
        public string BillingLastNamePropertyAlias { get; set; }

        [PaymentProviderSetting(SortOrder = 6000)]
        public string EmailPropertyAlias { get; set; }

        [PaymentProviderSetting(SortOrder = 7000)]
        public string BillingAddressLine1PropertyAlias { get; set; }

        [PaymentProviderSetting(SortOrder = 8000)]
        public string BillingAddressLine2PropertyAlias { get; set; }

        [PaymentProviderSetting(SortOrder = 9000)]
        public string BillingAddressCityPropertyAlias { get; set; }

        [PaymentProviderSetting(SortOrder = 10000)]
        public string BillingAddressZipCodePropertyAlias { get; set; }


        // ============================
        // Credentials
        // ============================

        [PaymentProviderSetting(SortOrder = 11000)]
        public string EntityId { get; set; }

        [PaymentProviderSetting(SortOrder = 12000)]
        public string TestUsername { get; set; }

        [PaymentProviderSetting(SortOrder = 13000)]
        public string TestPassword { get; set; }

        [PaymentProviderSetting(SortOrder = 14000)]
        public string LiveUsername { get; set; }

        [PaymentProviderSetting(SortOrder = 15000)]
        public string LivePassword { get; set; }

        [PaymentProviderSetting(SortOrder = 17000)]
        public bool TestMode { get; set; }


        // ============================
        // Advanced
        // ============================

        [PaymentProviderSetting(IsAdvanced = true, SortOrder = 18000)]
        public bool VerboseLogging { get; set; }
    }
}
