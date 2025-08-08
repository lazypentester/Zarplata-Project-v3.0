using ClientInitialization.Classes.Internal;
using ClientOperationMessages;
using CommonModels.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static ClientInitialization.Classes.Public.InitDelegates;
using static ClientInitialization.Classes.Public.InitEnums;
using static ClientOperationMessages.InitMessages;
using Microsoft.AspNetCore.SignalR.Client;

namespace ClientInitialization.Classes.Public
{
    public class Initialization
    {
        HubConnection? serverHubConnection = null;
        HttpClient? serverhttpConnection = null;

        public HubConnection? get_SERVER_HUB_CONNECTION() => serverHubConnection;
        public HttpClient? get_SERVER_HTTP_CONNECTION() => serverhttpConnection;
        public async Task<bool> Startinitialization(IEnumerable<string> appParams, Client client, PrintMessage printMessageMethods, Platform platform)
        {
            printMessageMethods.Invoke($"{OPERATION_START}Определение операционной системы");

            bool InitResult = false;

            if (platform == Platform.Linux)
            {
                printMessageMethods.Invoke($"{OPERATION_SUCCESSFUL} (Linux)");

                using (Init linuxInit = new LinuxInit(appParams, client, printMessageMethods))
                {
                    await linuxInit.StartInit().ContinueWith(init =>
                    {
                        printMessageMethods.Invoke($"\n{OPERATION_START}Инициализация клиента");

                        if (init.IsCompletedSuccessfully)
                        {
                            printMessageMethods.Invoke($"{OPERATION_SUCCESSFUL}");

                            InitResult = true;

                            serverHubConnection = linuxInit.get_SERVER_HUB_CONNECTION();
                            serverhttpConnection = linuxInit.get_SERVER_HTTP_CONNECTION();
                        }
                        else
                        {
                            printMessageMethods.Invoke($"{OPERATION_FAILED}");
                            printMessageMethods.Invoke($"\n{init.Exception?.Message}");

                            InitResult = false;
                        }
                    });
                }
            }
            else if (platform == Platform.Windows)
            {
                printMessageMethods.Invoke($"{OPERATION_SUCCESSFUL} (Windows)");

                using (Init linuxInit = new WindowsInit(appParams, client, printMessageMethods))
                {
                    await linuxInit.StartInit().ContinueWith(init =>
                    {
                        printMessageMethods.Invoke($"\n{OPERATION_START}Инициализация клиента");

                        if (init.IsCompletedSuccessfully)
                        {
                            printMessageMethods.Invoke($"{OPERATION_SUCCESSFUL}");

                            InitResult = true;

                            serverHubConnection = linuxInit.get_SERVER_HUB_CONNECTION();
                            serverhttpConnection = linuxInit.get_SERVER_HTTP_CONNECTION();
                        }
                        else
                        {
                            printMessageMethods.Invoke($"{OPERATION_FAILED}");
                            printMessageMethods.Invoke($"\n{init.Exception?.Message}");

                            InitResult = false;
                        }
                    });
                }
            }
            else if (platform == Platform.Other)
            {
                InitResult = false;
            }
            else
            {
                InitResult = false;
            }

            return InitResult;
        }
    }
}
