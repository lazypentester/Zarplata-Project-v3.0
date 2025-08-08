using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.Client
{
    public class EarningSiteWorkBotClient : Client
    {
        public EarningSiteWorkBotClient()
        {
            base.Role = BotRole.EarningSiteBot;
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
    }
}
