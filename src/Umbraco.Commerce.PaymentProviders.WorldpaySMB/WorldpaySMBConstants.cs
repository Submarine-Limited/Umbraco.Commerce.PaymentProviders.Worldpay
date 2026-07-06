using System;
using System.Collections.Generic;
using System.Text;

namespace Umbraco.Commerce.PaymentProviders.WorldpaySMB;

public static class WorldpaySMBConstants
{
    public static class Urls
    {
        public const string LiveModeBaseUrl = "https://access.worldpay.com";
        public const string TryModeBaseUrl = "https://try.access.worldpay.com";
        public const string ApiReturnUrl = "/api/v1/payments/worldpay-smb/callback";
    }

    public static class Client
    {
        public const string AuthenticationType = "Basic";
        public const string TransactionReferenceAlias = "worldpayTransactionReference";
        public const string OrderReferenceAlias = "orderReference";

        public static class Headers
        {
            public const string PaymentPagesContentType = "application/vnd.worldpay.payment_pages-v1.hal+json";
            public const string PaymentQueriesContentType = "application/vnd.worldpay.payment-queries-v1.hal+json";
        }
    }
}
