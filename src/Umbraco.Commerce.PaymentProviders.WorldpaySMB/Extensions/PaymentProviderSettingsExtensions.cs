using System;
using System.Collections.Generic;
using System.Text.Json;
using Umbraco.Commerce.PaymentProviders.WorldpaySMB.Converters;


namespace Umbraco.Commerce.PaymentProviders.WorldpaySMB.Extensions;

public static class PaymentProviderSettingsExtensions
{
    public static TSettings As<TSettings>(this IReadOnlyDictionary<string, string> providerSettings) where TSettings : class, new()
    {
        ArgumentNullException.ThrowIfNull(providerSettings);
        var settingsJson = JsonSerializer.Serialize(providerSettings);
        var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new BooleanConverter());
        return JsonSerializer.Deserialize<TSettings>(settingsJson, options) ?? new TSettings();
    }
}
