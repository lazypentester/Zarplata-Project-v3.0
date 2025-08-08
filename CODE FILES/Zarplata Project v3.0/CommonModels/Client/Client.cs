using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonModels.Client.Models;
using CommonModels.Client.Interfaces;
using System.Reflection;
using System.IO;

namespace CommonModels.Client
{
    public abstract class Client : ModelClient, IResourceManage
    {
        public Client()
        {
            MACHINE = new Machine();
        }

        public virtual bool WriteEnvironmentVariable(string variable, string value, EnvironmentVariableTarget target = EnvironmentVariableTarget.Process)
        {
            Environment.SetEnvironmentVariable(variable, value, target);

            return true;
        }
        public virtual string? ReadEnvironmentVariable(string variable, EnvironmentVariableTarget target = EnvironmentVariableTarget.Process)
        {
            string? value = null;

            value = Environment.GetEnvironmentVariable(variable, target);

            return value;
        }
        public abstract bool ReferenceCall(IEnumerable<string> args);

        public async Task<string> ReadResourceFileToString(string resourceFileName, Assembly assembly)
        {
            string result = "";
            string fullResourceFileName = "";
            
            string[] resourceNamesArray = assembly.GetManifestResourceNames();
            if(resourceNamesArray == null || resourceNamesArray.Length == 0)
            {
                throw new Exception($"resource file with name '{resourceFileName}' is not exists in current assembly");
            }
            foreach(string resourceName in resourceNamesArray)
            {
                if (resourceName.Contains(resourceFileName))
                {
                    fullResourceFileName = resourceName;
                    break;
                }
            }

            using (Stream? resourceStream = assembly.GetManifestResourceStream(fullResourceFileName))
            {
                if (resourceStream == null)
                {
                    throw new Exception($"resource file with name '{resourceFileName}' is not exists");
                }

                using (StreamReader reader = new StreamReader(resourceStream))
                {
                    result = await reader.ReadToEndAsync();
                }
            }

            return result;
        }
        public async Task<byte[]> ReadResourceFileToByteArray(string resourceFileName, Assembly assembly)
        {
            byte[] result;
            string fullResourceFileName = "";

            string[] resourceNamesArray = assembly.GetManifestResourceNames();
            if (resourceNamesArray == null || resourceNamesArray.Length == 0)
            {
                throw new Exception($"resource file with name '{resourceFileName}' is not exists in current assembly");
            }
            foreach (string resourceName in resourceNamesArray)
            {
                if (resourceName.Contains(resourceFileName))
                {
                    fullResourceFileName = resourceName;
                    break;
                }
            }

            using (Stream? resourceStream = assembly.GetManifestResourceStream(fullResourceFileName))
            {
                if (resourceStream == null)
                {
                    throw new Exception($"resource file with name '{resourceFileName}' is not exists");
                }

                await resourceStream.ReadAsync(result = new byte[resourceStream.Length], 0, (int)resourceStream.Length);
            }

            return result;
        }
        public async Task ExtractResourceFile(string resourceFileName, string extractFileName, Assembly assembly, string pathToExtract)
        {
            if (!Directory.Exists(pathToExtract))
            {
                throw new Exception($"directory with path '{pathToExtract}' is not exists");
            }

            string fullResourceFileName = "";

            string[] resourceNamesArray = assembly.GetManifestResourceNames();
            if (resourceNamesArray == null || resourceNamesArray.Length == 0)
            {
                throw new Exception($"resource file with name '{resourceFileName}' is not exists in current assembly");
            }
            foreach (string resourceName in resourceNamesArray)
            {
                if (resourceName.Contains(resourceFileName))
                {
                    fullResourceFileName = resourceName;
                    break;
                }
            }

            using (Stream? resourceStream = assembly.GetManifestResourceStream(fullResourceFileName))
            {
                if (resourceStream == null)
                {
                    throw new Exception($"resource file with name '{resourceFileName}' is not exists");
                }

                using (Stream extractStream = new FileStream(Path.Combine(pathToExtract, extractFileName), FileMode.Create))
                {
                    await resourceStream.CopyToAsync(extractStream);
                }
            }
        }

        public enum BotRole
        {
            EarningSiteBot = 0,
            ProxyCombineBot = 1,
            BotManager = 2
        }

        public enum ClientStatus
        {
            Started,
            Connected,
            AtWork,
            Free,
            Stopped,
            Disconnected
        }
    }
}
