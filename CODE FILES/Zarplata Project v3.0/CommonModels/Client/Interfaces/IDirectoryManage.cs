using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.Client.Interfaces
{
    internal interface IDirectoryManage
    {
        public bool CreateDirectory(string directoryName, string path = "/");
        public bool DeleteDirectory(string directoryName, string path = "/");
    }
}
