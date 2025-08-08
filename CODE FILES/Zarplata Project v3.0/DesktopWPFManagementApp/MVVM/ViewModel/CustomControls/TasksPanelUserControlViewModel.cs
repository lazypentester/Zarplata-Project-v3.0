using DesktopWPFManagementApp.Commands;
using DesktopWPFManagementApp.MVVM.ViewModel.Core;
using DesktopWPFManagementApp.MVVM.ViewModel.CustomControls.TasksPanelSubpanels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DesktopWPFManagementApp.MVVM.ViewModel.Windows.MainWindowViewModel;

namespace DesktopWPFManagementApp.MVVM.ViewModel.CustomControls
{
    internal class TasksPanelUserControlViewModel : ViewModelBase
    {
        #region Enums

        public enum TaskTypeMenuPanelItemEnum
        {
            EarnSiteTasksPanel,
            EarnSiteTasksSettingsPanel,
            ProxyTasksPanel,
            ProxyTasksSettingsPanel,
            BotManagementTasksPanel,
            BotManagementTasksSettingsPanel,
            CaptchaTasksPanel,
            CaptchaTasksSettingsPanel,
        }

        #endregion

        #region Properties

        private ViewModelBase currentChildView;
        public ViewModelBase CurrentChildView
        {
            get { return currentChildView; }
            set
            {
                currentChildView = value;
                OnPropertyChanged();
            }
        }

        private TaskTypeMenuPanelItemEnum currentSelectedMenuPanelItem = TaskTypeMenuPanelItemEnum.EarnSiteTasksPanel;
        public TaskTypeMenuPanelItemEnum CurrentSelectedMenuPanelItem
        {
            get { return currentSelectedMenuPanelItem; }
            set { currentSelectedMenuPanelItem = value; }
        }

        // Child view panels
        private EarnSiteTasksPanelUserControlViewModel earnSiteTasksPaneUserControlVM { get; }
        private EarnSiteTasksSettingsPanelUserControlViewModel earnSiteTasksSettingsPaneUserControlVM { get; }
        private ProxyTasksPanelUserControlViewModel proxyTasksPaneUserControlVM { get; }
        private ProxyTasksSettingsPanelUserControlViewModel proxyTasksSettingsPaneUserControlVM { get; }
        private BotManagementTasksPanelUserControlViewModel botManagementTasksPaneUserControlVM { get; }
        private BotManagementTasksSettingsPanelUserControlViewModel botManagementTasksSettingsPaneUserControlVM { get; }
        private CaptchaTasksPanelUserControlViewModel captchaTasksPaneUserControlVM { get; }
        private CaptchaTasksSettingsPanelUserControlViewModel captchaTasksSettingsPaneUserControlVM { get; }

        #endregion

        #region Commands

        public RelayCommand openEarnSiteTasksPanelCommand { get; }
        public RelayCommand openEarnSiteTasksSettingsPanelCommand { get; }
        public RelayCommand openProxyTasksPanelCommand { get; }
        public RelayCommand openProxyTasksSettingsPanelCommand { get; }
        public RelayCommand openBotManagementTasksPanelCommand { get; }
        public RelayCommand openBotManagementTasksSettingsPanelCommand { get; }
        public RelayCommand openCaptchaTasksPanelCommand { get; }
        public RelayCommand openCaptchaTasksSettingsPanelCommand { get; }

        #endregion

        public TasksPanelUserControlViewModel()
        {
            #region Init Child Views And CurrentView
            earnSiteTasksPaneUserControlVM = new EarnSiteTasksPanelUserControlViewModel();
            earnSiteTasksSettingsPaneUserControlVM = new EarnSiteTasksSettingsPanelUserControlViewModel();
            proxyTasksPaneUserControlVM = new ProxyTasksPanelUserControlViewModel();
            proxyTasksSettingsPaneUserControlVM = new ProxyTasksSettingsPanelUserControlViewModel();
            botManagementTasksPaneUserControlVM = new BotManagementTasksPanelUserControlViewModel();
            botManagementTasksSettingsPaneUserControlVM = new BotManagementTasksSettingsPanelUserControlViewModel();
            captchaTasksPaneUserControlVM = new CaptchaTasksPanelUserControlViewModel();
            captchaTasksSettingsPaneUserControlVM = new CaptchaTasksSettingsPanelUserControlViewModel();

            currentChildView = earnSiteTasksPaneUserControlVM;
            #endregion

            #region Init Commands
            openEarnSiteTasksPanelCommand = new RelayCommand(execute =>
            {
                CurrentChildView = earnSiteTasksPaneUserControlVM;
                CurrentSelectedMenuPanelItem = TaskTypeMenuPanelItemEnum.EarnSiteTasksPanel;
                earnSiteTasksPaneUserControlVM.LoadTasksCollection();
            },
            canExecute =>
            {
                return true;
            });
            openEarnSiteTasksSettingsPanelCommand = new RelayCommand(execute =>
            {
                CurrentChildView = earnSiteTasksSettingsPaneUserControlVM;
                CurrentSelectedMenuPanelItem = TaskTypeMenuPanelItemEnum.EarnSiteTasksSettingsPanel;
            },
            canExecute =>
            {
                return true;
            });
            openProxyTasksPanelCommand = new RelayCommand(execute =>
            {
                CurrentChildView = proxyTasksPaneUserControlVM;
                CurrentSelectedMenuPanelItem = TaskTypeMenuPanelItemEnum.ProxyTasksPanel;
            },
            canExecute =>
            {
                return true;
            });
            openProxyTasksSettingsPanelCommand = new RelayCommand(execute =>
            {
                CurrentChildView = proxyTasksSettingsPaneUserControlVM;
                CurrentSelectedMenuPanelItem = TaskTypeMenuPanelItemEnum.ProxyTasksSettingsPanel;
            },
            canExecute =>
            {
                return true;
            });
            openBotManagementTasksPanelCommand = new RelayCommand(execute =>
            {
                CurrentChildView = botManagementTasksPaneUserControlVM;
                CurrentSelectedMenuPanelItem = TaskTypeMenuPanelItemEnum.BotManagementTasksPanel;
            },
            canExecute =>
            {
                return true;
            });
            openBotManagementTasksSettingsPanelCommand = new RelayCommand(execute =>
            {
                CurrentChildView = botManagementTasksSettingsPaneUserControlVM;
                CurrentSelectedMenuPanelItem = TaskTypeMenuPanelItemEnum.BotManagementTasksSettingsPanel;
            },
            canExecute =>
            {
                return true;
            });
            openCaptchaTasksPanelCommand = new RelayCommand(execute =>
            {
                CurrentChildView = captchaTasksPaneUserControlVM;
                CurrentSelectedMenuPanelItem = TaskTypeMenuPanelItemEnum.CaptchaTasksPanel;
            },
            canExecute =>
            {
                return true;
            });
            openCaptchaTasksSettingsPanelCommand = new RelayCommand(execute =>
            {
                CurrentChildView = captchaTasksSettingsPaneUserControlVM;
                CurrentSelectedMenuPanelItem = TaskTypeMenuPanelItemEnum.CaptchaTasksSettingsPanel;
            },
            canExecute =>
            {
                return true;
            });
            #endregion
        }

        public void LoadLastOpenPanel()
        {
            try
            {
                switch (CurrentSelectedMenuPanelItem)
                {
                    case TaskTypeMenuPanelItemEnum.EarnSiteTasksPanel:
                        openEarnSiteTasksPanelCommand.Execute(null);
                        break;
                    case TaskTypeMenuPanelItemEnum.EarnSiteTasksSettingsPanel:
                        openEarnSiteTasksSettingsPanelCommand.Execute(null);
                        break;
                    case TaskTypeMenuPanelItemEnum.ProxyTasksPanel:
                        openProxyTasksPanelCommand.Execute(null);
                        break;
                    case TaskTypeMenuPanelItemEnum.ProxyTasksSettingsPanel:
                        openProxyTasksSettingsPanelCommand.Execute(null);
                        break;
                    case TaskTypeMenuPanelItemEnum.BotManagementTasksPanel:
                        openBotManagementTasksPanelCommand.Execute(null);
                        break;
                    case TaskTypeMenuPanelItemEnum.BotManagementTasksSettingsPanel:
                        openBotManagementTasksSettingsPanelCommand.Execute(null);
                        break;
                    case TaskTypeMenuPanelItemEnum.CaptchaTasksPanel:
                        openCaptchaTasksPanelCommand.Execute(null);
                        break;
                    case TaskTypeMenuPanelItemEnum.CaptchaTasksSettingsPanel:
                        openCaptchaTasksSettingsPanelCommand.Execute(null);
                        break;
                }
            }
            catch { }
        }
    }
}
