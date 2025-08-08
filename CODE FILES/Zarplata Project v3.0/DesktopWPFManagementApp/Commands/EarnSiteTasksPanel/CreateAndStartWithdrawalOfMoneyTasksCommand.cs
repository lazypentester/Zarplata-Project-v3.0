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
    internal class CreateAndStartWithdrawalOfMoneyTasksCommand : RelayCommand
    {
        // properties
        private EarnSiteTasksPanelUserControlViewModel earnSiteTasksPanelUserControlViewModel;

        // events
        public event Action CreateAndStartTasksSuccessfully = () => { };
        public event Action<string> CreateAndStartTasksFailed = (string msg) => { };
        public event Action ConnectionError = () => { };

        public CreateAndStartWithdrawalOfMoneyTasksCommand(EarnSiteTasksPanelUserControlViewModel earnSiteTasksPanelUserControlViewModel)
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
            CancellationTokenSource cancellationTokenSourceCreateAndStartTasks = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            string createAndStartTasksFailedText = "Произошла непредвиденная ошибка.";

            try
            {
                await ServiceStorage.EarnTaskService.CreateAndStartWithdrawalOfMoneyTasks(cancellationTokenSourceCreateAndStartTasks.Token);

                earnSiteTasksPanelUserControlViewModel.LoadingDataStatus = LoadDataStatusEnum.DataIsLoaded.ToString();
                CreateAndStartTasksSuccessfully?.Invoke();
            }
            catch
            {
                earnSiteTasksPanelUserControlViewModel.LoadingDataStatus = LoadDataStatusEnum.DataErrorLoad.ToString();
                ConnectionError?.Invoke();
            }
        }
    }
}
