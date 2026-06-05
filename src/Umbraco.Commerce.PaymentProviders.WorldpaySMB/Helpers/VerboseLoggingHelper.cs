using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Umbraco.Commerce.PaymentProviders.WorldpaySMB.Helpers;

public static class VerboseLoggingHelper
{
    public static string ToFriendlyString<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        IEnumerable<string> items = from kvp in dictionary
                                    select kvp.Key + "=" + kvp.Value;

        return "{" + string.Join(",", items) + "}";
    }

    public static string ToFriendlyString(this NameValueCollection nameValueCollection)
    {
        ArgumentNullException.ThrowIfNull(nameValueCollection);

        var nvc = nameValueCollection.AllKeys.SelectMany(nameValueCollection.GetValues, (k, v) => new { key = k, value = v });
        IEnumerable<string> items = from item in nvc
                                    select item.key + "=" + item.value;

        return "{" + string.Join(",", items) + "}";
    }

    public static string ToFriendlyString(this IFormCollection formData)
    {
        ArgumentNullException.ThrowIfNull(formData);

        IEnumerable<string> items = from kvp in formData
                                    select kvp.Key + "=" + kvp.Value;

        return "{" + string.Join(",", items) + "}";
    }
}
