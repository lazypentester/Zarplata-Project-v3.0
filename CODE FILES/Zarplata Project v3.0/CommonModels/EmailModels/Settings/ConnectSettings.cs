using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.Email.Settings
{
    public class ConnectSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public bool UseSsl { get; set; }

        public ConnectSettings(string host, int port, bool useSsl)
        {
            Host = host;
            Port = port;
            UseSsl = useSsl;
        }
    }
}
