using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.PaymentProviders;
using Umbraco.Commerce.Extensions;
using Umbraco.Commerce.PaymentProviders.WorldpaySMB.Api.Models;

namespace Umbraco.Commerce.PaymentProviders.WorldpaySMB;

public abstract class WorldpaySMBPaymentProviderBase : PaymentProviderBase<WorldpaySMBSettings>
{
    protected WorldpaySMBPaymentProviderBase(UmbracoCommerceContext ctx)
        : base(ctx)
    { }

    public override string GetCancelUrl(PaymentProviderContext<WorldpaySMBSettings> ctx)
    {
        ctx.Settings.MustNotBeNull("ctx.Settings");
        ctx.Settings.CancelUrl.MustNotBeNull("ctx.Settings.CancelUrl");

        return ctx.Settings.CancelUrl;
    }

    public override string GetContinueUrl(PaymentProviderContext<WorldpaySMBSettings> ctx) =>
        $"{WorldpaySMBConstants.Urls.ApiReturnUrl}/{ctx.Order.PaymentInfo.PaymentMethodId}/{ctx.Order.Id}";

    public override string GetErrorUrl(PaymentProviderContext<WorldpaySMBSettings> ctx)
    {
        ctx.Settings.MustNotBeNull("ctx.Settings");
        ctx.Settings.ErrorUrl.MustNotBeNull("ctx.Settings.ErrorUrl");

        return ctx.Settings.ErrorUrl;
    }
}
