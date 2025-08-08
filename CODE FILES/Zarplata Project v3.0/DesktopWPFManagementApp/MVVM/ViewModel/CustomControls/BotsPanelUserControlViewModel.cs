using CommonModels.Client.Models;
using CommonModels.Client.Models.DeleteBotsModels;
using CommonModels.Client.Models.SearchBotsModels;
using CommonModels.Client.Models.SearchBotsModels.FilterModels;
using CommonModels.User.Models;
using DesktopWPFManagementApp.Commands;
using DesktopWPFManagementApp.Commands.BotsPanel;
using DesktopWPFManagementApp.Models;
using DesktopWPFManagementApp.MVVM.ViewModel.Core;
using DesktopWPFManagementApp.Services;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static CommonModels.Client.Client;
using static CommonModels.Client.Machine;
using static CommonModels.Client.Models.SearchBotsModels.SearchBotEnums;

namespace DesktopWPFManagementApp.MVVM.ViewModel.CustomControls
{
    internal class BotsPanelUserControlViewModel : ViewModelBase
    {
        #region Enums
        public enum BotsPanelPageButtons
        {
            FirstPageButton = 1,
            SecondPageButton = 2,
            ThirdPageButton = 3,
            FourthPageButton = 4,
            FifthPageButton = 5,
            SixthPageButton = 6
        }

        public enum LoadDataStatusEnum
        {
            DataIsLoaded,
            DataIsLoading,
            DataErrorLoad
        }

        public enum DeleteBotsOneOrFew
        {
            One,
            Few
        }
        public enum DGContextMenuStartOrStopBotsMassActionValue
        {
            StartBots,
            StopBots
        }
        #endregion

        #region Properties

        #region Main Properties
        private ObservableCollection<BotModel> botsCollection;
        public ObservableCollection<BotModel> BotsCollection
        {
            get { return botsCollection; }
            set
            {
                botsCollection = value;
            }
        }

        private BotModel? currentSelectedBot;
        public BotModel? CurrentSelectedBot
        {
            get { return currentSelectedBot; }
            set 
            { 
                currentSelectedBot = value;
                OnPropertyChanged();
            }
        }

        private FindBots findBotsInfo;
        public FindBots FindBotsInfo
        {
            get { return findBotsInfo; }
            set { findBotsInfo = value; }
        }

        private int markedBotsCount = 0;
        public int MarkedBotsCount
        {
            get { return markedBotsCount; }
            set
            {
                markedBotsCount = value;
                OnPropertyChanged();
            }
        }

        private int allBotsCount = 0;
        public int AllBotsCount
        {
            get { return allBotsCount; }
            set
            {
                allBotsCount = value;
                OnPropertyChanged();
            }
        }

        private int firstElementNumberOfSelectedPage = 0;
        public int FirstElementNumberOfSelectedPage
        {
            get { return firstElementNumberOfSelectedPage; }
            set 
            { 
                firstElementNumberOfSelectedPage = value;
                OnPropertyChanged();
            }
        }

        private int lastElementNumberOfSelectedPage = 0;
        public int LastElementNumberOfSelectedPage
        {
            get { return lastElementNumberOfSelectedPage; }
            set
            {
                lastElementNumberOfSelectedPage = value;
                OnPropertyChanged();
            }
        }

        private string loadingDataStatus = LoadDataStatusEnum.DataIsLoaded.ToString();
        public string LoadingDataStatus
        {
            get { return loadingDataStatus; }
            set
            {
                loadingDataStatus = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Bots Actions Settings

        private bool loadAndLookBotsActionCanExecute = false;
        public bool LoadAndLookBotsActionCanExecute
        {
            get { return loadAndLookBotsActionCanExecute; }
            set
            {
                loadAndLookBotsActionCanExecute = value;
                OnPropertyChanged();
            }
        }

        // copy bot as text action
        private Visibility copyActionVisibility = Visibility.Collapsed;
        public Visibility CopyActionVisibility
        {
            get { return copyActionVisibility; }
            set
            {
                copyActionVisibility = value;
                OnPropertyChanged();
            }
        }

        private bool copyActionCanExecute = false;
        public bool CopyActionCanExecute
        {
            get { return copyActionCanExecute; }
            set
            {
                copyActionCanExecute = value;
                OnPropertyChanged();
            }
        }

        // check bot details action
        private Visibility checkBotDetailsActionVisibility = Visibility.Collapsed;
        public Visibility CheckBotDetailsActionVisibility
        {
            get { return checkBotDetailsActionVisibility; }
            set
            {
                checkBotDetailsActionVisibility = value;
                OnPropertyChanged();
            }
        }

        private bool checkBotDetailsActionCanExecute = false;
        public bool CheckBotDetailsActionCanExecute
        {
            get { return checkBotDetailsActionCanExecute; }
            set
            {
                checkBotDetailsActionCanExecute = value;
                OnPropertyChanged();
            }
        }

        // stop/start bot action
        private Visibility stopOrStartBotActionVisibility = Visibility.Collapsed;
        public Visibility StopOrStartBotActionVisibility
        {
            get { return stopOrStartBotActionVisibility; }
            set
            {
                stopOrStartBotActionVisibility = value;
                OnPropertyChanged();
            }
        }

        private bool stopOrStartBotActionCanExecute = false;
        public bool StopOrStartBotActionCanExecute
        {
            get { return stopOrStartBotActionCanExecute; }
            set
            {
                stopOrStartBotActionCanExecute = value;
                OnPropertyChanged();
            }
        }

        // delete with block bot action
        private Visibility deleteBotActionVisibility = Visibility.Collapsed;
        public Visibility DeleteBotActionVisibility
        {
            get { return deleteBotActionVisibility; }
            set
            {
                deleteBotActionVisibility = value;
                OnPropertyChanged();
            }
        }

        private bool deleteBotActionCanExecute = false;
        public bool DeleteBotActionCanExecute
        {
            get { return deleteBotActionCanExecute; }
            set
            {
                deleteBotActionCanExecute = value;
                OnPropertyChanged();
            }
        }

        private bool blockBotMachineActionCanExecute = false;
        public bool BlockBotMachineActionCanExecute
        {
            get { return blockBotMachineActionCanExecute; }
            set
            {
                blockBotMachineActionCanExecute = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Filter Bots Popup
        private bool isFilterPopupPanelOpen = false;
        public bool IsFilterPopupPanelOpen
        {
            get { return isFilterPopupPanelOpen; }
            set 
            {
                isFilterPopupPanelOpen = value;
                OnPropertyChanged();
            }
        }

        private bool filterItemRoleEarningSiteBotIsChecked = true;
        public bool FilterItemRoleEarningSiteBotIsChecked
        {
            get { return filterItemRoleEarningSiteBotIsChecked; }
            set 
            {
                filterItemRoleEarningSiteBotIsChecked = value;
                OnPropertyChanged();
            }
        }

        private bool filterItemRoleProxyCombineBotIsChecked = true;
        public bool FilterItemRoleProxyCombineBotIsChecked
        {
            get { return filterItemRoleProxyCombineBotIsChecked; }
            set
            {
                filterItemRoleProxyCombineBotIsChecked = value;
                OnPropertyChanged();
            }
        }

        private bool filterItemRoleBotManagerBotIsChecked = true;
        public bool FilterItemRoleBotManagerBotIsChecked
        {
            get { return filterItemRoleBotManagerBotIsChecked; }
            set
            {
                filterItemRoleBotManagerBotIsChecked = value;
                OnPropertyChanged();
            }
        }

        private bool filterItemOSPlatformLinuxIsChecked = true;
        public bool FilterItemOSPlatformLinuxIsChecked
        {
            get { return filterItemOSPlatformLinuxIsChecked; }
            set
            {
                filterItemOSPlatformLinuxIsChecked = value;
                OnPropertyChanged();
            }
        }

        private bool filterItemOSPlatformWindowsIsChecked = true;
        public bool FilterItemOSPlatformWindowsIsChecked
        {
            get { return filterItemOSPlatformWindowsIsChecked; }
            set
            {
                filterItemOSPlatformWindowsIsChecked = value;
                OnPropertyChanged();
            }
        }

        private bool filterItemOSPlatformMacOsIsChecked = true;
        public bool FilterItemOSPlatformMacOsIsChecked
        {
            get { return filterItemOSPlatformMacOsIsChecked; }
            set
            {
                filterItemOSPlatformMacOsIsChecked = value;
                OnPropertyChanged();
            }
        }

        private bool filterItemOSPlatformOtherIsChecked = true;
        public bool FilterItemOSPlatformOtherIsChecked
        {
            get { return filterItemOSPlatformOtherIsChecked; }
            set
            {
                filterItemOSPlatformOtherIsChecked = value;
                OnPropertyChanged();
            }
        }

        private bool filterItemBusyStatusStartedIsChecked = true;
        public bool FilterItemBusyStatusStartedIsChecked
        {
            get { return filterItemBusyStatusStartedIsChecked; }
            set
            {
                filterItemBusyStatusStartedIsChecked = value;
                OnPropertyChanged();
            }
        }

        private bool filterItemBusyStatusConnectedIsChecked = true;
        public bool FilterItemBusyStatusConnectedIsChecked
        {
            get { return filterItemBusyStatusConnectedIsChecked; }
            set
            {
                filterItemBusyStatusConnectedIsChecked = value;
                OnPropertyChanged();
            }
        }

        private bool filterItemBusyStatusFreeIsChecked = true;
        public bool FilterItemBusyStatusFreeIsChecked
        {
            get { return filterItemBusyStatusFreeIsChecked; }
            set
            {
                filterItemBusyStatusFreeIsChecked = value;
                OnPropertyChanged();
            }
        }

        private bool filterItemBusyStatusAtWorkIsChecked = true;
        public bool FilterItemBusyStatusAtWorkIsChecked
        {
            get { return filterItemBusyStatusAtWorkIsChecked; }
            set
            {
                filterItemBusyStatusAtWorkIsChecked = value;
                OnPropertyChanged();
            }
        }

        private bool filterItemBusyStatusStoppedIsChecked = true;
        public bool FilterItemBusyStatusStoppedIsChecked
        {
            get { return filterItemBusyStatusStoppedIsChecked; }
            set
            {
                filterItemBusyStatusStoppedIsChecked = value;
                OnPropertyChanged();
            }
        }

        private bool filterItemBusyStatusDisconnectedIsChecked = true;
        public bool FilterItemBusyStatusDisconnectedIsChecked
        {
            get { return filterItemBusyStatusDisconnectedIsChecked; }
            set
            {
                filterItemBusyStatusDisconnectedIsChecked = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Bot Info Popup
        private bool isBotInfoPopupPanelOpen = false;
        public bool IsBotInfoPopupPanelOpen
        {
            get { return isBotInfoPopupPanelOpen; }
            set
            {
                isBotInfoPopupPanelOpen = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Delete Bot Popup
        private bool isDeleteBotPopupPanelOpen = false;
        public bool IsDeleteBotPopupPanelOpen
        {
            get { return isDeleteBotPopupPanelOpen; }
            set
            {
                isDeleteBotPopupPanelOpen = value;
                OnPropertyChanged();
            }
        }

        private int botsCountToDelete = 0;
        public int BotsCountToDelete
        {
            get { return botsCountToDelete; }
            set 
            { 
                botsCountToDelete = value;
                OnPropertyChanged();
            }
        }

        private DeleteBotsOneOrFew deleteBotsOneOrFewInfo = DeleteBotsOneOrFew.One;
        public DeleteBotsOneOrFew DeleteBotsOneOrFewInfo
        {
            get { return deleteBotsOneOrFewInfo; }
            set { deleteBotsOneOrFewInfo = value; }
        }


        private bool deleteBotsWithBlockSettingIsChecked = false;
        public bool DeleteBotsWithBlockSettingIsChecked
        {
            get { return deleteBotsWithBlockSettingIsChecked; }
            set 
            { 
                deleteBotsWithBlockSettingIsChecked = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region DataGrid ContextMenu Actions

        private DGContextMenuStartOrStopBotsMassActionValue startOrStopBotsMassActionValue = DGContextMenuStartOrStopBotsMassActionValue.StopBots;
        public DGContextMenuStartOrStopBotsMassActionValue StartOrStopBotsMassActionValue
        {
            get { return startOrStopBotsMassActionValue; }
            set 
            { 
                startOrStopBotsMassActionValue = value;
                OnPropertyChanged();
            }
        }

        private bool stopOrStartBotContextMenuActionCanExecute = false;
        public bool StopOrStartBotContextMenuActionCanExecute
        {
            get { return stopOrStartBotContextMenuActionCanExecute; }
            set 
            { 
                stopOrStartBotContextMenuActionCanExecute = value;
                OnPropertyChanged();
            }
        }

        private bool deleteBotContextMenuActionCanExecute = true;
        public bool DeleteBotContextMenuActionCanExecute
        {
            get { return deleteBotContextMenuActionCanExecute; }
            set
            {
                deleteBotContextMenuActionCanExecute = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Search Bot UserControl
        private ObservableCollection<KeyValuePair<string, FindSearchKeywordParametersBots>> searchUserControlListCombobox;
        public ObservableCollection<KeyValuePair<string, FindSearchKeywordParametersBots>> SearchUserControlListCombobox
        {
            get { return searchUserControlListCombobox; }
            set
            {
                searchUserControlListCombobox = value;
                OnPropertyChanged();
            }
        }

        private KeyValuePair<string, FindSearchKeywordParametersBots> selectedItemSearchUserControlCombobox;
        public KeyValuePair<string, FindSearchKeywordParametersBots> SelectedItemSearchUserControlCombobox
        {
            get { return selectedItemSearchUserControlCombobox; }
            set
            {
                selectedItemSearchUserControlCombobox = value;
                OnPropertyChanged();
            }
        }

        private string searchUserControlKeywordText = "";
        public string SearchUserControlKeywordText
        {
            get { return searchUserControlKeywordText; }
            set 
            {
                searchUserControlKeywordText = value;
                OnPropertyChanged();
            }
        }

        private bool incorrectInputData = false;
        public bool IncorrectInputData
        {
            get { return incorrectInputData; }
            set 
            { 
                incorrectInputData = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Sort Bot List
        private ObservableCollection<KeyValuePair<string, FindSortBots>> searchSortListCombobox;
        public ObservableCollection<KeyValuePair<string, FindSortBots>> SearchSortCountListCombobox
        {
            get { return searchSortListCombobox; }
            set
            {
                searchSortListCombobox = value;
                OnPropertyChanged();
            }
        }

        private KeyValuePair<string, FindSortBots> selectedSearchSortCountCombobox;
        public KeyValuePair<string, FindSortBots> SelectedSearchSortCountCombobox
        {
            get { return selectedSearchSortCountCombobox; }
            set 
            { 
                selectedSearchSortCountCombobox = value;
                FindBotsInfo.FindSortBots = value.Value;
                LoadBotsCollection();
            }
        }
        #endregion

        #region Count of displayed elements per page
        private ObservableCollection<KeyValuePair<string, int>> searchPageCountListCombobox;
        public ObservableCollection<KeyValuePair<string, int>> SearchPageCountListCombobox
        {
            get { return searchPageCountListCombobox; }
            set
            {
                searchPageCountListCombobox = value;
                OnPropertyChanged();
            }
        }

        private KeyValuePair<string, int> selectedSearchPageCountCombobox;
        public KeyValuePair<string, int> SelectedSearchPageCountCombobox
        {
            get { return selectedSearchPageCountCombobox; }
            set 
            { 
                selectedSearchPageCountCombobox = value;
                FindBotsInfo.ResultsPerPage = selectedSearchPageCountCombobox.Value;
                LoadBotsCollection();
            }
        }
        #endregion

        #region Navigation Page Buttons

        #region Next & Previous Buttons
        private bool nextPageButtonIsEnabled = false;
        public bool NextPageButtonIsEnabled
        {
            get { return nextPageButtonIsEnabled; }
            set 
            { 
                nextPageButtonIsEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool previousPageButtonIsEnabled = false;
        public bool PreviousPageButtonIsEnabled
        {
            get { return previousPageButtonIsEnabled; }
            set 
            { 
                previousPageButtonIsEnabled = value;
                OnPropertyChanged();
            }
        }

        private int nextPageButtonValue = 0;
        public int NextPageButtonValue
        {
            get { return nextPageButtonValue; }
            set 
            { 
                nextPageButtonValue = value;
                OnPropertyChanged();
            }
        }

        private int previousPageButtonValue = 0;
        public int PreviousPageButtonValue
        {
            get { return previousPageButtonValue; }
            set 
            { 
                previousPageButtonValue = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Main Navigations Page Buttons
        // selected value
        private int currentSelectedPageButton = (int)BotsPanelPageButtons.FirstPageButton;
        public int CurrentSelectedPageButton
        {
            get { return currentSelectedPageButton; }
            set 
            {
                currentSelectedPageButton = value;
                OnPropertyChanged();
            }
        }

        private int currentSelectedPageButtonValue = 1;
        public int CurrentSelectedPageButtonValue
        {
            get { return currentSelectedPageButtonValue; }
            set
            {
                currentSelectedPageButtonValue = value;
                OnPropertyChanged();
            }
        }

        // button values
        private int firstPageButtonValue = 1;
        public int FirstPageButtonValue
        {
            get { return firstPageButtonValue; }
            set 
            { 
                firstPageButtonValue = value;
                OnPropertyChanged();
            }
        }

        private int secondPageButtonValue = 2;
        public int SecondPageButtonValue
        {
            get { return secondPageButtonValue; }
            set
            {
                secondPageButtonValue = value;
                OnPropertyChanged();
            }
        }

        private int thirdPageButtonValue = 3;
        public int ThirdPageButtonValue
        {
            get { return thirdPageButtonValue; }
            set
            {
                thirdPageButtonValue = value;
                OnPropertyChanged();
            }
        }

        private int fourthPageButtonValue = 4;
        public int FourthPageButtonValue
        {
            get { return fourthPageButtonValue; }
            set
            {
                fourthPageButtonValue = value;
                OnPropertyChanged();
            }
        }

        private int fifthPageButtonValue = 5;
        public int FifthPageButtonValue
        {
            get { return fifthPageButtonValue; }
            set
            {
                fifthPageButtonValue = value;
                OnPropertyChanged();
            }
        }

        private int sixthPageButtonValue = 6;
        public int SixthPageButtonValue
        {
            get { return sixthPageButtonValue; }
            set
            {
                sixthPageButtonValue = value;
                OnPropertyChanged();
            }
        }

        // button visible state
        private Visibility firstPageButtonVisibility = Visibility.Collapsed;
        public Visibility FirstPageButtonVisibility
        {
            get { return firstPageButtonVisibility; }
            set 
            { 
                firstPageButtonVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility secondPageButtonVisibility = Visibility.Collapsed;
        public Visibility SecondPageButtonVisibility
        {
            get { return secondPageButtonVisibility; }
            set
            {
                secondPageButtonVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility thirdPageButtonVisibility = Visibility.Collapsed;
        public Visibility ThirdPageButtonVisibility
        {
            get { return thirdPageButtonVisibility; }
            set
            {
                thirdPageButtonVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility navigationSplitterTextBlockVisibility = Visibility.Collapsed;
        public Visibility NavigationSplitterTextBlockVisibility
        {
            get { return navigationSplitterTextBlockVisibility; }
            set
            {
                navigationSplitterTextBlockVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility fourthPageButtonVisibility = Visibility.Collapsed;
        public Visibility FourthPageButtonVisibility
        {
            get { return fourthPageButtonVisibility; }
            set
            {
                fourthPageButtonVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility fifthPageButtonVisibility = Visibility.Collapsed;
        public Visibility FifthPageButtonVisibility
        {
            get { return fifthPageButtonVisibility; }
            set
            {
                fifthPageButtonVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility sixthPageButtonVisibility = Visibility.Collapsed;
        public Visibility SixthPageButtonVisibility
        {
            get { return sixthPageButtonVisibility; }
            set
            {
                sixthPageButtonVisibility = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #endregion

        #region Events
        public event Action<string?> ErrorHasOccurred = (string? msg) => { };
        #endregion

        #endregion

        #region Commands

        public GetBotCollectionCommand GetBotCollectionCommand { get; private set; }

        public RelayCommand NavigationButtonClickedCommand { get; private set; }

        public RelayCommand MarkOneBotCommand { get; private set; }
        public RelayCommand MarkAllBotsCommand { get; private set; }
        public RelayCommand CopyCellValueCommand { get; private set; }
        public RelayCommand CopyRowValueCommand { get; private set; }

        public RelayCommand OpenBotCollectionFilterPanelCommand { get; private set; }
        public RelayCommand ApplyFilterBotCollectionFilterPanelCommand { get; private set; }
        public RelayCommand CloseBotCollectionFilterPanelCommand { get; private set; }

        public RelayCommand SearchKeywordButtonIsPressed { get; private set; }

        public RelayCommand OpenBotInfoPanelCommand { get; private set; }
        public RelayCommand CloseBotInfoPanelCommand { get; private set; }

        public ChangeBotStatusCommand ChangeBotStatusMainCommand { get; private set; }
        public RelayCommand ChangeOneBotStatusCommand { get; private set; }
        public RelayCommand ChangeFewBotsStatusCommand { get; private set; }

        public DeleteBotCommand DeleteBotMainCommand { get; private set; }
        public RelayCommand OpenDeleteOneBotPanelCommand { get; private set; }
        public RelayCommand OpenDeleteFewBotsPanelCommand { get; private set; }
        public RelayCommand DeleteBotsWithBlockSettingCheckCommand { get; private set; }
        public RelayCommand CloseDeleteBotPanelCommand { get; private set; }

        public RelayCommand DataGridContextMenuPreparationBeforeOpenCommand { get; private set; }

        #endregion

        public BotsPanelUserControlViewModel()
        {
            #region Init Properties

            ConfigureUserAction();

            botsCollection = new ObservableCollection<BotModel>();
            currentSelectedBot = BotsCollection.FirstOrDefault();

            searchPageCountListCombobox = new ObservableCollection<KeyValuePair<string, int>>()
            {
                new KeyValuePair<string, int>("10 элементов на странице", 10),
                new KeyValuePair<string, int>("50 элементов на странице", 50),
                new KeyValuePair<string, int>("100 элементов на странице", 100)
            };
            selectedSearchPageCountCombobox = searchPageCountListCombobox[0];

            searchSortListCombobox = new ObservableCollection<KeyValuePair<string, FindSortBots>>()
            {
                new KeyValuePair<string, FindSortBots>("Сначала новые", FindSortBots.NewFirst),
                new KeyValuePair<string, FindSortBots>("Сначала старые", FindSortBots.OldFirst)
            };
            selectedSearchSortCountCombobox = searchSortListCombobox[0];

            searchUserControlListCombobox = new ObservableCollection<KeyValuePair<string, FindSearchKeywordParametersBots>>()
            {
                new KeyValuePair<string, FindSearchKeywordParametersBots>("Id бота", FindSearchKeywordParametersBots.Id),
                new KeyValuePair<string, FindSearchKeywordParametersBots>("Ip адресс", FindSearchKeywordParametersBots.Ip),
                new KeyValuePair<string, FindSearchKeywordParametersBots>("Имя компьютера", FindSearchKeywordParametersBots.MachineName),
                new KeyValuePair<string, FindSearchKeywordParametersBots>("Имя пользователя", FindSearchKeywordParametersBots.UserName),
                new KeyValuePair<string, FindSearchKeywordParametersBots>("Операционная система", FindSearchKeywordParametersBots.OSPlatformText),
                new KeyValuePair<string, FindSearchKeywordParametersBots>("Дата регистрации в системе", FindSearchKeywordParametersBots.RegistrationDateTime),
                new KeyValuePair<string, FindSearchKeywordParametersBots>("Уникальный идентификатор компьютера", FindSearchKeywordParametersBots.IdentityKey),
            };
            selectedItemSearchUserControlCombobox = searchUserControlListCombobox[0];

            findBotsInfo = new FindBots(
                CurrentSelectedPageButtonValue,
                selectedSearchPageCountCombobox.Value,
                selectedSearchSortCountCombobox.Value,
                new FindFilterBots()
                );

            FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Add(FindFilterParametersBots.Role.ToString(), new List<int>());
            FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Add("Machine." + FindFilterParametersBots.OSPlatform.ToString(), new List<int>());
            FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Add(FindFilterParametersBots.Status.ToString(), new List<int>());

            FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersBots.Role.ToString()).First().Value.Add((int)BotRole.EarningSiteBot);
            FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersBots.Role.ToString()).First().Value.Add((int)BotRole.ProxyCombineBot);
            FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersBots.Role.ToString()).First().Value.Add((int)BotRole.BotManager);

            FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == "Machine." + FindFilterParametersBots.OSPlatform.ToString()).First().Value.Add(((int)Platform.Linux));
            FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == "Machine." + FindFilterParametersBots.OSPlatform.ToString()).First().Value.Add(((int)Platform.Windows));
            FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == "Machine." + FindFilterParametersBots.OSPlatform.ToString()).First().Value.Add(((int)Platform.MacOS));
            FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == "Machine." + FindFilterParametersBots.OSPlatform.ToString()).First().Value.Add(((int)Platform.Other));

            FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersBots.Status.ToString()).First().Value.Add((int)ClientStatus.Started);
            FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersBots.Status.ToString()).First().Value.Add((int)ClientStatus.Connected);
            FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersBots.Status.ToString()).First().Value.Add((int)ClientStatus.AtWork);
            FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersBots.Status.ToString()).First().Value.Add((int)ClientStatus.Free);
            FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersBots.Status.ToString()).First().Value.Add((int)ClientStatus.Stopped);
            FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersBots.Status.ToString()).First().Value.Add((int)ClientStatus.Disconnected);

            #endregion

            #region Init Commands

            GetBotCollectionCommand = new GetBotCollectionCommand(this);

            NavigationButtonClickedCommand = new RelayCommand(execute =>
            {
                try
                {
                    string? buttonName = execute as string;
                    if(buttonName != null)
                    {
                        switch (buttonName)
                        {
                            case "NavigationPreviousButton":
                                CurrentSelectedPageButtonValue = PreviousPageButtonValue;
                                FindBotsInfo.SelectedPageNumber = CurrentSelectedPageButtonValue;
                                LoadBotsCollection();
                                break;
                            case "NavigationFirstButton":
                                CurrentSelectedPageButtonValue = FirstPageButtonValue;
                                FindBotsInfo.SelectedPageNumber = CurrentSelectedPageButtonValue;
                                LoadBotsCollection();
                                break;
                            case "NavigationSecondButton":
                                CurrentSelectedPageButtonValue = SecondPageButtonValue;
                                FindBotsInfo.SelectedPageNumber = CurrentSelectedPageButtonValue;
                                LoadBotsCollection();
                                break;
                            case "NavigationThirdButton":
                                CurrentSelectedPageButtonValue = ThirdPageButtonValue;
                                FindBotsInfo.SelectedPageNumber = CurrentSelectedPageButtonValue;
                                LoadBotsCollection();
                                break;
                            case "NavigationFourthButton":
                                CurrentSelectedPageButtonValue = FourthPageButtonValue;
                                FindBotsInfo.SelectedPageNumber = CurrentSelectedPageButtonValue;
                                LoadBotsCollection();
                                break;
                            case "NavigationFifthButton":
                                CurrentSelectedPageButtonValue = FifthPageButtonValue;
                                FindBotsInfo.SelectedPageNumber = CurrentSelectedPageButtonValue;
                                LoadBotsCollection();
                                break;
                            case "NavigationSixthButton":
                                CurrentSelectedPageButtonValue = SixthPageButtonValue;
                                FindBotsInfo.SelectedPageNumber = CurrentSelectedPageButtonValue;
                                LoadBotsCollection();
                                break;
                            case "NavigationNextButton":
                                CurrentSelectedPageButtonValue = NextPageButtonValue;
                                FindBotsInfo.SelectedPageNumber = CurrentSelectedPageButtonValue;
                                LoadBotsCollection();
                                break;
                        }
                    }
                }
                catch { }
            },
            canExecute =>
            {
                return LoadingDataStatus != LoadDataStatusEnum.DataIsLoading.ToString();
            });

            MarkOneBotCommand = new RelayCommand(execute =>
            {
                bool? booleanParam = (execute as bool?);
                if (booleanParam.HasValue)
                {
                    bool CheckOneCheckBoxIsChecked = booleanParam.Value;

                    if (CheckOneCheckBoxIsChecked)
                    {
                        MarkedBotsCount++;
                    }
                    else if (!CheckOneCheckBoxIsChecked)
                    {
                        MarkedBotsCount--;
                    }
                }
            },
            canExecute =>
            {
                return canExecute != null;
            });
            MarkAllBotsCommand = new RelayCommand(execute =>
            {
                bool? booleanParam = (execute as bool?);
                if (booleanParam.HasValue)
                {
                    bool CheckAllCheckBoxIsChecked = booleanParam.Value;

                    if (CheckAllCheckBoxIsChecked)
                    {
                        foreach (var bot in BotsCollection) bot.IsMarked = true;
                        MarkedBotsCount = BotsCollection.Count;
                    }
                    else if (!CheckAllCheckBoxIsChecked)
                    {
                        foreach (var bot in BotsCollection) bot.IsMarked = false;
                        MarkedBotsCount = 0;
                    }
                }
            },
            canExecute =>
            {
                return canExecute != null && BotsCollection.Count > 0;
            });
            CopyCellValueCommand = new RelayCommand(execute =>
            {
                try
                {
                    Clipboard.SetDataObject(execute);
                }
                catch { }
            },
            canExecute =>
            {
                return canExecute != null;
            });
            CopyRowValueCommand = new RelayCommand(execute =>
            {
                try
                {
                    BotModel? model = execute as BotModel;
                    CurrentSelectedBot = model;
                    StringBuilder stringBuilder = new StringBuilder();

                    stringBuilder.Append(nameof(model.ID) + ":");
                    stringBuilder.AppendLine(model!.ID);

                    stringBuilder.Append(nameof(model.RegistrationDateTime) + ":");
                    stringBuilder.AppendLine(model!.RegistrationDateTimePreviewFormat);

                    stringBuilder.Append(nameof(model.Role) + ":");
                    stringBuilder.AppendLine(model!.Role.ToString());

                    stringBuilder.Append(nameof(model.IP) + ":");
                    stringBuilder.AppendLine(model.IP);

                    stringBuilder.Append(nameof(model.MACHINE.MACHINE_NAME) + ":");
                    stringBuilder.AppendLine(model.MACHINE.MACHINE_NAME);

                    stringBuilder.Append(nameof(model.MACHINE.USER_NAME) + ":");
                    stringBuilder.AppendLine(model.MACHINE.USER_NAME);

                    stringBuilder.Append(nameof(model.MACHINE.OS_PLATFORM) + ":");
                    stringBuilder.AppendLine(model.MACHINE.OS_PLATFORM_TEXT);

                    stringBuilder.Append(nameof(model.MACHINE.IDENTITY_KEY) + ":");
                    stringBuilder.AppendLine(model.MACHINE.IDENTITY_KEY);

                    stringBuilder.Append(nameof(model.Status) + ":");
                    stringBuilder.AppendLine(model.Status.ToString());

                    Clipboard.SetDataObject(stringBuilder.ToString());
                }
                catch { }
            },
            canExecute =>
            {
                return canExecute != null && CopyActionCanExecute;
            });

            OpenBotCollectionFilterPanelCommand = new RelayCommand(execute =>
            {
                IsFilterPopupPanelOpen = true;

                var checkRolesList = FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersBots.Role.ToString()).FirstOrDefault().Value;
                if (checkRolesList != null)
                {
                    if(checkRolesList.Contains(((int)BotRole.EarningSiteBot)))
                        FilterItemRoleEarningSiteBotIsChecked = true;
                    else
                        FilterItemRoleEarningSiteBotIsChecked = false;

                    if (checkRolesList.Contains(((int)BotRole.ProxyCombineBot)))
                        FilterItemRoleProxyCombineBotIsChecked = true;
                    else
                        FilterItemRoleProxyCombineBotIsChecked = false;

                    if (checkRolesList.Contains(((int)BotRole.BotManager)))
                        FilterItemRoleBotManagerBotIsChecked = true;
                    else
                        FilterItemRoleBotManagerBotIsChecked = false;
                }

                var checkOSPlatformsList = FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == "Machine." + FindFilterParametersBots.OSPlatform.ToString()).FirstOrDefault().Value;
                if (checkOSPlatformsList != null)
                {
                    if (checkOSPlatformsList.Contains(((int)Platform.Linux)))
                        FilterItemOSPlatformLinuxIsChecked = true;
                    else
                        FilterItemOSPlatformLinuxIsChecked = false;

                    if (checkOSPlatformsList.Contains(((int)Platform.Windows)))
                        FilterItemOSPlatformWindowsIsChecked = true;
                    else
                        FilterItemOSPlatformWindowsIsChecked = false;

                    if (checkOSPlatformsList.Contains(((int)Platform.MacOS)))
                        FilterItemOSPlatformMacOsIsChecked = true;
                    else
                        FilterItemOSPlatformMacOsIsChecked = false;

                    if (checkOSPlatformsList.Contains(((int)Platform.Other)))
                        FilterItemOSPlatformOtherIsChecked = true;
                    else
                        FilterItemOSPlatformOtherIsChecked = false;
                }

                var checkBusyStatusesList = FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersBots.Status.ToString()).FirstOrDefault().Value;
                if (checkBusyStatusesList != null)
                {
                    if (checkBusyStatusesList.Contains(((int)ClientStatus.Started)))
                        FilterItemBusyStatusStartedIsChecked = true;
                    else
                        FilterItemBusyStatusStartedIsChecked = false;

                    if (checkBusyStatusesList.Contains(((int)ClientStatus.Connected)))
                        FilterItemBusyStatusConnectedIsChecked = true;
                    else
                        FilterItemBusyStatusConnectedIsChecked = false;

                    if (checkBusyStatusesList.Contains(((int)ClientStatus.Free)))
                        FilterItemBusyStatusFreeIsChecked = true;
                    else
                        FilterItemBusyStatusFreeIsChecked = false;

                    if (checkBusyStatusesList.Contains(((int)ClientStatus.AtWork)))
                        FilterItemBusyStatusAtWorkIsChecked = true;
                    else
                        FilterItemBusyStatusAtWorkIsChecked = false;

                    if (checkBusyStatusesList.Contains(((int)ClientStatus.Stopped)))
                        FilterItemBusyStatusStoppedIsChecked = true;
                    else
                        FilterItemBusyStatusStoppedIsChecked = false;

                    if (checkBusyStatusesList.Contains(((int)ClientStatus.Disconnected)))
                        FilterItemBusyStatusDisconnectedIsChecked = true;
                    else
                        FilterItemBusyStatusDisconnectedIsChecked = false;
                }
            },
            canExecute =>
            {
                return (LoadingDataStatus != LoadDataStatusEnum.DataIsLoading.ToString() && !IsFilterPopupPanelOpen && !IsBotInfoPopupPanelOpen);
            });
            ApplyFilterBotCollectionFilterPanelCommand = new RelayCommand(execute =>
            {
                // clear all filters
                FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersBots.Role.ToString()).FirstOrDefault().Value.Clear();
                FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == "Machine." + FindFilterParametersBots.OSPlatform.ToString()).FirstOrDefault().Value.Clear();
                FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersBots.Status.ToString()).FirstOrDefault().Value.Clear();

                // add or remove role filters
                if (FilterItemRoleEarningSiteBotIsChecked)
                    FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersBots.Role.ToString()).FirstOrDefault().Value.Add((int)BotRole.EarningSiteBot);

                if (FilterItemRoleProxyCombineBotIsChecked)
                    FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersBots.Role.ToString()).FirstOrDefault().Value.Add((int)BotRole.ProxyCombineBot);

                if (FilterItemRoleBotManagerBotIsChecked)
                    FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersBots.Role.ToString()).FirstOrDefault().Value.Add((int)BotRole.BotManager);

                // add or remove osPlatform filters
                if (FilterItemOSPlatformLinuxIsChecked)
                    FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == "Machine." + FindFilterParametersBots.OSPlatform.ToString()).FirstOrDefault().Value.Add((int)Platform.Linux);

                if (FilterItemOSPlatformWindowsIsChecked)
                    FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == "Machine." + FindFilterParametersBots.OSPlatform.ToString()).FirstOrDefault().Value.Add((int)Platform.Windows);

                if (FilterItemOSPlatformMacOsIsChecked)
                    FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == "Machine." + FindFilterParametersBots.OSPlatform.ToString()).FirstOrDefault().Value.Add((int)Platform.MacOS);

                if (FilterItemOSPlatformOtherIsChecked)
                    FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == "Machine." + FindFilterParametersBots.OSPlatform.ToString()).FirstOrDefault().Value.Add((int)Platform.Other);

                // add or remove Status filters
                if (FilterItemBusyStatusStartedIsChecked)
                    FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersBots.Status.ToString()).FirstOrDefault().Value.Add((int)ClientStatus.Started);

                if (FilterItemBusyStatusConnectedIsChecked)
                    FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersBots.Status.ToString()).FirstOrDefault().Value.Add((int)ClientStatus.Connected);

                if (FilterItemBusyStatusFreeIsChecked)
                    FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersBots.Status.ToString()).FirstOrDefault().Value.Add((int)ClientStatus.Free);

                if (FilterItemBusyStatusAtWorkIsChecked)
                    FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersBots.Status.ToString()).FirstOrDefault().Value.Add((int)ClientStatus.AtWork);

                if (FilterItemBusyStatusStoppedIsChecked)
                    FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersBots.Status.ToString()).FirstOrDefault().Value.Add((int)ClientStatus.Stopped);

                if (FilterItemBusyStatusDisconnectedIsChecked)
                    FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersBots.Status.ToString()).FirstOrDefault().Value.Add((int)ClientStatus.Disconnected);

                IsFilterPopupPanelOpen = false;

                LoadBotsCollection();
            },
            canExecute =>
            {
                return (LoadingDataStatus != LoadDataStatusEnum.DataIsLoading.ToString() && IsFilterPopupPanelOpen && !IsBotInfoPopupPanelOpen);
            });
            CloseBotCollectionFilterPanelCommand = new RelayCommand(execute =>
            {
                IsFilterPopupPanelOpen = false;
            },
            canExecute =>
            {
                return IsFilterPopupPanelOpen;
            });

            SearchKeywordButtonIsPressed = new RelayCommand(execute =>
            {
                switch (SelectedItemSearchUserControlCombobox.Value)
                {
                    case FindSearchKeywordParametersBots.Id:
                        FindBotsInfo.FindFilterBots.FindSearchKeyword = new KeyValuePair<string, string>(FindSearchKeywordParametersBots.Id.ToString(), SearchUserControlKeywordText);
                        LoadBotsCollection();
                        break;
                    case FindSearchKeywordParametersBots.Ip:
                        FindBotsInfo.FindFilterBots.FindSearchKeyword = new KeyValuePair<string, string>(FindSearchKeywordParametersBots.Ip.ToString(), SearchUserControlKeywordText);
                        LoadBotsCollection();
                        break;
                    case FindSearchKeywordParametersBots.MachineName:
                        FindBotsInfo.FindFilterBots.FindSearchKeyword = new KeyValuePair<string, string>("Machine." + FindSearchKeywordParametersBots.MachineName.ToString(), SearchUserControlKeywordText);
                        LoadBotsCollection();
                        break;
                    case FindSearchKeywordParametersBots.UserName:
                        FindBotsInfo.FindFilterBots.FindSearchKeyword = new KeyValuePair<string, string>("Machine." + FindSearchKeywordParametersBots.UserName.ToString(), SearchUserControlKeywordText);
                        LoadBotsCollection();
                        break;
                    case FindSearchKeywordParametersBots.RegistrationDateTime:
                        try
                        {
                            if (SearchUserControlKeywordText.ToLower().Contains("дата") && SearchUserControlKeywordText.ToLower().Contains("время"))
                            {
                                string[] date_and_time = SearchUserControlKeywordText.Split(' ');
                                string date_str = date_and_time[0].Replace("дата=", "");
                                string time_str = date_and_time[1].Replace("время=", "");
                                DateTime datetime;
                                TimeOnly time;
                                if(DateTime.TryParse(date_str, out datetime))
                                {
                                    if(TimeOnly.TryParse(time_str, out time))
                                    {
                                        datetime = datetime.AddHours(time.Hour);
                                        datetime = datetime.AddMinutes(time.Minute);
                                        datetime = datetime.AddSeconds(time.Second);

                                        FindBotsInfo.FindFilterBots.FindSearchKeyword = new KeyValuePair<string, string>(FindSearchKeywordParametersBots.RegistrationDateTime.ToString(), SearchUserControlKeywordText);
                                        FindBotsInfo.FindFilterBots.FindSearchKeywordDateTime = datetime;
                                        LoadBotsCollection();
                                    }
                                    else
                                    {
                                        throw new Exception();
                                    }
                                }
                                else
                                {
                                    throw new Exception();
                                }
                            }
                            else if (SearchUserControlKeywordText.ToLower().Contains("дата"))
                            {
                                string date_str = SearchUserControlKeywordText.Replace("дата=", "");
                                DateTime datetime;
                                if (DateTime.TryParse(date_str, out datetime))
                                {
                                    FindBotsInfo.FindFilterBots.FindSearchKeyword = new KeyValuePair<string, string>(FindSearchKeywordParametersBots.RegistrationDateTime.ToString(), SearchUserControlKeywordText);
                                    FindBotsInfo.FindFilterBots.FindSearchKeywordDateTime = datetime;
                                    LoadBotsCollection();
                                }
                                else
                                {
                                    throw new Exception();
                                }
                            }
                            else if (SearchUserControlKeywordText.ToLower().Contains("date") && SearchUserControlKeywordText.ToLower().Contains("time"))
                            {
                                string[] date_and_time = SearchUserControlKeywordText.Split(' ');
                                string date_str = date_and_time[0].Replace("date=", "");
                                string time_str = date_and_time[1].Replace("time=", "");
                                DateTime datetime;
                                TimeOnly time;
                                if (DateTime.TryParse(date_str, out datetime))
                                {
                                    if (TimeOnly.TryParse(time_str, out time))
                                    {
                                        datetime = datetime.AddHours(time.Hour);
                                        datetime = datetime.AddMinutes(time.Minute);
                                        datetime = datetime.AddSeconds(time.Second);

                                        FindBotsInfo.FindFilterBots.FindSearchKeyword = new KeyValuePair<string, string>(FindSearchKeywordParametersBots.RegistrationDateTime.ToString(), SearchUserControlKeywordText);
                                        FindBotsInfo.FindFilterBots.FindSearchKeywordDateTime = datetime;
                                        LoadBotsCollection();
                                    }
                                    else
                                    {
                                        throw new Exception();
                                    }
                                }
                                else
                                {
                                    throw new Exception();
                                }
                            }
                            else if (SearchUserControlKeywordText.ToLower().Contains("date"))
                            {
                                string date_str = SearchUserControlKeywordText.Replace("date=", "");
                                DateTime datetime;
                                if (DateTime.TryParse(date_str, out datetime))
                                {
                                    FindBotsInfo.FindFilterBots.FindSearchKeyword = new KeyValuePair<string, string>(FindSearchKeywordParametersBots.RegistrationDateTime.ToString(), SearchUserControlKeywordText);
                                    FindBotsInfo.FindFilterBots.FindSearchKeywordDateTime = datetime;
                                    LoadBotsCollection();
                                }
                                else
                                {
                                    throw new Exception();
                                }
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                        catch 
                        {
                            IncorrectInputData = true;
                            IncorrectInputData = false;
                        }
                        break;
                    case FindSearchKeywordParametersBots.OSPlatformText:
                        FindBotsInfo.FindFilterBots.FindSearchKeyword = new KeyValuePair<string, string>("Machine." + FindSearchKeywordParametersBots.OSPlatformText.ToString(), SearchUserControlKeywordText);
                        LoadBotsCollection();
                        break;
                    case FindSearchKeywordParametersBots.IdentityKey:
                        FindBotsInfo.FindFilterBots.FindSearchKeyword = new KeyValuePair<string, string>("Machine." + FindSearchKeywordParametersBots.IdentityKey.ToString(), SearchUserControlKeywordText);
                        LoadBotsCollection();
                        break;
                    default:
                        SearchUserControlKeywordText = "";
                        break;
                }
            },
            canExecute =>
            {
                return LoadingDataStatus != LoadDataStatusEnum.DataIsLoading.ToString();
            });

            OpenBotInfoPanelCommand = new RelayCommand(execute =>
            {
                CurrentSelectedBot = execute as BotModel;
                IsBotInfoPopupPanelOpen = true;
            },
            canExecute =>
            {
                return (CheckBotDetailsActionCanExecute && LoadingDataStatus != LoadDataStatusEnum.DataIsLoading.ToString() && canExecute != null && !IsFilterPopupPanelOpen && !IsBotInfoPopupPanelOpen);
            });
            CloseBotInfoPanelCommand = new RelayCommand(execute =>
            {
                IsBotInfoPopupPanelOpen = false;
            },
            canExecute =>
            {
                return IsBotInfoPopupPanelOpen;
            });

            ChangeBotStatusMainCommand = new ChangeBotStatusCommand(this);
            ChangeOneBotStatusCommand = new RelayCommand(execute =>
            {
                try
                {
                    BotModel? model = execute as BotModel;

                    if (ChangeBotStatusMainCommand.CanExecute(null))
                    {
                        List<string> listIds = new List<string>()
                        {
                            model!.ID
                        };

                        ChangeBotStatusMainCommand.Execute(listIds);
                    }
                }
                catch { }
            },
            canExecute =>
            {
                return (StopOrStartBotActionCanExecute && LoadingDataStatus != LoadDataStatusEnum.DataIsLoading.ToString() && canExecute != null);
            });
            ChangeFewBotsStatusCommand = new RelayCommand(execute =>
            {
                try
                {
                    if (ChangeBotStatusMainCommand.CanExecute(null))
                    {
                        List<string> listIds = new List<string>();
                        listIds.AddRange(BotsCollection.Where(bot => bot.IsMarked).Select(bot => bot.ID));

                        ChangeBotStatusMainCommand.Execute(listIds);
                    }
                }
                catch { }
            },
            canExecute =>
            {
                return (LoadingDataStatus != LoadDataStatusEnum.DataIsLoading.ToString() && StopOrStartBotActionCanExecute && StopOrStartBotContextMenuActionCanExecute);
            });

            DeleteBotMainCommand = new DeleteBotCommand(this);
            OpenDeleteOneBotPanelCommand = new RelayCommand(execute =>
            {
                DeleteBotsWithBlockSettingIsChecked = false;
                CurrentSelectedBot = execute as BotModel;
                BotsCountToDelete = 1;
                DeleteBotsOneOrFewInfo = DeleteBotsOneOrFew.One;
                IsDeleteBotPopupPanelOpen = true;
            },
            canExecute =>
            {
                return (LoadingDataStatus != LoadDataStatusEnum.DataIsLoading.ToString() && DeleteBotActionCanExecute && canExecute != null && !IsDeleteBotPopupPanelOpen && !IsFilterPopupPanelOpen && !IsBotInfoPopupPanelOpen);
            });
            OpenDeleteFewBotsPanelCommand = new RelayCommand(execute =>
            {
                DeleteBotsWithBlockSettingIsChecked = false;
                BotsCountToDelete = MarkedBotsCount;
                DeleteBotsOneOrFewInfo = DeleteBotsOneOrFew.Few;
                IsDeleteBotPopupPanelOpen = true;
            },
            canExecute =>
            {
                return (LoadingDataStatus != LoadDataStatusEnum.DataIsLoading.ToString() && DeleteBotActionCanExecute && DeleteBotContextMenuActionCanExecute && !IsDeleteBotPopupPanelOpen && !IsFilterPopupPanelOpen && !IsBotInfoPopupPanelOpen);
            });
            DeleteBotsWithBlockSettingCheckCommand = new RelayCommand(execute =>
            {
                bool? booleanParam = (execute as bool?);
                if (booleanParam.HasValue)
                {
                    bool BlockSettingCheckBoxIsChecked = booleanParam.Value;

                    if (BlockSettingCheckBoxIsChecked)
                    {
                        DeleteBotsWithBlockSettingIsChecked = false;
                    }
                    else if (!BlockSettingCheckBoxIsChecked)
                    {
                        DeleteBotsWithBlockSettingIsChecked = true;
                    }
                }
            },
            canExecute =>
            {
                return (BlockBotMachineActionCanExecute && IsDeleteBotPopupPanelOpen);
            });
            CloseDeleteBotPanelCommand = new RelayCommand(execute =>
            {
                IsDeleteBotPopupPanelOpen = false;
            },
            canExecute =>
            {
                return IsDeleteBotPopupPanelOpen;
            });

            DataGridContextMenuPreparationBeforeOpenCommand = new RelayCommand(execute =>
            {
                BotModel? botForCheckMassAction = BotsCollection.Where(bot => bot.IsMarked).FirstOrDefault();
                if (botForCheckMassAction != null)
                {
                    //prepair for delete
                    DeleteBotContextMenuActionCanExecute = true;

                    // prepair for change status
                    StartOrStopBotsMassActionValue = botForCheckMassAction.Status switch
                    {
                        ClientStatus.Stopped => DGContextMenuStartOrStopBotsMassActionValue.StartBots,
                        ClientStatus.AtWork => DGContextMenuStartOrStopBotsMassActionValue.StopBots,
                        ClientStatus.Free => DGContextMenuStartOrStopBotsMassActionValue.StopBots,
                        _ => DGContextMenuStartOrStopBotsMassActionValue.StopBots
                    };

                    if(BotsCollection.Where(bot => bot.IsMarked && bot.Status != botForCheckMassAction.Status).Count() > 0)
                    {
                        StopOrStartBotContextMenuActionCanExecute = false;
                    }
                    else
                    {
                        StopOrStartBotContextMenuActionCanExecute = true;
                    }
                }
                else
                {
                    //prepair for delete
                    DeleteBotContextMenuActionCanExecute = false;

                    // prepair for change status
                    StopOrStartBotContextMenuActionCanExecute = false;
                }
            },
            canExecute =>
            {
                return botsCollection.Count > 0;
            });

            #endregion

            #region init event handlers

            GetBotCollectionCommand.ConnectionError += GetBotCollectionCommand_ConnectionError;
            GetBotCollectionCommand.GetBotCollectionFailed += GetBotCollectionCommand_GetBotCollectionFailed;
            GetBotCollectionCommand.GetBotCollectionSuccessfully += GetBotCollectionCommand_GetBotCollectionSuccessfully;

            ChangeBotStatusMainCommand.ChangeBotStatusSuccessfully += ChangeBotStatusMainCommand_ChangeBotStatusSuccessfully;
            ChangeBotStatusMainCommand.ChangeBotStatusFailed += ChangeBotStatusMainCommand_ChangeBotStatusFailed;
            ChangeBotStatusMainCommand.ConnectionError += ChangeBotStatusMainCommand_ConnectionError;

            DeleteBotMainCommand.ConnectionError += DeleteBotMainCommand_ConnectionError;
            DeleteBotMainCommand.DeleteBotFailed += DeleteBotMainCommand_DeleteBotFailed;
            DeleteBotMainCommand.DeleteBotSuccessfully += DeleteBotMainCommand_DeleteBotSuccessfully;

            ServiceStorage.BotService.NewBotConnected += BotService_NewBotConnected;
            ServiceStorage.BotService.BotsStatusChanged += BotService_BotsStatusChanged;
            ServiceStorage.BotService.BotClosed += BotService_BotClosed;
            ServiceStorage.BotService.BotsDeleted += BotService_BotsDeleted;

            #endregion
        }

        public void LoadBotsCollection()
        {
            if (GetBotCollectionCommand.CanExecute(null))
            {
                GetBotCollectionCommand.Execute(FindBotsInfo);
            }
        }
        public void ConfigureUserAction()
        {
            if (Services.ServiceStorage.UserService.CanUserExecuteAction(UserActions.CanLookBots))
            {
                LoadAndLookBotsActionCanExecute = true;
            }
            else
            {
                LoadAndLookBotsActionCanExecute = false;
            }

            if (Services.ServiceStorage.UserService.CanUserExecuteAction(UserActions.CanCopyBots))
            {
                CopyActionVisibility = Visibility.Visible;
                CopyActionCanExecute = true;
            }
            else
            {
                CopyActionVisibility = Visibility.Collapsed;
                CopyActionCanExecute = false;
            }

            if (Services.ServiceStorage.UserService.CanUserExecuteAction(UserActions.CanCheckDetailsBots))
            {
                CheckBotDetailsActionVisibility = Visibility.Visible;
                CheckBotDetailsActionCanExecute = true;
            }
            else
            {
                CheckBotDetailsActionVisibility = Visibility.Collapsed;
                CheckBotDetailsActionCanExecute = false;
            }

            if (Services.ServiceStorage.UserService.CanUserExecuteAction(UserActions.CanStopOrStartBots))
            {
                StopOrStartBotActionVisibility = Visibility.Visible;
                StopOrStartBotActionCanExecute = true;
            }
            else
            {
                StopOrStartBotActionVisibility = Visibility.Collapsed;
                StopOrStartBotActionCanExecute = false;
            }

            if (Services.ServiceStorage.UserService.CanUserExecuteAction(UserActions.CanDeleteBots))
            {
                DeleteBotActionVisibility = Visibility.Visible;
                DeleteBotActionCanExecute = true;
            }
            else
            {
                DeleteBotActionVisibility = Visibility.Collapsed;
                DeleteBotActionCanExecute = false;
            }

            if (Services.ServiceStorage.UserService.CanUserExecuteAction(UserActions.CanBlockBots))
            {
                BlockBotMachineActionCanExecute = true;
            }
            else
            {
                BlockBotMachineActionCanExecute = false;
            }
        }
        private void ConfigureNavigationButtonElements(int selectedPage, int allPagesCount)
        {   
            CurrentSelectedPageButtonValue = selectedPage;
            PreviousPageButtonValue = selectedPage - 1;
            NextPageButtonValue = selectedPage + 1;

            switch (allPagesCount)
            {
                case 1:
                    CurrentSelectedPageButton = 1;

                    NextPageButtonIsEnabled = false;
                    PreviousPageButtonIsEnabled = false;

                    FirstPageButtonVisibility = Visibility.Visible;
                    SecondPageButtonVisibility = Visibility.Collapsed;
                    ThirdPageButtonVisibility = Visibility.Collapsed;
                    FourthPageButtonVisibility = Visibility.Collapsed;
                    FifthPageButtonVisibility = Visibility.Collapsed;
                    SixthPageButtonVisibility = Visibility.Collapsed;
                    NavigationSplitterTextBlockVisibility = Visibility.Collapsed;

                    FirstPageButtonValue = 1;
                    break;
                case 2:
                    if(selectedPage == 1)
                    {
                        CurrentSelectedPageButton = 1;
                        NextPageButtonIsEnabled = true;
                        PreviousPageButtonIsEnabled = false;
                    }
                    else if(selectedPage == 2)
                    {
                        CurrentSelectedPageButton = 2;
                        NextPageButtonIsEnabled = false;
                        PreviousPageButtonIsEnabled = true;
                    }

                    FirstPageButtonVisibility = Visibility.Visible;
                    SecondPageButtonVisibility = Visibility.Visible;
                    ThirdPageButtonVisibility = Visibility.Collapsed;
                    FourthPageButtonVisibility = Visibility.Collapsed;
                    FifthPageButtonVisibility = Visibility.Collapsed;
                    SixthPageButtonVisibility = Visibility.Collapsed;
                    NavigationSplitterTextBlockVisibility = Visibility.Collapsed;

                    FirstPageButtonValue = 1;
                    SecondPageButtonValue = 2;
                    break;
                case 3:
                    if (selectedPage == 1)
                    {
                        CurrentSelectedPageButton = 1;
                        NextPageButtonIsEnabled = true;
                        PreviousPageButtonIsEnabled = false;
                    }
                    else if (selectedPage == 2)
                    {
                        CurrentSelectedPageButton = 2;
                        NextPageButtonIsEnabled = true;
                        PreviousPageButtonIsEnabled = true;
                    }
                    else if (selectedPage == 3)
                    {
                        CurrentSelectedPageButton = 3;
                        NextPageButtonIsEnabled = false;
                        PreviousPageButtonIsEnabled = true;
                    }

                    FirstPageButtonVisibility = Visibility.Visible;
                    SecondPageButtonVisibility = Visibility.Visible;
                    ThirdPageButtonVisibility = Visibility.Visible;
                    FourthPageButtonVisibility = Visibility.Collapsed;
                    FifthPageButtonVisibility = Visibility.Collapsed;
                    SixthPageButtonVisibility = Visibility.Collapsed;
                    NavigationSplitterTextBlockVisibility = Visibility.Collapsed;

                    FirstPageButtonValue = 1;
                    SecondPageButtonValue = 2;
                    ThirdPageButtonValue = 3;
                    break;
                case 4:
                    if (selectedPage == 1)
                    {
                        CurrentSelectedPageButton = 1;
                        NextPageButtonIsEnabled = true;
                        PreviousPageButtonIsEnabled = false;
                    }
                    else if (selectedPage == 2)
                    {
                        CurrentSelectedPageButton = 2;
                        NextPageButtonIsEnabled = true;
                        PreviousPageButtonIsEnabled = true;
                    }
                    else if (selectedPage == 3)
                    {
                        CurrentSelectedPageButton = 3;
                        NextPageButtonIsEnabled = true;
                        PreviousPageButtonIsEnabled = true;
                    }
                    else if (selectedPage == 4)
                    {
                        CurrentSelectedPageButton = 4;
                        NextPageButtonIsEnabled = false;
                        PreviousPageButtonIsEnabled = true;
                    }

                    FirstPageButtonVisibility = Visibility.Visible;
                    SecondPageButtonVisibility = Visibility.Visible;
                    ThirdPageButtonVisibility = Visibility.Visible;
                    FourthPageButtonVisibility = Visibility.Visible;
                    FifthPageButtonVisibility = Visibility.Collapsed;
                    SixthPageButtonVisibility = Visibility.Collapsed;
                    NavigationSplitterTextBlockVisibility = Visibility.Collapsed;

                    FirstPageButtonValue = 1;
                    SecondPageButtonValue = 2;
                    ThirdPageButtonValue = 3;
                    FourthPageButtonValue = 4;
                    break;
                case 5:
                    if (selectedPage == 1)
                    {
                        CurrentSelectedPageButton = 1;
                        NextPageButtonIsEnabled = true;
                        PreviousPageButtonIsEnabled = false;
                    }
                    else if (selectedPage == 2)
                    {
                        CurrentSelectedPageButton = 2;
                        NextPageButtonIsEnabled = true;
                        PreviousPageButtonIsEnabled = true;
                    }
                    else if (selectedPage == 3)
                    {
                        CurrentSelectedPageButton = 3;
                        NextPageButtonIsEnabled = true;
                        PreviousPageButtonIsEnabled = true;
                    }
                    else if (selectedPage == 4)
                    {
                        CurrentSelectedPageButton = 4;
                        NextPageButtonIsEnabled = true;
                        PreviousPageButtonIsEnabled = true;
                    }
                    else if (selectedPage == 5)
                    {
                        CurrentSelectedPageButton = 5;
                        NextPageButtonIsEnabled = false;
                        PreviousPageButtonIsEnabled = true;
                    }

                    FirstPageButtonVisibility = Visibility.Visible;
                    SecondPageButtonVisibility = Visibility.Visible;
                    ThirdPageButtonVisibility = Visibility.Visible;
                    FourthPageButtonVisibility = Visibility.Visible;
                    FifthPageButtonVisibility = Visibility.Visible;
                    SixthPageButtonVisibility = Visibility.Collapsed;
                    NavigationSplitterTextBlockVisibility = Visibility.Collapsed;

                    FirstPageButtonValue = 1;
                    SecondPageButtonValue = 2;
                    ThirdPageButtonValue = 3;
                    FourthPageButtonValue = 4;
                    FifthPageButtonValue = 5;
                    break;
                case 6:
                    if (selectedPage == 1)
                    {
                        CurrentSelectedPageButton = 1;
                        NextPageButtonIsEnabled = true;
                        PreviousPageButtonIsEnabled = false;
                    }
                    else if (selectedPage == 2)
                    {
                        CurrentSelectedPageButton = 2;
                        NextPageButtonIsEnabled = true;
                        PreviousPageButtonIsEnabled = true;
                    }
                    else if (selectedPage == 3)
                    {
                        CurrentSelectedPageButton = 3;
                        NextPageButtonIsEnabled = true;
                        PreviousPageButtonIsEnabled = true;
                    }
                    else if (selectedPage == 4)
                    {
                        CurrentSelectedPageButton = 4;
                        NextPageButtonIsEnabled = true;
                        PreviousPageButtonIsEnabled = true;
                    }
                    else if (selectedPage == 5)
                    {
                        CurrentSelectedPageButton = 5;
                        NextPageButtonIsEnabled = true;
                        PreviousPageButtonIsEnabled = true;
                    }
                    else if (selectedPage == 6)
                    {
                        CurrentSelectedPageButton = 6;
                        NextPageButtonIsEnabled = false;
                        PreviousPageButtonIsEnabled = true;
                    }

                    FirstPageButtonVisibility = Visibility.Visible;
                    SecondPageButtonVisibility = Visibility.Visible;
                    ThirdPageButtonVisibility = Visibility.Visible;
                    FourthPageButtonVisibility = Visibility.Visible;
                    FifthPageButtonVisibility = Visibility.Visible;
                    SixthPageButtonVisibility = Visibility.Visible;
                    NavigationSplitterTextBlockVisibility = Visibility.Collapsed;

                    FirstPageButtonValue = 1;
                    SecondPageButtonValue = 2;
                    ThirdPageButtonValue = 3;
                    FourthPageButtonValue = 4;
                    FifthPageButtonValue = 5;
                    SixthPageButtonValue = 6;
                    break;
                case > 6:
                    int ostatokRightButtons = allPagesCount - selectedPage;

                    if (ostatokRightButtons == 0)
                    {
                        PreviousPageButtonIsEnabled = true;
                        NextPageButtonIsEnabled = false;

                        CurrentSelectedPageButton = 6;

                        FirstPageButtonValue = selectedPage - 5;
                        SecondPageButtonValue = selectedPage - 4;
                        ThirdPageButtonValue = selectedPage - 3;
                    }
                    else if (ostatokRightButtons == 1)
                    {
                        PreviousPageButtonIsEnabled = true;
                        NextPageButtonIsEnabled = true;

                        CurrentSelectedPageButton = 5;

                        FirstPageButtonValue = selectedPage - 4;
                        SecondPageButtonValue = selectedPage - 3;
                        ThirdPageButtonValue = selectedPage - 2;
                    }
                    else if (ostatokRightButtons == 2)
                    {
                        PreviousPageButtonIsEnabled = true;
                        NextPageButtonIsEnabled = true;

                        CurrentSelectedPageButton = 4;

                        FirstPageButtonValue = selectedPage - 3;
                        SecondPageButtonValue = selectedPage - 2;
                        ThirdPageButtonValue = selectedPage - 1;
                    }
                    else if (ostatokRightButtons == 3)
                    {
                        PreviousPageButtonIsEnabled = true;
                        NextPageButtonIsEnabled = true;

                        CurrentSelectedPageButton = 3;

                        FirstPageButtonValue = selectedPage - 2;
                        SecondPageButtonValue = selectedPage - 1;
                        ThirdPageButtonValue = selectedPage;
                    }
                    else if (ostatokRightButtons == 4)
                    {
                        PreviousPageButtonIsEnabled = true;
                        NextPageButtonIsEnabled = true;

                        CurrentSelectedPageButton = 2;

                        FirstPageButtonValue = selectedPage - 1;
                        SecondPageButtonValue = selectedPage;
                        ThirdPageButtonValue = selectedPage + 1;
                    }
                    else if (ostatokRightButtons >= 5)
                    {
                        if(selectedPage == 1)
                        {
                            PreviousPageButtonIsEnabled = false;
                        }
                        else
                        {
                            PreviousPageButtonIsEnabled = true;
                        }

                        CurrentSelectedPageButton = 1;
                        NextPageButtonIsEnabled = true;

                        FirstPageButtonValue = selectedPage;
                        SecondPageButtonValue = selectedPage + 1;
                        ThirdPageButtonValue = selectedPage + 2;
                    }
                    else
                    {
                        // error
                    }

                    FourthPageButtonValue = allPagesCount - 2;
                    FifthPageButtonValue = allPagesCount - 1;
                    SixthPageButtonValue = allPagesCount;

                    FirstPageButtonVisibility = Visibility.Visible;
                    SecondPageButtonVisibility = Visibility.Visible;
                    ThirdPageButtonVisibility = Visibility.Visible;
                    FourthPageButtonVisibility = Visibility.Visible;
                    FifthPageButtonVisibility = Visibility.Visible;
                    SixthPageButtonVisibility = Visibility.Visible;
                    NavigationSplitterTextBlockVisibility = Visibility.Visible;

                    break;
            }
        }

        private void GetBotCollectionCommand_GetBotCollectionSuccessfully(FoundedBots bots)
        {
            AllBotsCount = bots.AllBotClientsCount;
            FirstElementNumberOfSelectedPage = bots.FirstElementNumberOfCurrentPage;
            LastElementNumberOfSelectedPage = bots.LastElementNumberOfCurrentPage;

            BotsCollection.Clear();
            MarkedBotsCount = 0;

            foreach (var bot in bots.BotClients)
            {
                BotsCollection.Add(new BotModel(
                    bot.ID!,
                    bot.SERVER_HOST!,
                    bot.RegistrationDateTime!.Value.ToLocalTime(),
                    bot.Role!.Value,
                    bot.IP!,
                    bot.MACHINE,
                    bot.Status
                    ));
            }

            ConfigureNavigationButtonElements(bots.SelectedPageNumber, bots.AllPagesCount);
        }
        private void GetBotCollectionCommand_GetBotCollectionFailed(string fail_text)
        {
            ErrorHasOccurred.Invoke(fail_text);
        }
        private void GetBotCollectionCommand_ConnectionError()
        {
            ErrorHasOccurred.Invoke("GetBotCollectionCommand_ConnectionError");
        }

        private void ChangeBotStatusMainCommand_ChangeBotStatusSuccessfully(List<string> idsList, ClientStatus changedToStatus)
        {
            try
            {
                foreach (string id in idsList)
                {
                    botsCollection.Where(bot => bot.ID == id).First().Status = changedToStatus;
                }
            }
            catch { }
        }
        private void ChangeBotStatusMainCommand_ChangeBotStatusFailed(string fail_text)
        {
            ErrorHasOccurred.Invoke(fail_text);
        }
        private void ChangeBotStatusMainCommand_ConnectionError()
        {
            ErrorHasOccurred.Invoke("ChangeBotStatusMainCommand_ConnectionError");
        }

        private void DeleteBotMainCommand_DeleteBotSuccessfully(List<ModelClient>? deletedBots)
        {
            try
            {
                if (deletedBots != null)
                {
                    foreach (ModelClient bot in deletedBots)
                    {
                        BotModel? botModel = botsCollection.Where(view_bot => view_bot.ID == bot.ID).FirstOrDefault();
                        if(botModel != null)
                        {
                            App.Current.Dispatcher.BeginInvoke(() =>
                            {
                                botsCollection.Remove(botModel);
                            });

                            LastElementNumberOfSelectedPage -= 1;
                            AllBotsCount -= 1;
                        }
                    } 
                }
            }
            catch { }
        }
        private void DeleteBotMainCommand_DeleteBotFailed(string fail_text)
        {
            ErrorHasOccurred.Invoke(fail_text);
        }
        private void DeleteBotMainCommand_ConnectionError()
        {
            ErrorHasOccurred.Invoke("DeleteBotMainCommand_ConnectionError");
        }

        private void BotService_NewBotConnected(ModelClient botNew)
        {
            try
            {
                if (LoadAndLookBotsActionCanExecute
                    && FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(key => key.Key == FindFilterParametersBots.Role.ToString()).FirstOrDefault().Value.Contains((int)botNew.Role!.Value)
                    && FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(key => key.Key == "Machine." + FindFilterParametersBots.OSPlatform.ToString()).FirstOrDefault().Value.Contains((int)botNew.MACHINE.OS_PLATFORM!.Value)
                    && FindBotsInfo.FindFilterBots.FindFiltersInTypeInt.Where(key => key.Key == FindFilterParametersBots.Status.ToString()).FirstOrDefault().Value.Contains((int)botNew.Status))
                {
                    bool goodByKeyword = false;

                    if (!String.IsNullOrEmpty(FindBotsInfo.FindFilterBots.FindSearchKeyword.Value))
                    {
                        switch (FindBotsInfo.FindFilterBots.FindSearchKeyword.Key)
                        {
                            case nameof(FindSearchKeywordParametersBots.Id):
                                if (botNew.ID == FindBotsInfo.FindFilterBots.FindSearchKeyword.Value)
                                {
                                    goodByKeyword = true;
                                }
                                break;
                            case nameof(FindSearchKeywordParametersBots.Ip):
                                if (botNew.IP!.Contains(FindBotsInfo.FindFilterBots.FindSearchKeyword.Value))
                                {
                                    goodByKeyword = true;
                                }
                                break;
                            case ("Machine." + nameof(FindSearchKeywordParametersBots.MachineName)):
                                if (botNew.MACHINE.MACHINE_NAME!.Contains(FindBotsInfo.FindFilterBots.FindSearchKeyword.Value))
                                {
                                    goodByKeyword = true;
                                }
                                break;
                            case ("Machine." + nameof(FindSearchKeywordParametersBots.UserName)):
                                if (botNew.MACHINE.USER_NAME!.Contains(FindBotsInfo.FindFilterBots.FindSearchKeyword.Value))
                                {
                                    goodByKeyword = true;
                                }
                                break;
                            case nameof(FindSearchKeywordParametersBots.RegistrationDateTime):
                                if (botNew.RegistrationDateTime?.Date == FindBotsInfo.FindFilterBots.FindSearchKeywordDateTime?.Date)
                                {
                                    goodByKeyword = true;
                                }
                                break;
                            case ("Machine." + nameof(FindSearchKeywordParametersBots.IdentityKey)):
                                if (botNew.MACHINE.IDENTITY_KEY!.Contains(FindBotsInfo.FindFilterBots.FindSearchKeyword.Value))
                                {
                                    goodByKeyword = true;
                                }
                                break;
                        }
                    }
                    else
                    {
                        goodByKeyword = true;
                    }

                    if (goodByKeyword)
                    {
                        var botToInsert = new BotModel(
                            botNew.ID!,
                            botNew.SERVER_HOST!,
                            botNew.RegistrationDateTime!.Value.ToLocalTime(),
                            botNew.Role!.Value,
                            botNew.IP!,
                            botNew.MACHINE,
                            botNew.Status
                        );

                        if (FindBotsInfo.FindSortBots == FindSortBots.NewFirst)
                        {
                            // remove last element
                            if (AllBotsCount >= SelectedSearchPageCountCombobox.Value && BotsCollection.Count >= SelectedSearchPageCountCombobox.Value)
                            {
                                App.Current.Dispatcher.BeginInvoke(() =>
                                {
                                    BotsCollection.RemoveAt(BotsCollection.Count - 1);
                                });

                                LastElementNumberOfSelectedPage -= 1;
                                AllBotsCount -= 1;

                                if (BotsCollection.Count == 0)
                                    FirstElementNumberOfSelectedPage = 0;
                            }

                            // push to head
                            App.Current.Dispatcher.BeginInvoke(() =>
                            {
                                BotsCollection.Insert(0, botToInsert);
                            });

                            LastElementNumberOfSelectedPage += 1;
                            AllBotsCount += 1;

                            if (BotsCollection.Count > 0)
                                FirstElementNumberOfSelectedPage = 1;
                        }
                        else if (FindBotsInfo.FindSortBots == FindSortBots.OldFirst && AllBotsCount < SelectedSearchPageCountCombobox.Value && BotsCollection.Count < SelectedSearchPageCountCombobox.Value)
                        {
                            // push to end 
                            App.Current.Dispatcher.BeginInvoke(() =>
                            {
                                BotsCollection.Add(botToInsert);
                            });

                            LastElementNumberOfSelectedPage += 1;
                            AllBotsCount += 1;

                            if (BotsCollection.Count > 0)
                                FirstElementNumberOfSelectedPage = 1;
                        }
                    }
                }
            }
            catch { }
        }
        private void BotService_BotsStatusChanged(List<string> botsIds, ClientStatus statusThatChanged)
        {
            try
            {
                if (LoadAndLookBotsActionCanExecute && BotsCollection.Count > 0)
                {
                    foreach (string id in botsIds)
                    {
                        if(botsCollection.Where(bot => bot.ID == id).FirstOrDefault() != null)
                        {
                            App.Current.Dispatcher.BeginInvoke(() =>
                            {
                                botsCollection.Where(bot => bot.ID == id).First().Status = statusThatChanged;
                            });
                        }
                    }
                }
            }
            catch { }
        }
        private void BotService_BotClosed(string closedBotId)
        {
            try
            {
                if (LoadAndLookBotsActionCanExecute && BotsCollection.Count > 0)
                {
                    BotModel? botModelThatClosed = BotsCollection.Where(bot => bot.ID == closedBotId).FirstOrDefault();

                    if (botModelThatClosed != null)
                    {
                        App.Current.Dispatcher.BeginInvoke(() =>
                        {
                            BotsCollection.Remove(botModelThatClosed);
                        });

                        LastElementNumberOfSelectedPage -= 1;
                        AllBotsCount -= 1;

                        if (BotsCollection.Count == 0)
                            FirstElementNumberOfSelectedPage = 0;
                    }
                }
            }
            catch { }
        }
        private void BotService_BotsDeleted(DeleteBots deletedBotModel)
        {
            try
            {
                if (LoadAndLookBotsActionCanExecute && BotsCollection.Count > 0)
                {
                    foreach(var deletedBot in deletedBotModel.Bots)
                    {
                        BotModel? botModelThatClosed = BotsCollection.Where(bot => bot.ID == deletedBot.ID).FirstOrDefault();

                        if (botModelThatClosed != null)
                        {
                            App.Current.Dispatcher.BeginInvoke(() =>
                            {
                                BotsCollection.Remove(botModelThatClosed);
                            });

                            LastElementNumberOfSelectedPage -= 1;
                            AllBotsCount -= 1;
                        }
                    }

                    if (BotsCollection.Count == 0)
                        FirstElementNumberOfSelectedPage = 0;
                }
            }
            catch { }
        }
    }
}
