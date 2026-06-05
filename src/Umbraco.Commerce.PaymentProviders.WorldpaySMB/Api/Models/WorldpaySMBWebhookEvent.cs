using System;
using System.Collections.Generic;
using System.Text;

namespace Umbraco.Commerce.PaymentProviders.WorldpaySMB.Api.Models;

public class WorldpaySMBWebhookEvent
{
    public Guid EventId { get; set; }
    public DateTime EventTimestamp { get; set; }
    public WorldpaySMBWebhookEventDetails EventDetails { get; set; }
}

public class WorldpaySMBWebhookEventDetails
{
    public string Classification { get; set; }
    public string DownstreamReference { get; set; }
    public string TransactionReference { get; set; }
    public string Type { get; set; }
    public DateTime Date { get; set; }
    public WorldpaySMBWebhookEventAmount Amount { get; set; }
}

public class WorldpaySMBWebhookEventAmount
{
    public int Value { get; set; }
    public string CurrencyCode { get; set; }
}
