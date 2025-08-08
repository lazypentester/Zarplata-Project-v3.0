using ClientInitialization.Classes.Public;
using ClientInitialization.Interfaces.Public;
using CommonModels.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static ClientInitialization.Classes.Public.InitDelegates;
using static ClientInitialization.Classes.Public.InitEnums;

namespace ProxyCombineBotConsoleClient
{
    internal class Init : IInitializable, IMessagePrintable
    {
        public async Task Initialize(string[] args, Client client, PrintMessage printMessageMethods)
        {
            Platform platform;

            if (Environment.OSVersion.Platform == PlatformID.Unix && RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                platform = Platform.Linux;
            }
            else if (Environment.OSVersion.Platform == PlatformID.Win32NT && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                platform = Platform.Windows;
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                platform = Platform.MacOSX;
            }
            else if (Environment.OSVersion.Platform == PlatformID.Other)
            {
                platform = Platform.Other;
            }
            else
            {
                platform = Platform.Other;
            }

            Initialization initialization = new Initialization();
            bool InitializeResult = await initialization.Startinitialization(args, client, printMessageMethods, platform);

            if (!InitializeResult)
            {
                Environment.Exit(0);
            }
            else
            {
                ServerConnections.serverHubConnection = initialization.get_SERVER_HUB_CONNECTION();
                ServerConnections.serverhttpConnection = initialization.get_SERVER_HTTP_CONNECTION();
            }
        }

        public void PrintClientMessage(string message)
        {
            Console.Write(message);
        }
    }
}
