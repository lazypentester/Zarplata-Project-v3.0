using Microsoft.AspNetCore.SignalR;
using ServerExtensions;
using Server.Attributes.Authorization;

namespace Server.Hubs.HubFilters
{
    public class HubAuthorizeIPFilter : IHubFilter
    {
        public async ValueTask<object?> InvokeMethodAsync(HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object?>> next)
        {
            bool AuthorizeIPFilter = invocationContext.HubMethod.CustomAttributes.Any(attribute => attribute.AttributeType == typeof(HubAuthorizeIPAttribute));

            if (AuthorizeIPFilter)
            {
                var httpContext = invocationContext.Context.GetHttpContext();

                if (httpContext == null)
                {
                    throw new Exception("httpContext == null");
                }

                var requestIp = httpContext.GetRemoteIPAddress();

                if (requestIp == null)
                {
                    //remove after
                    File.Create(Path.Combine(Directory.GetCurrentDirectory(), "requestIp == null.error"));

                    throw new Exception("requestIp == null");
                }

                if (invocationContext.Context.User == null || invocationContext.Context.User.Identity == null || !invocationContext.Context.User.Identity.IsAuthenticated)
                {
                    //remove after
                    File.Create(Path.Combine(Directory.GetCurrentDirectory(), "Not Authenticated.error"));

                    throw new Exception("Not Authenticated");
                }

                bool match = invocationContext.Context.User.Claims.Any(claim => claim.Type == "ip" && claim.Value == requestIp.ToString());

                if (!match)
                {
                    //remove after
                    File.Create(Path.Combine(Directory.GetCurrentDirectory(), "requestIp does not match with tokenIp.error"));

                    throw new HubException("requestIp does not match with tokenIp");
                }
            }

            return await next(invocationContext);
        }
    }
}
