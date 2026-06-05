using System;
using System.Collections.Generic;
using System.Text;

namespace Umbraco.Commerce.PaymentProviders.WorldpaySMB;

public static class WorldpaySMBConstants
{
    public static class Urls
    {
        public const string LiveBaseUrl = "https://access.worldpay.com";
        public const string TestBaseUrl = "https://try.access.worldpay.com";
    }

    public static class Client
    {
        public const string AuthenticationType = "Basic";
        public const string TransactionReferenceAlias = "worldpayTransactionReference";
        public const string OrderReferenceAlias = "orderReference";

        public static class Headers
        {
            public const string ContentType = "application/vnd.worldpay.payment_pages-v1.hal+json";
        }
    }
}
