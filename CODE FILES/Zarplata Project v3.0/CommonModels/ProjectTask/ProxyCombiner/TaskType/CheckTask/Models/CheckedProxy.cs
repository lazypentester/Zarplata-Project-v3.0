using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.ParseTask.Models;
using static CommonModels.ProjectTask.ProxyCombiner.Models.EnvironmentProxy;

namespace CommonModels.ProjectTask.ProxyCombiner.TaskType.CheckTask.Models
{
    public class CheckedProxy
    {
        public ParsedProxy proxy { get; set; }
        public string? requestURL { get; set; } = null;
        public CPProtocol? protocol { get; set; } = null;
        public SupportsSecureConnectionState? supportsSecureConnection { get; set; } = null; 
        public string? country { get; set; } = null;
        public string? city { get; set; } = null;
        public int? speed { get; set; } = null;
        public AnonymousState? anonymity { get; set; } = null;
        public CPWorkState? work { get; set; } = null;
        public int? requestStatusCode { get; set; } = null;
        public string? requestStatusCodeName { get; set; } = null;
        public ProxyExeption? errorType { get; set; } = null;
        public string? FullProxyAddress
        {
            get
            {
                if(protocol == null)
                {
                    return null;
                }

                return $"{protocol}://{proxy.FullProxyAddress}";
            }
        }
        public Uri? FullProxyAddressUri
        {
            get
            {
                if (protocol == null)
                {
                    return null;
                }

                return new Uri($"{protocol}://{proxy.FullProxyAddress}");
            }
        }

        public CheckedProxy(ParsedProxy proxy)
        {
            this.proxy = proxy;
        }

        public enum CPWorkState
        {
            Work,
            NotWork,
            Error
        }

        public enum CPProtocol
        {
            http,
            socks4,
            socks5,
            imap
        }

        public enum SupportsSecureConnectionState
        {
            Undefined,
            Support,
            NotSupport
        }

        public enum AnonymousState
        {
            Undefined,
            Transparent,
            Anonymous,
            Error
        }

        public class ProxyExeption
        {
            public ExeptionState exeption { get; set; }
            public string message { get; set; }

            public ProxyExeption(ExeptionState exeption, string message)
            {
                this.exeption = exeption;
                this.message = message;
            }
        }

        public enum ExeptionState
        {
            System_Net_Http_HttpRequestException,
            System_Threading_Tasks_TaskCanceledException,
            Other
        }
    }
}
