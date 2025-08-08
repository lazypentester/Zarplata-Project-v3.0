using System.Diagnostics;

namespace BotManagerBotConsoleClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start new bot..");

            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.UseShellExecute = true;
            processStartInfo.FileName = @"C:\WINDOWS\system32\mspaint.exe";
            //processStartInfo.FileName = @"Paint.exe";

            Process.Start(processStartInfo);
            Process.Start(processStartInfo);
            Process.Start(processStartInfo);

            Thread.Sleep(Timeout.Infinite);
        }
    }
}