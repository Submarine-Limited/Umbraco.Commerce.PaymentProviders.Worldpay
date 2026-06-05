using System;
using System.Collections.Generic;
using System.Text;

namespace Umbraco.Commerce.PaymentProviders.WorldpaySMB.Api;

public class WorldpaySMBApiException : Exception
{
    public int StatusCode { get; }
    public string ResponseBody { get; }

    public WorldpaySMBApiException(int statusCode, string responseBody, Exception innerException)
        : base($"Worldpay SMB API returned {statusCode}: {responseBody}", innerException)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}
