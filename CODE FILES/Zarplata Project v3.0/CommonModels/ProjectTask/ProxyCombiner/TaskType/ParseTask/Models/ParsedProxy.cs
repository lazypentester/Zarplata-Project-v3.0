using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.ProjectTask.ProxyCombiner.TaskType.ParseTask.Models
{
    public class ParsedProxy
    {
        public string Ip { get; set; }
        public string Port { get; set; }
        public string FullProxyAddress
        {
            get
            {
                return $"{Ip}:{Port}";
            }
        }

        public ParsedProxy(string ip, string port)
        {
            Ip = ip;
            Port = port;
        }
    }
}
