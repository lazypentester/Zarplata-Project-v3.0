using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.Client.Interfaces
{
    internal interface IResourceManage
    {
        public Task<string> ReadResourceFileToString(string resourceFileName, Assembly assembly);
        public Task<byte[]> ReadResourceFileToByteArray(string resourceFileName, Assembly assembly);
        public Task ExtractResourceFile(string resourceFileName, string extractFileName, Assembly assembly, string pathToExtract = ".");
    }
}
