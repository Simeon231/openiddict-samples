using System.Text.Json.Nodes;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace Aridka.Server.Handlers
{
    public class CustomGrantHandler : IOpenIddictServerHandler<HandleTokenRequestContext>
    {
        public ValueTask HandleAsync(HandleTokenRequestContext context)
        {
            if (context.Request.GrantType != "custom_grant")
            {
                return ValueTask.CompletedTask;
            }

            var parameter = context.Request["custom_parameter"];
            if (parameter is null || parameter.Value != "custom_value")
            {
                context.Reject(
                    error: OpenIddictConstants.Errors.InvalidGrant,
                    description: "Invalid custom_parameter.");
            }

            context.Request.Claims = new JsonObject { { "handler_claim", "handler_value" } };

            return ValueTask.CompletedTask;
        }
    }
}