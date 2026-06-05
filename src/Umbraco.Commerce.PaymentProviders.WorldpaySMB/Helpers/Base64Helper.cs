using System;
using System.Collections.Generic;
using System.Text;

namespace Umbraco.Commerce.PaymentProviders.WorldpaySMB.Helpers;

public static class Base64Helper
{
    public static string Encode(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "";

        var bytes = Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(bytes);
    }

    public static string Decode(string encodedString)
    {
        if (string.IsNullOrWhiteSpace(encodedString))
            return "";

        var bytes = Convert.FromBase64String(encodedString);
        return Encoding.UTF8.GetString(bytes);
    }
}
