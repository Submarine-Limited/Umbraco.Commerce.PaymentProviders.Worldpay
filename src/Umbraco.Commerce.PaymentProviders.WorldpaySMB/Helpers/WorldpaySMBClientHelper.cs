using Umbraco.Commerce.PaymentProviders.WorldpaySMB.Api.Models;

namespace Umbraco.Commerce.PaymentProviders.WorldpaySMB.Helpers;

public static class WorldpaySMBClientHelper
{
    public static WorldpaySMBClientConfig GetWorldpayClientConfig(WorldpaySMBSettings settings)
    {
        if (!settings.TryMode)
        {
            return new WorldpaySMBClientConfig(
                settings.LiveModeUsername,
                settings.LiveModePassword,
                WorldpaySMBConstants.Urls.LiveModeBaseUrl);
        }
        else
        {
            return new WorldpaySMBClientConfig(
                settings.TryModeUsername,
                settings.TryModePassword,
                WorldpaySMBConstants.Urls.TryModeBaseUrl);
        }
    }
}
