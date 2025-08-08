
using CommonModels.Client.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.Client
{
    public class BotManagerClient : Client, IFileManage, IDirectoryManage
    {
        public BotManagerClient()
        {
            base.Role = BotRole.BotManager;
        }

        public override bool ReferenceCall(IEnumerable<string> args)
        {
            // вызов справки
            if (args.Contains("--help") || args.Contains("-h"))
            {
                Console.Clear();
                Console.WriteLine("   -h   or  --help             - вызов справки");
                Console.WriteLine("   -sh  or  --server-host      - запись хоста сервера");
                Console.WriteLine("   -r   or  --register         - регистрация машины");
                Console.WriteLine("   -rm  or  --remove           - удаление машины");
                Console.WriteLine("   -rp  or  --repair           - восстановление машины");
                Console.WriteLine("   -sai or  --StartAfterInit   - принудительное получение нового задания ");

                return true;
            }

            return false;
        }

        public bool CreateDirectory(string directoryName, string path = "/")
        {
            Directory.CreateDirectory(Path.Combine(path, directoryName));

            return true;
        }

        public bool DeleteDirectory(string directoryName, string path = "/")
        {
            Directory.Delete(Path.Combine(path, directoryName), true);

            return true;
        }

        public string ReadFile(string fileName, string path = "/")
        {
            return File.ReadAllText(Path.Combine(path, fileName), Encoding.UTF8);
        }

        public bool WriteFile(string fileName, string value, string path = "/")
        {
            File.WriteAllText(Path.Combine(path, fileName), value, Encoding.UTF8);

            return true;
        }
    }
}
