namespace Umbraco.Commerce.PaymentProviders.WorldpaySMB.Api.Models;

public record WorldpaySMBTransactionData
{
    public required string TransactionReference { get; set; }
    public required WorldpaySMBTransactionMerchant Merchant { get; set; }
    public required WorldpaySMBTransactionValue Value { get; set; }
    public required WorldpaySMBTransactionNarrative Narrative { get; set; }
    public required string Description { get; set; }
    public required WorldpaySMBTransactionBillingAddress BillingAddress { get; set; }
    public required WorldpaySMBTransactionResultURLs ResultURLs { get; set; }
    public required WorldpaySMBTransactionRiskData RiskData { get; set; }

    public string ThreeDS { get; set; }
}

public record WorldpaySMBTransactionMerchant
{
    public required string Entity { get; set; }
}

public record WorldpaySMBTransactionValue
{
    public required string Currency { get; set; }
    public required int Amount { get; set; }
}

public record WorldpaySMBTransactionNarrative
{
    public required string Line1 { get; set; }
}

public record WorldpaySMBTransactionBillingAddress
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Address1 { get; set; }
    public required string Address2 { get; set; }
    public required string City { get; set; }
    public required string CountryCode { get; set; }
    public required string PostalCode { get; set; }
}

public record WorldpaySMBTransactionResultURLs
{
    public required string SuccessURL { get; set; }
    public required string PendingURL { get; set; }
    public required string FailureURL { get; set; }
    public required string ErrorURL { get; set; }
    public required string CancelURL { get; set; }
    public required string ExpiryURL { get; set; }
}

public record WorldpaySMBTransactionRiskData
{
    public required WorldpaySMBTransactionAccount Account { get; set; }
}

public record WorldpaySMBTransactionAccount
{
    public required string Email { get; set; }
}
