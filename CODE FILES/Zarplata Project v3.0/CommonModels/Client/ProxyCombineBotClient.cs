

namespace CommonModels.Client
{
    public class ProxyCombineBotClient : Client
    {
        public ProxyCombineBotClient()
        {
            base.Role = BotRole.ProxyCombineBot;
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
