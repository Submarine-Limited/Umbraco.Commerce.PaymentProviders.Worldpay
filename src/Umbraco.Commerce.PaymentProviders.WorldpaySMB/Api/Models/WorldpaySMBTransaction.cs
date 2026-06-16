using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Umbraco.Commerce.PaymentProviders.WorldpaySMB.Api.Models;

public record WorldpaySMBTransaction
{
    [JsonPropertyName("_embedded")]
    public  WorldpaySMBTransactionEmbeddedData EmbeddedData { get; set; }
}

public record WorldpaySMBTransactionEmbeddedData
{
    public List<WorldpaySMBTransactionPayment> Payments { get; set; }
}

public record WorldpaySMBTransactionPayment
{
    public string PaymentId { get; set; }
}
