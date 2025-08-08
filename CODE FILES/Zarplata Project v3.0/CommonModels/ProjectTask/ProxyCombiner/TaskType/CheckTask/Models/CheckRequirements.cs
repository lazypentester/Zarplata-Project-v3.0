using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonModels.ProjectTask.ProxyCombiner.TaskType.CheckTask.Models.CheckedProxy;
using static CommonModels.ProjectTask.ProxyCombiner.TaskType.CheckTask.Models.CheckRequirements;
using static CommonModels.ProjectTask.ProxyCombiner.TaskType.ParseTask.Models.ParsedProxy;

namespace CommonModels.ProjectTask.ProxyCombiner.TaskType.CheckTask.Models
{
    public class CheckRequirements
    {
        public string requestURL { get; set; }
        public string requestURL_SSL { get; set; }
        public CPProtocol[] requiredProxyProtocol { get; set; }
        public SupportsSecureConnectionState[] requiredSupportsSecureConnectionState { get; set; }
        public AnonymousState[] requiredAnonymityState { get; set; }
        public int requestTimeoutMs { get; set; }
        public CPWorkState[] requiredWorkState { get; set; }
        public string requestImapHost { get; set; }
        public int requestImapPort { get; set; }


        public CheckRequirements(string requestURL, string requestURL_SSL, CPProtocol[] requiredProxyProtocol, SupportsSecureConnectionState[] requiredSupportsSecureConnectionState, AnonymousState[] requiredAnonymityState, int requestTimeoutMs, CPWorkState[] requiredWorkState, string requestImapHost = "", int requestImapPort = 0)
        {
            this.requestURL = requestURL;
            this.requestURL_SSL = requestURL_SSL;
            this.requiredProxyProtocol = requiredProxyProtocol;
            this.requiredSupportsSecureConnectionState = requiredSupportsSecureConnectionState;
            this.requiredAnonymityState = requiredAnonymityState;
            this.requestTimeoutMs = requestTimeoutMs;
            this.requiredWorkState = requiredWorkState;
            this.requestImapHost = requestImapHost;
            this.requestImapPort = requestImapPort;
        }
    }
}
