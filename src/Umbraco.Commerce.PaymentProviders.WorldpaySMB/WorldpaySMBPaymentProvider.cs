using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Umbraco.Commerce.Common.Logging;
using Umbraco.Commerce.Core;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Core.PaymentProviders;
using Umbraco.Commerce.Core.Services;
using Umbraco.Commerce.Extensions;
using Umbraco.Commerce.PaymentProviders.WorldpaySMB.Api;
using Umbraco.Commerce.PaymentProviders.WorldpaySMB.Api.Models;
using Umbraco.Commerce.PaymentProviders.WorldpaySMB.Helpers;

namespace Umbraco.Commerce.PaymentProviders.WorldpaySMB;

[PaymentProvider("worldpay-smb", Icon = "icon-credit-card")]
public class WorldpaySMBPaymentProvider : WorldpaySMBPaymentProviderBase
{
    private readonly ILogger<WorldpaySMBPaymentProvider> _logger;
    private readonly IOrderService _orderService;

    public override bool FinalizeAtContinueUrl => false;

    public WorldpaySMBPaymentProvider(
        UmbracoCommerceContext ctx,
        ILogger<WorldpaySMBPaymentProvider> logger,
        IOrderService orderService)
        : base(ctx)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));

    }

    public override async Task<PaymentFormResult> GenerateFormAsync(PaymentProviderContext<WorldpaySMBSettings> ctx, CancellationToken cancellationToken = default)
    {
        try
        {
            if (ctx.Settings.VerboseLogging)
            {
                _logger.Info($"GenerateForm method called for cart {ctx.Order.OrderNumber}");
            }

            ctx.Settings.EntityId.MustNotBeNull("ctx.Settings.EntityId");

            var firstname = ctx.Order.CustomerInfo.FirstName;
            var surname = ctx.Order.CustomerInfo.LastName;
            var email = ctx.Order.CustomerInfo.Email;

            if (!string.IsNullOrEmpty(ctx.Settings.BillingFirstNamePropertyAlias))
            {
                firstname = ctx.Order.Properties[ctx.Settings.BillingFirstNamePropertyAlias];
            }

            if (!string.IsNullOrEmpty(ctx.Settings.BillingLastNamePropertyAlias))
            {
                surname = ctx.Order.Properties[ctx.Settings.BillingLastNamePropertyAlias];
            }

            if (!string.IsNullOrEmpty(ctx.Settings.EmailPropertyAlias))
            {
                email = ctx.Order.Properties[ctx.Settings.EmailPropertyAlias];
            }

            var address1 = ctx.Order.Properties[ctx.Settings.BillingAddressLine1PropertyAlias] ?? string.Empty;
            var address2 = ctx.Order.Properties[ctx.Settings.BillingAddressLine2PropertyAlias] ?? string.Empty;
            var city = ctx.Order.Properties[ctx.Settings.BillingAddressCityPropertyAlias] ?? string.Empty;
            var postcode = ctx.Order.Properties[ctx.Settings.BillingAddressZipCodePropertyAlias] ?? string.Empty;
            var billingCountry = await Context.Services.CountryService.GetCountryAsync(ctx.Order.PaymentInfo.CountryId.Value);
            var billingCountryCode = billingCountry.Code.ToUpperInvariant();
            var amount = ctx.Order.TransactionAmount.Value.Value.ToString("0.00", CultureInfo.InvariantCulture);
            _ = decimal.TryParse(amount, out var decimalAmount);
            var currency = await Context.Services.CurrencyService.GetCurrencyAsync(ctx.Order.CurrencyId);
            var currencyCode = currency.Code.ToUpperInvariant();

            // Ensure billing country has valid ISO 3166 code
            var iso3166Countries = await Context.Services.CountryService.GetIso3166CountryRegionsAsync();
            if (iso3166Countries.All(x => x.Code != billingCountryCode))
            {
                throw new Exception("Country must be a valid ISO 3166 billing country code: " + billingCountry.Name);
            }

            // Ensure currency has valid ISO 4217 code
            if (!Iso4217.CurrencyCodes.ContainsKey(currencyCode))
            {
                throw new Exception("Currency must be a valid ISO 4217 currency code: " + currency.Name);
            }

            var store = await CommerceApi.Instance.GetStoreAsync(ctx.Order.StoreId).ConfigureAwait(false);

            WorldpaySMBTransactionData transactionData = new()
            {
                TransactionReference = $"{ctx.Order.Id:N}:{DateTime.UtcNow:yyyyMMddHHmmss}",
                Merchant = new()
                {
                    Entity = ctx.Settings.EntityId,
                },
                Value = new()
                {
                    Currency = currencyCode,
                    Amount = (int)(decimalAmount * 100),
                },
                Narrative = new()
                {
                    Line1 = store.Name,
                },
                Description = ctx.Order.OrderNumber,
                BillingAddress = new()
                {
                    FirstName = firstname,
                    LastName = surname,
                    Address1 = address1,
                    Address2 = address2,
                    City = city,
                    CountryCode = billingCountryCode,
                    PostalCode = postcode,
                },
                ResultURLs = new()
                {
                    SuccessURL = ctx.Urls.ContinueUrl,
                    PendingURL = ctx.Urls.ContinueUrl,
                    FailureURL = ctx.Urls.ErrorUrl,
                    ErrorURL = ctx.Urls.ErrorUrl,
                    CancelURL = ctx.Urls.CancelUrl,
                    ExpiryURL = ctx.Urls.CancelUrl,
                },
                RiskData = new()
                {
                    Account = new()
                    {
                        Email = email,
                    },
                },
                ThreeDS = "disabled",
            };

            var clientConfig = GetWorldpayClientConfig(ctx.Settings);
            var client = new WorldpayClient(_logger, clientConfig);

            var transactionUrl = await client.CreateTransactionAsync(transactionData, cancellationToken).ConfigureAwait(false);

            var callbackUrl = ctx.Urls.CallbackUrl;

            if (ctx.Settings.VerboseLogging)
            {
                _logger.Info($"Payment url {transactionUrl.Url}");
                _logger.Info($"Transaction data {JsonSerializer.Serialize(transactionData)}");
            }

            return new PaymentFormResult
            {
                Form = new PaymentForm(transactionUrl.Url, PaymentFormMethod.Get),
                MetaData = new Dictionary<string, string>
                {
                    { WorldpaySMBConstants.Client.TransactionReferenceAlias, transactionData.TransactionReference },
                    { WorldpaySMBConstants.Client.OrderReferenceAlias, ctx.Order.GenerateOrderReference() }
                },
            };
        }
        catch (Exception e)
        {
            _logger.Error($"Exception thrown for cart {ctx.Order.OrderNumber} - with error {e.Message}");

            throw;
        }
    }

    public override async Task<OrderReference> GetOrderReferenceAsync(PaymentProviderContext<WorldpaySMBSettings> ctx, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(ctx.Settings);

        var body = await ctx.HttpContext.Request.ReadFromJsonAsync<WorldpaySMBWebhookEvent>(cancellationToken).ConfigureAwait(false);

        if (body.EventDetails.Type == "sentForAuthorization")
        {
            return await base.GetOrderReferenceAsync(ctx, cancellationToken).ConfigureAwait(false);
        }

        var bodyString = JsonSerializer.Serialize(body);

        var eventData = new NameValueCollection
        {
            { "authAmount", body.EventDetails.Amount.Value.ToString() },
            { "transId", body.EventDetails.DownstreamReference },
            { "transactionType", body.EventDetails.Type }
        };

        ctx.AdditionalData.Add("eventData", eventData);

        if (ctx.Settings.VerboseLogging)
        {
            _logger.Info($"Worldpay data {bodyString}");
        }

        var id = body.EventDetails.TransactionReference.Split(":")[0];
        if (!Guid.TryParse(id, out Guid orderId))
        {
            return await base.GetOrderReferenceAsync(ctx, cancellationToken).ConfigureAwait(false);
        }

        var order = await _orderService.GetOrderAsync(orderId).ConfigureAwait(false);

        if (order != null)
        {
            return order.GenerateOrderReference();
        }

        return await base.GetOrderReferenceAsync(ctx, cancellationToken).ConfigureAwait(false);
    }

    public override Task<CallbackResult> ProcessCallbackAsync(PaymentProviderContext<WorldpaySMBSettings> ctx, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ctx);

        // The request stream is processed inside GetOrderReferenceAsync and the relevant data
        // is stored in the payment provider context to prevent needing to re-process it
        // so we just access it directly from the context assuming it exists.
        var eventData = ctx.AdditionalData["eventData"] as NameValueCollection;

        _logger.Info($"Payment call back for cart {ctx.Order.OrderNumber}");

        if (ctx.Settings.VerboseLogging)
        {
            _logger.Info($"Worldpay data {eventData.ToFriendlyString()}");
        }

        if (eventData["transactionType"] == "authorized")
        {
            var totalAmount = decimal.Parse(eventData["authAmount"], CultureInfo.InvariantCulture);
            var transactionId = eventData["transId"];

            _logger.Info($"Payment call back for cart {ctx.Order.OrderNumber} payment authorised");

            return Task.FromResult(CallbackResult.Ok(new TransactionInfo
            {
                AmountAuthorized = totalAmount,
                TransactionFee = 0m,
                TransactionId = transactionId,
                PaymentStatus = PaymentStatus.Authorized
            }));
        }
        else
        {
            _logger.Info($"Payment call back for cart {ctx.Order.OrderNumber} payment not authorised or error");
        }

        return Task.FromResult(CallbackResult.Ok());
    }
}
