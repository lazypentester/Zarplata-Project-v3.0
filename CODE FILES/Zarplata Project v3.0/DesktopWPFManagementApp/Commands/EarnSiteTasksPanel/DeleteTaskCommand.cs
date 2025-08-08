using CommonModels.Client.Models.DeleteBotsModels;
using CommonModels.Client.Models;
using DesktopWPFManagementApp.MVVM.ViewModel.CustomControls;
using DesktopWPFManagementApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DesktopWPFManagementApp.MVVM.ViewModel.CustomControls.BotsPanelUserControlViewModel;
using DesktopWPFManagementApp.MVVM.ViewModel.CustomControls.TasksPanelSubpanels;
using DesktopWPFManagementApp.Models;
using static DesktopWPFManagementApp.MVVM.ViewModel.CustomControls.TasksPanelSubpanels.EarnSiteTasksPanelUserControlViewModel;
using LoadDataStatusEnum = DesktopWPFManagementApp.MVVM.ViewModel.CustomControls.TasksPanelSubpanels.EarnSiteTasksPanelUserControlViewModel.LoadDataStatusEnum;
using CommonModels.ProjectTask.EarningSite.DeleteTasksModels;
using CommonModels.ProjectTask.EarningSite;

namespace DesktopWPFManagementApp.Commands.EarnSiteTasksPanel
{
    internal class DeleteTaskCommand : RelayCommand
    {
        // properties
        private EarnSiteTasksPanelUserControlViewModel earnSiteTasksPanelUserControlViewModel;

        // events
        public event Action<List<dynamic>?> DeleteTaskSuccessfully = (List<dynamic>? deletedTasks) => { };
        public event Action<string> DeleteTaskFailed = (string msg) => { };
        public event Action ConnectionError = () => { };

        public DeleteTaskCommand(EarnSiteTasksPanelUserControlViewModel earnSiteTasksPanelUserControlViewModel)
        {
            this.earnSiteTasksPanelUserControlViewModel = earnSiteTasksPanelUserControlViewModel;
        }

        public override bool CanExecute(object? parameter)
        {
            return earnSiteTasksPanelUserControlViewModel.LoadingDataStatus == LoadDataStatusEnum.DataIsLoaded.ToString() && earnSiteTasksPanelUserControlViewModel.DeleteTaskActionCanExecute && base.CanExecute(parameter);
        }

        public override async void Execute(object? parameter)
        {
            // hide Delete popup Panel
            earnSiteTasksPanelUserControlViewModel.IsDeleteTaskPopupPanelOpen = false;

            // set load view visible
            earnSiteTasksPanelUserControlViewModel.LoadingDataStatus = LoadDataStatusEnum.DataIsLoading.ToString();

            // logic
            CancellationTokenSource cancellationTokenSourceDeleteTask = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            List<dynamic>? modelTasks = null;
            DeleteTasks? deleteTaskModel = null;
            DeletedTasks? deletedTaskModel = null;
            bool canTryToDeleteTasks = false;
            string deleteTasksFailedText = "Произошла непредвиденная ошибка.";

            // try to create DeleteTasks parameter
            try
            {
                if (earnSiteTasksPanelUserControlViewModel.DeleteTasksOneOrFewInfo == DeleteTasksOneOrFew.One && earnSiteTasksPanelUserControlViewModel.CurrentSelectedTask != null)
                {
                    modelTasks = new List<dynamic>();
                    modelTasks.Add(earnSiteTasksPanelUserControlViewModel.CurrentSelectedTask);

                    deleteTaskModel = new DeleteTasks(modelTasks);
                }
                else if (earnSiteTasksPanelUserControlViewModel.DeleteTasksOneOrFewInfo == DeleteTasksOneOrFew.Few)
                {
                    modelTasks = new List<dynamic>();
                    foreach (var viewTask in earnSiteTasksPanelUserControlViewModel.TasksCollection.Where(task => task.IsMarked))
                    {
                        modelTasks.Add(viewTask);
                    }

                    deleteTaskModel = new DeleteTasks(modelTasks);
                }
                else
                {
                    throw new Exception();
                }

                canTryToDeleteTasks = true;
            }
            catch
            {
                canTryToDeleteTasks = false;
                deleteTasksFailedText = "Не удалось получить данные для удаления заданий.";
            }

            // try to delete tasks
            if (canTryToDeleteTasks)
            {
                try
                {
                    deletedTaskModel = await ServiceStorage.EarnTaskService.DeleteTasks(deleteTaskModel!, cancellationTokenSourceDeleteTask.Token);

                    if (deletedTaskModel != null && deletedTaskModel.DeletedTasksStatus)
                    {
                        // update bots collection in viewModel
                        //try
                        //{

                        //}
                        //catch { }

                        // set load view invisible
                        earnSiteTasksPanelUserControlViewModel.LoadingDataStatus = LoadDataStatusEnum.DataIsLoaded.ToString();
                        DeleteTaskSuccessfully?.Invoke(modelTasks);
                    }
                    else if (deletedTaskModel != null && !deletedTaskModel.DeletedTasksStatus)
                    {
                        earnSiteTasksPanelUserControlViewModel.LoadingDataStatus = LoadDataStatusEnum.DataErrorLoad.ToString();
                        DeleteTaskFailed?.Invoke(deletedTaskModel.DeletedTasksStatusText);
                    }
                    else
                    {
                        earnSiteTasksPanelUserControlViewModel.LoadingDataStatus = LoadDataStatusEnum.DataErrorLoad.ToString();
                        deleteTasksFailedText = "Произошла непредвиденная ошибка.";
                        DeleteTaskFailed?.Invoke(deleteTasksFailedText);
                    }
                }
                catch
                {
                    earnSiteTasksPanelUserControlViewModel.LoadingDataStatus = LoadDataStatusEnum.DataErrorLoad.ToString();
                    ConnectionError?.Invoke();
                }
            }
            else
            {
                earnSiteTasksPanelUserControlViewModel.LoadingDataStatus = LoadDataStatusEnum.DataErrorLoad.ToString();
                DeleteTaskFailed?.Invoke(deleteTasksFailedText);
            }
        }
    }
}
