using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using OpenIddict.Client;

var services = new ServiceCollection();

services.AddOpenIddict()

    // Register the OpenIddict client components.
    .AddClient(options =>
    {
        // Allow grant_type=client_credentials to be negotiated.
        options.AllowClientCredentialsFlow();
        
        options.AllowCustomFlow("custom_grant");

        // Disable token storage, which is not necessary for non-interactive flows like
        // grant_type=password, grant_type=client_credentials or grant_type=refresh_token.
        options.DisableTokenStorage();

        // Register the System.Net.Http integration and use the identity of the current
        // assembly as a more specific user agent, which can be useful when dealing with
        // providers that use the user agent as a way to throttle requests (e.g Reddit).
        options.UseSystemNetHttp()
               .SetProductInformation(typeof(Program).Assembly);

        // Add a client registration matching the client application definition in the server project.
        options.AddRegistration(new OpenIddictClientRegistration
        {
            Issuer = new Uri("https://localhost:44385/", UriKind.Absolute),

            ClientId = "console",
            ClientSecret = "388D45FA-B36B-4988-BA59-B187D329C207"
        });
    });

await using var provider = services.BuildServiceProvider();

Console.WriteLine("OpenIddict Client Test");

// var token = await GetTokenAsync(provider);
// Console.WriteLine("Access token: {0}", token);
// Console.WriteLine();
//
// var resource = await GetResourceAsync(provider, token);
// Console.WriteLine("API response: {0}", resource);

var customToken = await GetTokenByCustomGrantAsync(provider);
Console.WriteLine("Custom access token: {0}", customToken);

var resource2 = await GetResourceAsync(provider, customToken);
Console.WriteLine("API response returned by custom token: {0}", resource2);

Console.ReadLine();
//
// static async Task<string> GetTokenAsync(IServiceProvider provider)
// {
//     var service = provider.GetRequiredService<OpenIddictClientService>();
//
//     var result = await service.AuthenticateWithClientCredentialsAsync(new());
//     return result.AccessToken;
// }

static async Task<string> GetTokenByCustomGrantAsync(IServiceProvider provider)
{
    var service = provider.GetRequiredService<OpenIddictClientService>();
    
    var result = await service.AuthenticateWithCustomGrantAsync(new OpenIddictClientModels.CustomGrantAuthenticationRequest
    {
        GrantType = "custom_grant",
        AdditionalTokenRequestParameters = new Dictionary<string, OpenIddictParameter>
        {
            ["custom_parameter"] = "custom_value",
        },
    });

    return result.AccessToken;
}

static async Task<string> GetResourceAsync(IServiceProvider provider, string token)
{
    using var client = provider.GetRequiredService<HttpClient>();
    using var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:44385/api/message");
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    using var response = await client.SendAsync(request);
    response.EnsureSuccessStatusCode();

    return await response.Content.ReadAsStringAsync();
}
