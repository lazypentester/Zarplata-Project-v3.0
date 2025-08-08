
using CommonModels.Client;
using CommonModels.ProjectTask.EarningSite.SocpublicCom;
using CommonModels.ProjectTask.EarningSite.SocpublicCom.TaskType.GSelectiveTaskWithAuth.SelectiveTask.WithdrawMoney;
using Microsoft.AspNetCore.SignalR.Client;
using SocpublicCom.Classes.Internal;
using SocpublicCom.Classes.Internal.Abstract;
using System;
using static CommonModels.ProjectTask.Platform.SocpublicCom.SocpublicComTaskEnums;
using static CommonModels.ProjectTask.ProjectTaskEnums;

namespace SocpublicCom.Classes.Public
{
    public class DoPlatformTask
    {
        private static EarningSiteSocpublicComTask? socpublicComBotTask = null;

        public async Task Do(EarningSiteWorkBotClient client, SocpublicComTask task, HubConnection serverHubConnection, HttpClient serverHttpConnection)
        {
            try
            {
                socpublicComBotTask = task.InternalType switch
                {
                    SocpublicComTaskType.GroupSelectiveTaskWithAuth => new SelectiveGroupTaskWithAuth(client, task, serverHubConnection, serverHttpConnection),
                    _ => null
                };
            }
            catch(Exception e)
            {
                throw new Exception($"class DoPlatformTask(), method Do(), error {e.Message}");
            }

            if (socpublicComBotTask == null)
            {
                throw new Exception($"class DoPlatformTask(), method Do(), error socpublicComBotTask == null");
            }

            try
            {
                await socpublicComBotTask.StartWork();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task Do(EarningSiteWorkBotClient client, SocpublicComAutoregTask task, HubConnection serverHubConnection, HttpClient serverHttpConnection)
        {
            try
            {
                socpublicComBotTask = new AutoregAccountTask(client, task, serverHubConnection, serverHttpConnection);
            }
            catch (Exception e)
            {
                throw new Exception($"class DoPlatformTask(), method Do(), error {e.Message}");
            }

            if (socpublicComBotTask == null)
            {
                throw new Exception($"class DoPlatformTask(), method Do(), error socpublicComBotTask == null");
            }

            try
            {
                await socpublicComBotTask.StartWork();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task Do(EarningSiteWorkBotClient client, WithdrawMoneyGroupSelectiveTaskWithAuth task, HubConnection serverHubConnection, HttpClient serverHttpConnection)
        {
            try
            {
                socpublicComBotTask = new WithdrawMoneyAccountTask(client, task, serverHubConnection, serverHttpConnection);
            }
            catch (Exception e)
            {
                throw new Exception($"class DoPlatformTask(), method Do(), error {e.Message}");
            }

            if (socpublicComBotTask == null)
            {
                throw new Exception($"class DoPlatformTask(), method Do(), error socpublicComBotTask == null");
            }

            try
            {
                await socpublicComBotTask.StartWork();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public static async Task TaskChangedStatus(TaskResultStatus resultStatus, Exception exception)
        {
            try
            {
                if (socpublicComBotTask != null)
                {
                    if (socpublicComBotTask.Task != null)
                    {
                        switch (socpublicComBotTask.Task.InternalType)
                        {
                            case SocpublicComTaskType.GroupSelectiveTaskWithoutAuth:
                                await (socpublicComBotTask as SelectiveGroupTaskWithAuth)!.TaskChangeResultStatus_SendServerReport(resultStatus, exception);
                                break;
                            case SocpublicComTaskType.GroupSelectiveTaskWithAuth:
                                await (socpublicComBotTask as SelectiveGroupTaskWithAuth)!.TaskChangeResultStatus_SendServerReport(resultStatus, exception);
                                break;
                        }
                    }
                    else if (socpublicComBotTask.TaskAutoreg != null)
                    {
                        await (socpublicComBotTask as AutoregAccountTask)!.TaskChangeResultStatus_SendServerReport(resultStatus, exception);
                    }
                    else if (socpublicComBotTask.TaskWithdrawMoney != null)
                    {
                        await (socpublicComBotTask as WithdrawMoneyAccountTask)!.TaskChangeResultStatus_SendServerReport(resultStatus, exception);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public static void DisposeTask()
        {
            try
            {
                if (socpublicComBotTask != null)
                {
                    if(socpublicComBotTask.Task != null)
                    {
                        switch (socpublicComBotTask!.Task!.InternalType)
                        {
                            case SocpublicComTaskType.GroupSelectiveTaskWithAuth:
                                (socpublicComBotTask as SelectiveGroupTaskWithAuth)!.Dispose();
                                break;
                        }
                    }
                    else if(socpublicComBotTask.TaskAutoreg != null)
                    {
                        switch (socpublicComBotTask!.TaskAutoreg!.InternalType)
                        {
                            case SocpublicComTaskType.AutoregTask:
                                (socpublicComBotTask as AutoregAccountTask)!.Dispose();
                                break;
                        }
                    }
                    else if(socpublicComBotTask.TaskWithdrawMoney != null)
                    {
                        (socpublicComBotTask as WithdrawMoneyAccountTask)!.Dispose();
                    }
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
