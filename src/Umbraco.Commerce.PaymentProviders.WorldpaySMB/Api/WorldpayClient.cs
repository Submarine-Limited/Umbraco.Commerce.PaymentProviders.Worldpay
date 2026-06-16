using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Umbraco.Commerce.Common.Logging;
using Umbraco.Commerce.PaymentProviders.WorldpaySMB.Api.Models;

namespace Umbraco.Commerce.PaymentProviders.WorldpaySMB.Api;

public class WorldpayClient
{
    private readonly ILogger<WorldpaySMBPaymentProvider> _logger;
    private readonly WorldpaySMBClientConfig _config;

    public WorldpayClient(ILogger<WorldpaySMBPaymentProvider> logger, WorldpaySMBClientConfig config)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public async Task<WorldpaySMBTransactionUrl> CreateTransactionAsync(WorldpaySMBTransactionData data, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PreferredObjectCreationHandling = JsonObjectCreationHandling.Replace
        });

        return await RequestAsync("/payment_pages", WorldpaySMBConstants.Client.Headers.PaymentPagesContentType, async (req, ct) => await req
                .PostJsonAsync(data, cancellationToken: ct)
                .ReceiveJson<WorldpaySMBTransactionUrl>().ConfigureAwait(false),
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<WorldpaySMBTransaction> QueryTransactionAsync(string transactionRef, CancellationToken cancellationToken = default)
    {
        return await RequestAsync($"/paymentQueries/payments?transactionReference={transactionRef}", WorldpaySMBConstants.Client.Headers.PaymentQueriesContentType, async (req, ct) => await req
            .SendAsync(HttpMethod.Get, cancellationToken: ct)
            .ReceiveJson<WorldpaySMBTransaction>().ConfigureAwait(false),
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<WorldpaySMBPayment> QueryPaymentAsync(string paymentId, CancellationToken cancellationToken = default)
    {
        return await RequestAsync($"/paymentQueries/payments/{paymentId}", WorldpaySMBConstants.Client.Headers.PaymentQueriesContentType, async (req, ct) => await req
            .SendAsync(HttpMethod.Get, cancellationToken: ct)
            .ReceiveJson<WorldpaySMBPayment>().ConfigureAwait(false),
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<TResult> RequestAsync<TResult>(string url, string contentType, Func<IFlurlRequest, CancellationToken, Task<TResult>> func, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new FlurlRequest(_config.BaseUrl + url)
                .WithSettings(x => x.JsonSerializer = new CustomFlurlJsonSerializer(new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    PreferredObjectCreationHandling = JsonObjectCreationHandling.Replace
                }))
                .WithHeader("Accept", contentType)
                .WithHeader("Content-Type", contentType)
                .WithBasicAuth(_config.Username, _config.Password);

            return await func.Invoke(request, cancellationToken).ConfigureAwait(false);
        }
        catch (FlurlHttpException ex)
        {
            _logger.Error(ex);

            var errorBody = await ex.GetResponseStringAsync().ConfigureAwait(false);
            throw new WorldpaySMBApiException(ex.StatusCode ?? 0, errorBody, ex);
        }
    }
}
