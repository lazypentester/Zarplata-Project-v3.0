using CommonModels.Client.Models;
using CommonModels.Client.Models.DeleteBotsModels;
using DesktopWPFManagementApp.Models;
using DesktopWPFManagementApp.MVVM.ViewModel.CustomControls;
using DesktopWPFManagementApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using static CommonModels.Client.Client;
using static DesktopWPFManagementApp.MVVM.ViewModel.CustomControls.BotsPanelUserControlViewModel;

namespace DesktopWPFManagementApp.Commands.BotsPanel
{
    internal class DeleteBotCommand : RelayCommand
    {
        // properties
        private BotsPanelUserControlViewModel botsPanelUserControlViewModel;

        // events
        public event Action<List<ModelClient>?> DeleteBotSuccessfully = (List<ModelClient>? deletedBots) => { };
        public event Action<string> DeleteBotFailed = (string msg) => { };
        public event Action ConnectionError = () => { };

        public DeleteBotCommand(BotsPanelUserControlViewModel botsPanelUserControlViewModel)
        {
            this.botsPanelUserControlViewModel = botsPanelUserControlViewModel;
        }

        public override bool CanExecute(object? parameter)
        {
            return botsPanelUserControlViewModel.LoadingDataStatus == LoadDataStatusEnum.DataIsLoaded.ToString() && botsPanelUserControlViewModel.DeleteBotActionCanExecute && base.CanExecute(parameter);
        }

        public override async void Execute(object? parameter)
        {
            // hide Delete popup Panel
            botsPanelUserControlViewModel.IsDeleteBotPopupPanelOpen = false;

            // set load view visible
            botsPanelUserControlViewModel.LoadingDataStatus = LoadDataStatusEnum.DataIsLoading.ToString();

            // logic
            CancellationTokenSource cancellationTokenSourceDeleteBot = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            List<ModelClient>? modelClients = null;
            DeleteBots? deleteBotModel = null;
            DeletedBots? deletedBotModel = null;
            bool canTryToDeleteBots = false;
            string deleteBotsFailedText = "Произошла непредвиденная ошибка.";

            // try to create DeleteBots parameter
            try
            {
                if (botsPanelUserControlViewModel.DeleteBotsOneOrFewInfo == DeleteBotsOneOrFew.One && botsPanelUserControlViewModel.CurrentSelectedBot != null)
                {
                    modelClients = new List<ModelClient>();
                    modelClients.Add(new ModelClient()
                    {
                        ID = botsPanelUserControlViewModel.CurrentSelectedBot.ID,
                        SERVER_HOST = botsPanelUserControlViewModel.CurrentSelectedBot.SERVER_HOST,
                        RegistrationDateTime = botsPanelUserControlViewModel.CurrentSelectedBot.RegistrationDateTime,
                        Role = botsPanelUserControlViewModel.CurrentSelectedBot.Role,
                        IP = botsPanelUserControlViewModel.CurrentSelectedBot.IP,
                        MACHINE = botsPanelUserControlViewModel.CurrentSelectedBot.MACHINE,
                        Status = botsPanelUserControlViewModel.CurrentSelectedBot.Status
                    });

                    deleteBotModel = new DeleteBots(modelClients, botsPanelUserControlViewModel.DeleteBotsWithBlockSettingIsChecked);
                }
                else if (botsPanelUserControlViewModel.DeleteBotsOneOrFewInfo == DeleteBotsOneOrFew.Few)
                {
                    modelClients = new List<ModelClient>();
                    foreach (var viewBot in botsPanelUserControlViewModel.BotsCollection.Where(bot => bot.IsMarked))
                    {
                        modelClients.Add(new ModelClient()
                        {
                            ID = viewBot.ID,
                            SERVER_HOST = viewBot.SERVER_HOST,
                            RegistrationDateTime = viewBot.RegistrationDateTime,
                            Role = viewBot.Role,
                            IP = viewBot.IP,
                            MACHINE = viewBot.MACHINE,
                            Status = viewBot.Status
                        });
                    }

                    deleteBotModel = new DeleteBots(modelClients, botsPanelUserControlViewModel.DeleteBotsWithBlockSettingIsChecked);
                }
                else
                {
                    throw new Exception();
                }

                canTryToDeleteBots = true;
            }
            catch
            {
                canTryToDeleteBots = false;
                deleteBotsFailedText = "Не удалось получить данные для удаления ботов.";
            }

            // try to delete bots
            if (canTryToDeleteBots)
            {
                try
                {
                    deletedBotModel = await ServiceStorage.BotService.DeleteBots(deleteBotModel!, cancellationTokenSourceDeleteBot.Token);

                    if (deletedBotModel != null && deletedBotModel.DeletedBotsStatus)
                    {
                        // update bots collection in viewModel
                        //try
                        //{

                        //}
                        //catch { }

                        // set load view invisible
                        botsPanelUserControlViewModel.LoadingDataStatus = LoadDataStatusEnum.DataIsLoaded.ToString();
                        DeleteBotSuccessfully?.Invoke(modelClients);
                    }
                    else if(deletedBotModel != null && !deletedBotModel.DeletedBotsStatus)
                    {
                        botsPanelUserControlViewModel.LoadingDataStatus = LoadDataStatusEnum.DataErrorLoad.ToString();
                        DeleteBotFailed?.Invoke(deletedBotModel.DeletedBotsStatusText);
                    }
                    else
                    {
                        botsPanelUserControlViewModel.LoadingDataStatus = LoadDataStatusEnum.DataErrorLoad.ToString();
                        deleteBotsFailedText = "Произошла непредвиденная ошибка.";
                        DeleteBotFailed?.Invoke(deleteBotsFailedText);
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
                DeleteBotFailed?.Invoke(deleteBotsFailedText);
            }
        }
    }
}
