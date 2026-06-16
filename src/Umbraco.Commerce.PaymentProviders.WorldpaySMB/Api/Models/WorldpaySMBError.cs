namespace Umbraco.Commerce.PaymentProviders.WorldpaySMB.Api.Models;

public record WorldpaySMBError
{

    public string Name { get; set; }
    public string Message { get; set; }
}
