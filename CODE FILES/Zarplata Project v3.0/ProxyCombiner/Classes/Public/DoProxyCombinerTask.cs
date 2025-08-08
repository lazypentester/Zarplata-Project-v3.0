using CommonModels.Client;
using Microsoft.AspNetCore.SignalR.Client;
using ProxyCombiner.Classes.Internal;
using System.Net;
using static CommonModels.ProjectTask.Platform.SocpublicCom.SocpublicComTaskEnums;
using static CommonModels.ProjectTask.ProjectTaskEnums;
using CommonProxyCombinerTask = CommonModels.ProjectTask.ProxyCombiner.ProxyCombinerTask;

namespace ProxyCombiner.Classes.Public
{
    public class DoProxyCombinerTask
    {
        private SelectiveProxyCombinerTask? proxyCombinerTask = null;

        public async Task Do(CommonProxyCombinerTask task, HubConnection serverHubConnection, HttpClient proxyParseHttpClient, CookieContainer proxyParseCookieContainer, Client client)
        {
            // create task
            try
            {
                proxyCombinerTask = new SelectiveProxyCombinerTask(task, serverHubConnection, proxyParseHttpClient, proxyParseCookieContainer, client);
            }
            catch (Exception e)
            {
                //removeafterthis!!!--------
                await SaveMessage.Save(client!.ID!, $"class DoProxyCombinerTask(), method Do(), error {e.Message}");
                Console.WriteLine($"class DoProxyCombinerTask(), method Do(), error {e.Message}");

                throw new Exception($"class DoProxyCombinerTask(), method Do(), error {e.Message}");
            }

            if (proxyCombinerTask == null)
            {
                //removeafterthis!!!--------
                await SaveMessage.Save(client!.ID!, $"class DoProxyCombinerTask(), method Do(), error proxyCombinerTask == null");
                Console.WriteLine($"class DoProxyCombinerTask(), method Do(), error proxyCombinerTask == null");

                throw new Exception($"class DoProxyCombinerTask(), method Do(), error proxyCombinerTask == null");
            }

            // start created task
            try
            {
                await proxyCombinerTask.StartWork();
            }
            catch (Exception e)
            {
                //removeafterthis!!!--------
                await SaveMessage.Save(client!.ID!, $"await proxyCombinerTask.StartWork();, error {e.Message}");
                Console.WriteLine($"await proxyCombinerTask.StartWork();, error {e.Message}");

                throw new Exception(e.Message);
            }
        }

        public async Task TaskChangedStatus(TaskResultStatus resultStatus, Exception? exception = null)
        {
            try
            {
                if (proxyCombinerTask != null)
                {
                    await proxyCombinerTask.TaskChangeResultStatus_SendServerReport(resultStatus, exception);
                }
            }
            catch (Exception e)
            {
                //removeafterthis!!!--------
                await SaveMessage.Save("TaskChangedStatus", $"TaskChangedStatus;, error {e.Message}");
                Console.WriteLine($"TaskChangedStatus;, error {e.Message}");

                throw new Exception(e.Message);
            }
        }

        public void DisposeTask()
        {
            try
            {
                if (proxyCombinerTask != null)
                {
                    (proxyCombinerTask as SelectiveProxyCombinerTask)!.Dispose();
                }
            }
            catch (Exception e)
            {
                //removeafterthis!!!--------
                SaveMessage.Save("DisposeTask", $"DisposeTask, error {e.Message}");
                Console.WriteLine($"DisposeTask, error {e.Message}");
            }
        }
    }
}
