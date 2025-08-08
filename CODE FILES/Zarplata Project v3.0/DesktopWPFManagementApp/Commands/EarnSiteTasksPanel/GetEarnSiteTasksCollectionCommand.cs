using CommonModels.Client.Models.SearchBotsModels;
using CommonModels.ProjectTask.EarningSite.SearchTasksModels;
using DesktopWPFManagementApp.MVVM.ViewModel.CustomControls;
using DesktopWPFManagementApp.MVVM.ViewModel.CustomControls.TasksPanelSubpanels;
using DesktopWPFManagementApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DesktopWPFManagementApp.MVVM.ViewModel.CustomControls.TasksPanelSubpanels.EarnSiteTasksPanelUserControlViewModel;

namespace DesktopWPFManagementApp.Commands.EarnSiteTasksPanel
{
    internal class GetEarnSiteTasksCollectionCommand : RelayCommand
    {
        // properties
        private EarnSiteTasksPanelUserControlViewModel earnSiteTasksPanelUserControlViewModel;

        // events
        public event Action<FoundedTasks> GetTaskCollectionSuccessfully = (FoundedTasks tasks) => { };
        public event Action<string> GetTaskCollectionFailed = (string msg) => { };
        public event Action ConnectionError = () => { };

        public GetEarnSiteTasksCollectionCommand(EarnSiteTasksPanelUserControlViewModel earnSiteTasksPanelUserControlViewModel)
        {
            this.earnSiteTasksPanelUserControlViewModel = earnSiteTasksPanelUserControlViewModel;
        }

        public override bool CanExecute(object? parameter)
        {
            return earnSiteTasksPanelUserControlViewModel.LoadAndLookTasksActionCanExecute && (earnSiteTasksPanelUserControlViewModel.LoadingDataStatus == LoadDataStatusEnum.DataIsLoaded.ToString() || earnSiteTasksPanelUserControlViewModel.LoadingDataStatus == LoadDataStatusEnum.DataErrorLoad.ToString()) && base.CanExecute(parameter);
        }

        public override async void Execute(object? parameter)
        {
            // set load view visible
            earnSiteTasksPanelUserControlViewModel.LoadingDataStatus = LoadDataStatusEnum.DataIsLoading.ToString();

            // logic
            CancellationTokenSource cancellationTokenSourceGetTaskCollection = new CancellationTokenSource(TimeSpan.FromSeconds(300));
            FindTasks? findTasks = null;
            bool canTryToGetTaskCollection = false;
            FoundedTasks? foundedTasks = null;
            string getTaskCollectionFailedText = "Произошла непредвиденная ошибка.";

            // try to caste findTasks parameter
            try
            {
                findTasks = (parameter as FindTasks);
                if (findTasks != null)
                {
                    canTryToGetTaskCollection = true;
                }
                else
                {
                    getTaskCollectionFailedText = "Не удалось получить данные для получения списка заданий.";
                }
            }
            catch { }

            // try to get collection
            if (canTryToGetTaskCollection)
            {
                try
                {
                    foundedTasks = await ServiceStorage.EarnTaskService.GetTasksCollection(findTasks!, cancellationTokenSourceGetTaskCollection.Token);

                    if (foundedTasks != null && foundedTasks.FoundedTasksStatus == true)
                    {
                        // set load view invisible
                        earnSiteTasksPanelUserControlViewModel.LoadingDataStatus = LoadDataStatusEnum.DataIsLoaded.ToString();
                        GetTaskCollectionSuccessfully?.Invoke(foundedTasks);
                    }
                    else if (foundedTasks != null && foundedTasks.FoundedTasksStatus == false)
                    {
                        earnSiteTasksPanelUserControlViewModel.LoadingDataStatus = LoadDataStatusEnum.DataErrorLoad.ToString();
                        GetTaskCollectionFailed?.Invoke(foundedTasks.FoundedTasksStatusText);
                    }
                    else if (foundedTasks == null)
                    {
                        earnSiteTasksPanelUserControlViewModel.LoadingDataStatus = LoadDataStatusEnum.DataErrorLoad.ToString();
                        getTaskCollectionFailedText = "Произошла непредвиденная ошибка.";
                        GetTaskCollectionFailed?.Invoke(getTaskCollectionFailedText);
                    }
                }
                catch(Exception e)
                {
                    earnSiteTasksPanelUserControlViewModel.LoadingDataStatus = LoadDataStatusEnum.DataErrorLoad.ToString();
                    ConnectionError?.Invoke();
                }
            }
            else
            {
                earnSiteTasksPanelUserControlViewModel.LoadingDataStatus = LoadDataStatusEnum.DataErrorLoad.ToString();
                GetTaskCollectionFailed?.Invoke(getTaskCollectionFailedText);
            }
        }
    }
}
