using System.Net.Http;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace ServerExtensions
{
    public static class HttpContextExtensions
    {
        //public static string BrokenHeaders = "";
        public static IPAddress? GetRemoteIPAddress(this HttpContext context, bool allowForwarded = true)
        {
            if (allowForwarded)
            {
                string? header = (context.Request.Headers["Remote_Addr"].FirstOrDefault() ??
                    context.Request.Headers["X-Forwarded-For"].FirstOrDefault() ??
                    context.Request.Headers["CF-Connecting-IP"].FirstOrDefault() ??
                    context.Request.Headers["CF-Connecting-IPv6"].FirstOrDefault());

                //if (header == null)
                //{
                //    foreach(var h in context.Request.Headers)
                //    {
                //        BrokenHeaders += $"{h.Key}:{h.Value}";
                //    }
                //    return null;
                //}

                if (header != null)
                {
                    if (IPAddress.TryParse(header, out IPAddress? ip))
                    {
                        return ip;
                    }
                }

                //if (IPAddress.TryParse(header, out IPAddress? ip))
                //{
                //    return ip;
                //}
            }

            return context.Connection.RemoteIpAddress;
        }
    }
}