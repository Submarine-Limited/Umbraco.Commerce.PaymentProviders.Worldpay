namespace Umbraco.Commerce.PaymentProviders.WorldpaySMB.Api.Models;

public class WorldpaySMBClientConfig
{
    public string BaseUrl { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }

    public WorldpaySMBClientConfig(string username, string password, string baseUrl)
    {
        Username = username;
        Password = password;
        BaseUrl = baseUrl;
    }
}
