using CommonModels.Client.Models.SearchBotsModels;
using DesktopWPFManagementApp.MVVM.ViewModel.CustomControls;
using DesktopWPFManagementApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static CommonModels.Client.Client;
using static DesktopWPFManagementApp.MVVM.ViewModel.CustomControls.BotsPanelUserControlViewModel;

namespace DesktopWPFManagementApp.Commands.BotsPanel
{
    internal class ChangeBotStatusCommand : RelayCommand
    {
        // properties
        private BotsPanelUserControlViewModel botsPanelUserControlViewModel;

        // events
        public event Action<List<string>, ClientStatus> ChangeBotStatusSuccessfully = (List<string> botsIdList, ClientStatus changedToStatus) => { };
        public event Action<string> ChangeBotStatusFailed = (string msg) => { };
        public event Action ConnectionError = () => { };

        public ChangeBotStatusCommand(BotsPanelUserControlViewModel botsPanelUserControlViewModel)
        {
            this.botsPanelUserControlViewModel = botsPanelUserControlViewModel;
        }

        public override bool CanExecute(object? parameter)
        {
            return botsPanelUserControlViewModel.LoadingDataStatus == LoadDataStatusEnum.DataIsLoaded.ToString() && base.CanExecute(parameter);
        }

        public override async void Execute(object? parameter)
        {
            // set load view visible
            botsPanelUserControlViewModel.LoadingDataStatus = LoadDataStatusEnum.DataIsLoading.ToString();

            // logic
            CancellationTokenSource cancellationTokenSourceChangeBotStatus = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            List<string>? changeStatusBotIdList = null;
            bool canTryToChangeBotsStatus = false;
            bool successfull = false;
            string changeBotsStatusFailedText = "Произошла непредвиденная ошибка.";

            // try to caste changeStatusBotIdList parameter
            try
            {
                changeStatusBotIdList = (parameter as List<string>);
                if (changeStatusBotIdList != null)
                {
                    canTryToChangeBotsStatus = true;
                }
                else
                {
                    changeBotsStatusFailedText = "Не удалось получить данные для изменения статуса ботов.";
                }
            }
            catch { }

            // try to change bots status
            if (canTryToChangeBotsStatus)
            {
                try
                {
                    ClientStatus changedToStatus;

                    if (botsPanelUserControlViewModel.BotsCollection.Where(bot => bot.ID == changeStatusBotIdList!.First()).First().Status == ClientStatus.Stopped)
                    {
                        changedToStatus = ClientStatus.Free;
                        successfull = await ServiceStorage.BotService.StartBots(changeStatusBotIdList!, cancellationTokenSourceChangeBotStatus.Token);
                    }
                    else
                    {
                        changedToStatus = ClientStatus.Stopped;
                        successfull = await ServiceStorage.BotService.StopBots(changeStatusBotIdList!, cancellationTokenSourceChangeBotStatus.Token);
                    }

                    if (successfull)
                    {
                        // update bots collection in viewModel
                        //try
                        //{

                        //}
                        //catch { }

                        // set load view invisible
                        botsPanelUserControlViewModel.LoadingDataStatus = LoadDataStatusEnum.DataIsLoaded.ToString();
                        ChangeBotStatusSuccessfully?.Invoke(changeStatusBotIdList!, changedToStatus);
                    }
                    else
                    {
                        botsPanelUserControlViewModel.LoadingDataStatus = LoadDataStatusEnum.DataErrorLoad.ToString();
                        changeBotsStatusFailedText = "Произошла непредвиденная ошибка.";
                        ChangeBotStatusFailed?.Invoke(changeBotsStatusFailedText);
                    }
                }
                catch
                {
                    botsPanelUserControlViewModel.LoadingDataStatus = LoadDataStatusEnum.DataErrorLoad.ToString();
                    ConnectionError?.Invoke();
                }
            }
            else
            {
                botsPanelUserControlViewModel.LoadingDataStatus = LoadDataStatusEnum.DataErrorLoad.ToString();
                ChangeBotStatusFailed?.Invoke(changeBotsStatusFailedText);
            }
        }
    }
}
