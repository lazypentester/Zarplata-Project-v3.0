using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyCombiner.Classes.Public
{
    public static class SaveMessage
    {
        public static string CurrentDirectory = Directory.GetCurrentDirectory();
        public static async Task Save(string bot_id, string msg)
        {
            //Console.WriteLine(msg);
            await File.WriteAllTextAsync(Path.Combine(CurrentDirectory, $"{bot_id}_bot_log_file.txt"), msg);
        }
    }
}
