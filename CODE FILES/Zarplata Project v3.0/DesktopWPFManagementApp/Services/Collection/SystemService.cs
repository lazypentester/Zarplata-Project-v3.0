using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopWPFManagementApp.Services.Collection
{
    public class SystemService
    {
        public SystemService()
        {
        }

        public string CreateIdentityKey()
        {
            string IDENTITY_KEY = "";

            var data = CollectIdentityKeyData();

            using (var md5 = MD5.Create())
            {
                StringBuilder? builder = null;
                byte[]? hashInBytes = null;
                string? hashInString = null;

                try
                {
                    builder = new StringBuilder();

                    foreach (KeyValuePair<string, string?> part in data)
                    {
                        builder.Append(part.Value);
                    }

                    hashInBytes = md5.ComputeHash(Encoding.ASCII.GetBytes(builder.ToString()));
                    hashInString = Convert.ToHexString(hashInBytes);

                    IDENTITY_KEY = hashInString;
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
                finally
                {
                    data = null;
                    builder = null;
                    hashInBytes = null;
                    hashInString = null;
                }
            }

            return IDENTITY_KEY;
        }
        private List<KeyValuePair<string, string?>> CollectIdentityKeyData()
        {
            List<KeyValuePair<string, string?>> result = new List<KeyValuePair<string, string?>>();

            result.Add(new KeyValuePair<string, string?>("MASHINE_NAME", System.Environment.MachineName));
            result.Add(new KeyValuePair<string, string?>("IS_64_BIT_OPERATING_SYSTEM", System.Environment.Is64BitOperatingSystem.ToString()));
            result.Add(new KeyValuePair<string, string?>("OS_VERSION_Platform", System.Environment.OSVersion.Platform.ToString()));
            result.Add(new KeyValuePair<string, string?>("OS_VERSION_VersionString", System.Environment.OSVersion.VersionString));
            result.Add(new KeyValuePair<string, string?>("OS_VERSION_ServicePack", System.Environment.OSVersion.ServicePack));
            result.Add(new KeyValuePair<string, string?>("OS_VERSION_Version_Build", System.Environment.OSVersion.Version.Build.ToString()));
            result.Add(new KeyValuePair<string, string?>("OS_VERSION_Version_Major", System.Environment.OSVersion.Version.Major.ToString()));
            result.Add(new KeyValuePair<string, string?>("OS_VERSION_Minor", System.Environment.OSVersion.Version.Minor.ToString()));
            result.Add(new KeyValuePair<string, string?>("OS_VERSION_MinorRevision", System.Environment.OSVersion.Version.MinorRevision.ToString()));
            result.Add(new KeyValuePair<string, string?>("OS_VERSION_Revision", System.Environment.OSVersion.Version.Revision.ToString()));
            result.Add(new KeyValuePair<string, string?>("PROCESSOR_COUNT", System.Environment.ProcessorCount.ToString()));
            result.Add(new KeyValuePair<string, string?>("USER_NAME", System.Environment.UserName));
            result.Add(new KeyValuePair<string, string?>("USER_DOMAIN_NAME", System.Environment.UserDomainName));

            result.Add(new KeyValuePair<string, string?>("OSDescription", RuntimeInformation.OSDescription));
            result.Add(new KeyValuePair<string, string?>("OSArchitecture", RuntimeInformation.OSArchitecture.ToString()));
            result.Add(new KeyValuePair<string, string?>("ProcessArchitecture", RuntimeInformation.ProcessArchitecture.ToString()));

            //result.Add(new KeyValuePair<string, string?>("SystemPageSize", Environment.SystemPageSize.ToString()));

            // get network interfaces
            var adapters = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            foreach (var adapter in adapters)
            {
                if (adapter.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Ethernet ||
                    adapter.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Ethernet3Megabit ||
                    adapter.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.FastEthernetFx ||
                    adapter.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.FastEthernetT ||
                    adapter.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.GigabitEthernet
                    )
                {
                    string adapterInfo = GetAdapterInfo(adapter);

                    result.Add(new KeyValuePair<string, string?>("NETWORK_INTERFACE_ETHERNET", adapterInfo));
                }
                else if (adapter.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Wireless80211)
                {
                    string adapterInfo = GetAdapterInfo(adapter);

                    result.Add(new KeyValuePair<string, string?>("NETWORK_INTERFACE_WIRELESS80211", adapterInfo));
                }
            }

            return result;
        }
        private string GetAdapterInfo(System.Net.NetworkInformation.NetworkInterface adapter)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(adapter.Id);
            stringBuilder.Append(adapter.Name);
            stringBuilder.Append(adapter.GetPhysicalAddress().ToString());

            return stringBuilder.ToString();
        }

        public bool WriteEnvironmentVariable(string variable, string value, EnvironmentVariableTarget target = EnvironmentVariableTarget.Process)
        {
            Environment.SetEnvironmentVariable(variable, value, target);

            return true;
        }
        public string? ReadEnvironmentVariable(string variable, EnvironmentVariableTarget target = EnvironmentVariableTarget.Process)
        {
            string? value = null;

            value = Environment.GetEnvironmentVariable(variable, target);

            return value;
        }
    }
}
