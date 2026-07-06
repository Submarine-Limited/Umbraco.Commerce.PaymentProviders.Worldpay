using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Commerce.Common.Logging;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Core.Services;
using Umbraco.Commerce.Core.Session;
using Umbraco.Commerce.PaymentProviders.WorldpaySMB.Api.Models;
using Umbraco.Commerce.PaymentProviders.WorldpaySMB.Extensions;
using Umbraco.Commerce.PaymentProviders.WorldpaySMB.Helpers;

namespace Umbraco.Commerce.PaymentProviders.WorldpaySMB.Api;


[ApiController]
[Route("/api/v1/payments/worldpay-smb")]
[ApiExplorerSettings(GroupName = "Worldpay SMB")]
public class PaymentController(
    IPaymentMethodService paymentMethodService,
    IOrderService orderService,
    IStoreService storeService,
    IOrderStatusService orderStatusService,
    ILogger<WorldpaySMBPaymentProvider> logger,
    IUmbracoCommerceApi umbracoCommerceApi,
    ISessionManager sessionManager
) : Controller
{

    [HttpGet("callback/{paymentMethodId}/{orderId}")]
    public async Task<IActionResult> CallbackAsync(Guid paymentMethodId, Guid orderId, CancellationToken cancellationToken)
    {
        logger.Info($"Hit the callback {DateTime.Now}");

        var errorRedirectUrl = "/checkout/customer-information/?status=error";

        try
        {
            var paymentMethod = await paymentMethodService.GetPaymentMethodAsync(paymentMethodId).ConfigureAwait(false);

            if (paymentMethod == null)
            {
                logger.Error($"No payment method could be found with id {paymentMethodId}");
                return Redirect("/");
            }

            // Get the order from the id
            var order = await orderService.GetOrderAsync(orderId).ConfigureAwait(false);

            if (order == null)
            {
                logger.Error($"No order could be found with id {orderId}");
                return Redirect("/");
            }

            if (order.TransactionInfo == null || string.IsNullOrWhiteSpace(order.OrderNumber))
            {
                logger.Error($"Cannot process order {orderId} as the transaction has not been initialised");
                return Redirect("/checkout");
            }

            // Get the transaction reference from the order
            var transactionRef = order.Properties.FirstOrDefault(x => x.Key == WorldpaySMBConstants.Client.TransactionReferenceAlias).Value.ToString();

            if (string.IsNullOrWhiteSpace(transactionRef))
            {
                logger.Error($"No transaction reference was found for order {orderId}");
                return Redirect(errorRedirectUrl);
            }

            var worldpaySettings = paymentMethod.PaymentProviderSettings.As<WorldpaySMBSettings>();

            var clientConfig = WorldpaySMBClientHelper.GetWorldpayClientConfig(worldpaySettings);
            var client = new WorldpayClient(logger, clientConfig);

            // We need to poll the transaction periodically until we get data back from Worldpay.
            // Sometimes it hasn't authorised by the time we return to the Continute Url
            WorldpaySMBTransaction transaction = null;
            var attempts = 0;
            var maxAttempts = 20;

            while (transaction == null && attempts < maxAttempts)
            {
                attempts++;
                var transactionAttempt = await client.QueryTransactionAsync(transactionRef, cancellationToken).ConfigureAwait(false);

                if (transactionAttempt != null && transactionAttempt.EmbeddedData.Payments.Count > 0)
                {
                    transaction = transactionAttempt;
                }
                else
                {
                    await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                }
            }

            if (transaction == null)
            {
                logger.Error($"Transaction could not be retrieved from Worldpay after {attempts}/{maxAttempts} attempts");
                return Redirect(errorRedirectUrl);
            }

            if (worldpaySettings.VerboseLogging)
            {
                logger.Info($"Transaction retrieved from Worldpay after {attempts}/{maxAttempts} attempts");
                logger.Info($"Worldpay transaction data {JsonSerializer.Serialize(transaction)}");
            }

            var paymentId = transaction.EmbeddedData.Payments.FirstOrDefault()?.PaymentId;
            if (string.IsNullOrWhiteSpace(paymentId))
            {
                logger.Error("Payment Id could not be found from the transaction");
                return Redirect(errorRedirectUrl);
            }

            // Now we have the payment id, we need to poll Worldpay for the payment status
            WorldpaySMBPayment payment = null;
            attempts = 0;
            List<string> processableEvents = ["authorizationSucceeded", "authorizationRefused", "authorizationFailed", "authorizationTimedOut"];

            while (payment == null && attempts < maxAttempts)
            {
                attempts++;

                try
                {
                    var paymentAttempt = await client.QueryPaymentAsync(paymentId, cancellationToken).ConfigureAwait(false);
                }
                catch { }

                if (paymentAttempt != null && paymentAttempt.Events.Any(x => processableEvents.Contains(x.EventName)))
                {
                    payment = paymentAttempt;
                }
                else
                {
                    await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                }
            }

            if (payment == null)
            {
                logger.Error($"Payment could not be retrieved from Worldpay after {attempts}/{maxAttempts} attempts");
                return Redirect(errorRedirectUrl);
            }

            if (worldpaySettings.VerboseLogging)
            {
                logger.Info($"Payment retrieved from Worldpay after {attempts}/{maxAttempts} attempts");
                logger.Info($"Worldpay payment data {JsonSerializer.Serialize(payment)}");
            }

            // Get the latest processable event
            var lastEvent = payment.Events.Where(x => processableEvents.Contains(x.EventName)).OrderByDescending(x => x.Timestamp).FirstOrDefault();

            if (lastEvent.EventName == "authorizationSucceeded")
            {
                return Redirect(worldpaySettings.SuccessUrl);
            }
            else
            {
                // Uninitialise the transaction here so we can keep the cart around
                await umbracoCommerceApi.Uow.ExecuteAsync(
                    async (uow, ct) =>
                    {
                        var writableOrder = await order.AsWritableAsync(uow).ConfigureAwait(false);
                        await writableOrder.UpdateTransactionAsync(0m, null, PaymentStatus.Initialized).ConfigureAwait(false);
                        await orderService.SaveOrderAsync(writableOrder, ct).ConfigureAwait(false);
                    },
                    cancellationToken)
                .ConfigureAwait(false);

                await sessionManager.SetCurrentOrderAsync(order.StoreId, order.Id).ConfigureAwait(false);

                return Redirect(worldpaySettings.ErrorUrl);
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex);
            return Redirect(errorRedirectUrl);
        }
    }
}
