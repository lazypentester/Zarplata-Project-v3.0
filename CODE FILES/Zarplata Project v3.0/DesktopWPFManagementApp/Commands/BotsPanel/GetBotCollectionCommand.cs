using CommonModels.Client.Models.SearchBotsModels;
using CommonModels.User.Models;
using DesktopWPFManagementApp.MVVM.ViewModel.CustomControls;
using DesktopWPFManagementApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DesktopWPFManagementApp.MVVM.ViewModel.CustomControls.AuthPanelUserControlViewModel;
using static DesktopWPFManagementApp.MVVM.ViewModel.CustomControls.BotsPanelUserControlViewModel;

namespace DesktopWPFManagementApp.Commands.BotsPanel
{
    internal class GetBotCollectionCommand : RelayCommand
    {
        // properties
        private BotsPanelUserControlViewModel botsPanelUserControlViewModel;

        // events
        public event Action<FoundedBots> GetBotCollectionSuccessfully = (FoundedBots bots) => { };
        public event Action<string> GetBotCollectionFailed = (string msg) => { };
        public event Action ConnectionError = () => { };

        public GetBotCollectionCommand(BotsPanelUserControlViewModel botsPanelUserControlViewModel)
        {
            this.botsPanelUserControlViewModel = botsPanelUserControlViewModel;
        }

        public override bool CanExecute(object? parameter)
        {
            return botsPanelUserControlViewModel.LoadAndLookBotsActionCanExecute && (botsPanelUserControlViewModel.LoadingDataStatus == LoadDataStatusEnum.DataIsLoaded.ToString() || botsPanelUserControlViewModel.LoadingDataStatus == LoadDataStatusEnum.DataErrorLoad.ToString()) && base.CanExecute(parameter);
        }

        public override async void Execute(object? parameter)
        {
            // set load view visible
            botsPanelUserControlViewModel.LoadingDataStatus = LoadDataStatusEnum.DataIsLoading.ToString();

            // logic
            CancellationTokenSource cancellationTokenSourceGetBotCollection = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            FindBots? findBots = null;
            bool canTryToGetBotCollection = false;
            FoundedBots? foundedBots = null;
            string getBotCollectionFailedText = "Произошла непредвиденная ошибка.";

            // try to caste findBots parameter
            try
            {
                findBots = (parameter as FindBots);
                if (findBots != null)
                {
                    canTryToGetBotCollection = true;
                }
                else
                {
                    getBotCollectionFailedText = "Не удалось получить данные для получения списка ботов.";
                }
            }
            catch { }

            // try to get collection
            if (canTryToGetBotCollection)
            {
                try
                {
                    foundedBots = await ServiceStorage.BotService.GetBotsCollection(findBots!, cancellationTokenSourceGetBotCollection.Token);

                    if (foundedBots != null && foundedBots.FoundedBotsStatus == true)
                    {
                        // update bots collection in viewModel
                        //try
                        //{

                        //}
                        //catch { }

                        // set load view invisible
                        botsPanelUserControlViewModel.LoadingDataStatus = LoadDataStatusEnum.DataIsLoaded.ToString();
                        GetBotCollectionSuccessfully?.Invoke(foundedBots);
                    }
                    else if (foundedBots != null && foundedBots.FoundedBotsStatus == false)
                    {
                        botsPanelUserControlViewModel.LoadingDataStatus = LoadDataStatusEnum.DataErrorLoad.ToString();
                        GetBotCollectionFailed?.Invoke(foundedBots.FoundedBotsStatusText);
                    }
                    else if (foundedBots == null)
                    {
                        botsPanelUserControlViewModel.LoadingDataStatus = LoadDataStatusEnum.DataErrorLoad.ToString();
                        getBotCollectionFailedText = "Произошла непредвиденная ошибка.";
                        GetBotCollectionFailed?.Invoke(getBotCollectionFailedText);
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
                GetBotCollectionFailed?.Invoke(getBotCollectionFailedText);
            }
        }
    }
}
