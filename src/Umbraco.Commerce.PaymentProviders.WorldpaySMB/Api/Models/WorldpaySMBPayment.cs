using System;
using System.Collections.Generic;
using System.Text;

namespace Umbraco.Commerce.PaymentProviders.WorldpaySMB.Api.Models;

public record WorldpaySMBPayment
{
    public string PaymentId { get; set; }
    public string TransactionReference { get; set; }
    public List<WorldpaySMBPaymentEvent> Events { get; set; }
    public WorldpaySMBTransactionValue Value { get; set; }
}

public record WorldpaySMBPaymentEvent
{
    public string EventName { get; set; }
    public DateTime Timestamp { get; set; }
    public string Description { get; set; }
    public string Code { get; set; }
    public WorldpaySMBError Error { get; set; }
    public string Outcome { get; set; }
    public string CommandId { get; set; }
}
