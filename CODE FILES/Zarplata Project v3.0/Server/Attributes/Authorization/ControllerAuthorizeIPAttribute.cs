using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ServerExtensions;

namespace Server.Attributes.Authorization
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ControllerAuthorizeIPAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var requestIp = context.HttpContext.GetRemoteIPAddress();

            if (requestIp == null)
            {
                context.Result = new JsonResult(new { message = "Unauthorized", error = "requestIp == null" })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }

            bool match = context.HttpContext.User.Claims.Any(claim => claim.Type == "ip" && claim.Value == requestIp!.ToString());

            if (!match)
            {
                context.Result = new JsonResult(new { message = "Unauthorized", error = "requestIp does not match with tokenIp" })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
        }
    }
}
