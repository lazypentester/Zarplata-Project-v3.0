using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.Client.Interfaces
{
    internal interface IFileManage
    {
        public bool WriteFile(string fileName, string value, string path = "/");
        public string ReadFile(string fileName, string path = "/");
    }
}
