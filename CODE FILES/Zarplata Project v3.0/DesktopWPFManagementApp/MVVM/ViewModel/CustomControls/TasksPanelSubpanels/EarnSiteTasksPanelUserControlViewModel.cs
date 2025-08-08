using CommonModels.Client.Models.SearchBotsModels;
using DesktopWPFManagementApp.Commands.BotsPanel;
using DesktopWPFManagementApp.Commands;
using DesktopWPFManagementApp.Models;
using DesktopWPFManagementApp.MVVM.ViewModel.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonModels.Client.Client;
using static CommonModels.Client.Models.SearchBotsModels.SearchBotEnums;
using static DesktopWPFManagementApp.MVVM.ViewModel.CustomControls.BotsPanelUserControlViewModel;
using System.Windows;
using CommonModels.Client.Models.SearchBotsModels.FilterModels;
using DesktopWPFManagementApp.Commands.EarnSiteTasksPanel;
using CommonModels.ProjectTask.EarningSite.SearchTasksModels;
using CommonModels.ProjectTask.EarningSite.SearchTasksModels.FilterModels;
using static CommonModels.ProjectTask.EarningSite.SearchTasksModels.SearchTaskEnums;
using static CommonModels.ProjectTask.ProjectTaskEnums;
using TaskStatus = CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus;
using CommonModels.User.Models;
using Newtonsoft.Json;

namespace DesktopWPFManagementApp.MVVM.ViewModel.CustomControls.TasksPanelSubpanels
{
    internal class EarnSiteTasksPanelUserControlViewModel : ViewModelBase
    {
        #region Enums
        public enum CollectionPageButtons
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

        public enum DeleteTasksOneOrFew
        {
            One,
            Few
        }
        #endregion

        #region Properties

        #region Main Properties
        private ObservableCollection<EarnSiteTaskModel> tasksCollection;
        public ObservableCollection<EarnSiteTaskModel> TasksCollection
        {
            get { return tasksCollection; }
            set
            {
                tasksCollection = value;
            }
        }

        private dynamic? currentSelectedTask;
        public dynamic? CurrentSelectedTask
        {
            get { return currentSelectedTask; }
            set
            {
                currentSelectedTask = value;
                OnPropertyChanged();
            }
        }

        private FindTasks findTasksInfo;
        public FindTasks FindTasksInfo
        {
            get { return findTasksInfo; }
            set { findTasksInfo = value; }
        }

        private int markedTasksCount = 0;
        public int MarkedTasksCount
        {
            get { return markedTasksCount; }
            set
            {
                markedTasksCount = value;
                OnPropertyChanged();
            }
        }

        private int allTasksCount = 0;
        public int AllTasksCount
        {
            get { return allTasksCount; }
            set
            {
                allTasksCount = value;
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

        #region Tasks Actions Settings

        private bool loadAndLookTasksActionCanExecute = false;
        public bool LoadAndLookTasksActionCanExecute
        {
            get { return loadAndLookTasksActionCanExecute; }
            set
            {
                loadAndLookTasksActionCanExecute = value;
                OnPropertyChanged();
            }
        }

        // copy task as text action
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

        // check task details action
        private Visibility checkTaskDetailsActionVisibility = Visibility.Collapsed;
        public Visibility CheckTaskDetailsActionVisibility
        {
            get { return checkTaskDetailsActionVisibility; }
            set
            {
                checkTaskDetailsActionVisibility = value;
                OnPropertyChanged();
            }
        }

        private bool checkTaskDetailsActionCanExecute = false;
        public bool CheckTaskDetailsActionCanExecute
        {
            get { return checkTaskDetailsActionCanExecute; }
            set
            {
                checkTaskDetailsActionCanExecute = value;
                OnPropertyChanged();
            }
        }

        // delete task action
        private Visibility deleteTaskActionVisibility = Visibility.Collapsed;
        public Visibility DeleteTaskActionVisibility
        {
            get { return deleteTaskActionVisibility; }
            set
            {
                deleteTaskActionVisibility = value;
                OnPropertyChanged();
            }
        }

        private bool deleteTaskActionCanExecute = false;
        public bool DeleteTaskActionCanExecute
        {
            get { return deleteTaskActionCanExecute; }
            set
            {
                deleteTaskActionCanExecute = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Filter Tasks Popup
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

        private bool filterItemTaskAssignedByBotIsChecked = true;
        public bool FilterItemTaskAssignedByBotIsChecked
        {
            get { return filterItemTaskAssignedByBotIsChecked; }
            set
            {
                filterItemTaskAssignedByBotIsChecked = value;
                OnPropertyChanged();
            }
        }

        private bool filterItemTaskAssignedByManagementIsChecked = true;
        public bool FilterItemTaskAssignedByManagementIsChecked
        {
            get { return filterItemTaskAssignedByManagementIsChecked; }
            set
            {
                filterItemTaskAssignedByManagementIsChecked = value;
                OnPropertyChanged();
            }
        }

        private bool filterItemTaskStatusCreatedIsChecked = true;
        public bool FilterItemTaskStatusCreatedIsChecked
        {
            get { return filterItemTaskStatusCreatedIsChecked; }
            set
            {
                filterItemTaskStatusCreatedIsChecked = value;
                OnPropertyChanged();
            }
        }

        private bool filterItemTaskStatusStartedIsChecked = true;
        public bool FilterItemTaskStatusStartedIsChecked
        {
            get { return filterItemTaskStatusStartedIsChecked; }
            set
            {
                filterItemTaskStatusStartedIsChecked = value;
                OnPropertyChanged();
            }
        }

        private bool filterItemTaskStatusWaitingForCheckAccountHistoryProxyIsChecked = true;
        public bool FilterItemTaskStatusWaitingForCheckAccountHistoryProxyIsChecked
        {
            get { return filterItemTaskStatusWaitingForCheckAccountHistoryProxyIsChecked; }
            set
            {
                filterItemTaskStatusWaitingForCheckAccountHistoryProxyIsChecked = value;
                OnPropertyChanged();
            }
        }

        private bool filterItemTaskStatusWaitingForNewProxyIsChecked = true;
        public bool FilterItemTaskStatusWaitingForNewProxyIsChecked
        {
            get { return filterItemTaskStatusWaitingForNewProxyIsChecked; }
            set
            {
                filterItemTaskStatusWaitingForNewProxyIsChecked = value;
                OnPropertyChanged();
            }
        }

        private bool filterItemTaskStatusWaitingForCaptchaIsChecked = true;
        public bool FilterItemTaskStatusWaitingForCaptchaIsChecked
        {
            get { return filterItemTaskStatusWaitingForCaptchaIsChecked; }
            set
            {
                filterItemTaskStatusWaitingForCaptchaIsChecked = value;
                OnPropertyChanged();
            }
        }

        private bool filterItemTaskStatusExecutingIsChecked = true;
        public bool FilterItemTaskStatusExecutingIsChecked
        {
            get { return filterItemTaskStatusExecutingIsChecked; }
            set
            {
                filterItemTaskStatusExecutingIsChecked = value;
                OnPropertyChanged();
            }
        }

        private bool filterItemTaskStatusDoneIsChecked = true;
        public bool FilterItemTaskStatusDoneIsChecked
        {
            get { return filterItemTaskStatusDoneIsChecked; }
            set
            {
                filterItemTaskStatusDoneIsChecked = value;
                OnPropertyChanged();
            }
        }

        private bool filterItemResultStatusUnknownIsChecked = true;
        public bool FilterItemResultStatusUnknownIsChecked
        {
            get { return filterItemResultStatusUnknownIsChecked; }
            set
            {
                filterItemResultStatusUnknownIsChecked = value;
                OnPropertyChanged();
            }
        }

        private bool filterItemResultStatusSuccessIsChecked = true;
        public bool FilterItemResultStatusSuccessIsChecked
        {
            get { return filterItemResultStatusSuccessIsChecked; }
            set
            {
                filterItemResultStatusSuccessIsChecked = value;
                OnPropertyChanged();
            }
        }

        private bool filterItemResultStatusErrorIsChecked = true;
        public bool FilterItemResultStatusErrorIsChecked
        {
            get { return filterItemResultStatusErrorIsChecked; }
            set
            {
                filterItemResultStatusErrorIsChecked = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Task Info Popup
        private bool isTaskInfoPopupPanelOpen = false;
        public bool IsTaskInfoPopupPanelOpen
        {
            get { return isTaskInfoPopupPanelOpen; }
            set
            {
                isTaskInfoPopupPanelOpen = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Delete Task Popup
        private bool isDeleteTaskPopupPanelOpen = false;
        public bool IsDeleteTaskPopupPanelOpen
        {
            get { return isDeleteTaskPopupPanelOpen; }
            set
            {
                isDeleteTaskPopupPanelOpen = value;
                OnPropertyChanged();
            }
        }

        private int tasksCountToDelete = 0;
        public int TasksCountToDelete
        {
            get { return tasksCountToDelete; }
            set
            {
                tasksCountToDelete = value;
                OnPropertyChanged();
            }
        }

        private DeleteTasksOneOrFew deleteTasksOneOrFewInfo = DeleteTasksOneOrFew.One;
        public DeleteTasksOneOrFew DeleteTasksOneOrFewInfo
        {
            get { return deleteTasksOneOrFewInfo; }
            set { deleteTasksOneOrFewInfo = value; }
        }

        #endregion

        #region DataGrid ContextMenu Actions

        private bool deleteTaskContextMenuActionCanExecute = true;
        public bool DeleteTaskContextMenuActionCanExecute
        {
            get { return deleteTaskContextMenuActionCanExecute; }
            set
            {
                deleteTaskContextMenuActionCanExecute = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Search Task UserControl
        private ObservableCollection<KeyValuePair<string, FindSearchKeywordParametersEarnSiteTasks>> searchUserControlListCombobox;
        public ObservableCollection<KeyValuePair<string, FindSearchKeywordParametersEarnSiteTasks>> SearchUserControlListCombobox
        {
            get { return searchUserControlListCombobox; }
            set
            {
                searchUserControlListCombobox = value;
                OnPropertyChanged();
            }
        }

        private KeyValuePair<string, FindSearchKeywordParametersEarnSiteTasks> selectedItemSearchUserControlCombobox;
        public KeyValuePair<string, FindSearchKeywordParametersEarnSiteTasks> SelectedItemSearchUserControlCombobox
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

        #region Sort Task List
        private ObservableCollection<KeyValuePair<string, FindSortEarnSiteTasks>> searchSortListCombobox;
        public ObservableCollection<KeyValuePair<string, FindSortEarnSiteTasks>> SearchSortCountListCombobox
        {
            get { return searchSortListCombobox; }
            set
            {
                searchSortListCombobox = value;
                OnPropertyChanged();
            }
        }

        private KeyValuePair<string, FindSortEarnSiteTasks> selectedSearchSortCountCombobox;
        public KeyValuePair<string, FindSortEarnSiteTasks> SelectedSearchSortCountCombobox
        {
            get { return selectedSearchSortCountCombobox; }
            set
            {
                selectedSearchSortCountCombobox = value;
                FindTasksInfo.FindSortTasks = value.Value;
                LoadTasksCollection();
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
                FindTasksInfo.ResultsPerPage = selectedSearchPageCountCombobox.Value;
                LoadTasksCollection();
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
        private int currentSelectedPageButton = (int)CollectionPageButtons.FirstPageButton;
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

        #endregion

        #region Commands

        public GetEarnSiteTasksCollectionCommand GetEarnSiteTasksCollectionCommand { get; private set; }

        public RelayCommand NavigationButtonClickedCommand { get; private set; }

        public RelayCommand MarkOneElementCommand { get; private set; }
        public RelayCommand MarkAllElementsCommand { get; private set; }
        public RelayCommand CopyCellValueCommand { get; private set; }
        public RelayCommand CopyRowValueCommand { get; private set; }

        public RelayCommand OpenTaskCollectionFilterPanelCommand { get; private set; }
        public RelayCommand ApplyFilterTaskCollectionFilterPanelCommand { get; private set; }
        public RelayCommand CloseTaskCollectionFilterPanelCommand { get; private set; }

        public RelayCommand SearchKeywordButtonIsPressed { get; private set; }

        public RelayCommand OpenTaskInfoPanelCommand { get; private set; }
        public RelayCommand CloseTaskInfoPanelCommand { get; private set; }

        public DeleteTaskCommand DeleteTaskMainCommand { get; private set; }
        public RelayCommand OpenDeleteOneTaskPanelCommand { get; private set; }
        public RelayCommand OpenDeleteFewTasksPanelCommand { get; private set; }
        public RelayCommand CloseDeleteTaskPanelCommand { get; private set; }

        public CreateAndStartEarnTasksCommand CreateAndStartTasksCommand { get; private set; }
        public CreateAndStartAutoregTasksCommand CreateAndStartAutoregTasksCommand { get; private set; }
        public CreateAndStartWithdrawalOfMoneyTasksCommand CreateAndStartWithdrawalOfMoneyTasksCommand { get; private set; }



        #endregion

        public EarnSiteTasksPanelUserControlViewModel()
        {
            #region Init Properties

            ConfigureUserAction();

            tasksCollection = new ObservableCollection<EarnSiteTaskModel>();
            currentSelectedTask = TasksCollection.FirstOrDefault();

            searchPageCountListCombobox = new ObservableCollection<KeyValuePair<string, int>>()
            {
                new KeyValuePair<string, int>("10 элементов на странице", 10),
                new KeyValuePair<string, int>("50 элементов на странице", 50),
                new KeyValuePair<string, int>("100 элементов на странице", 100)
            };
            selectedSearchPageCountCombobox = searchPageCountListCombobox[0];

            searchSortListCombobox = new ObservableCollection<KeyValuePair<string, FindSortEarnSiteTasks>>()
            {
                new KeyValuePair<string, FindSortEarnSiteTasks>("Сначала новые", FindSortEarnSiteTasks.NewFirst),
                new KeyValuePair<string, FindSortEarnSiteTasks>("Сначала старые", FindSortEarnSiteTasks.OldFirst)
            };
            selectedSearchSortCountCombobox = searchSortListCombobox[0];

            searchUserControlListCombobox = new ObservableCollection<KeyValuePair<string, FindSearchKeywordParametersEarnSiteTasks>>()
            {
                new KeyValuePair<string, FindSearchKeywordParametersEarnSiteTasks>("Id задания", FindSearchKeywordParametersEarnSiteTasks.Id),
                new KeyValuePair<string, FindSearchKeywordParametersEarnSiteTasks>("Создатель", FindSearchKeywordParametersEarnSiteTasks.AuthorId),
                new KeyValuePair<string, FindSearchKeywordParametersEarnSiteTasks>("Исполнитель", FindSearchKeywordParametersEarnSiteTasks.ExecutorId),
                new KeyValuePair<string, FindSearchKeywordParametersEarnSiteTasks>("Дата создания", FindSearchKeywordParametersEarnSiteTasks.DateTimeCreate),
                new KeyValuePair<string, FindSearchKeywordParametersEarnSiteTasks>("Дата старта", FindSearchKeywordParametersEarnSiteTasks.DateTimeStart),
                new KeyValuePair<string, FindSearchKeywordParametersEarnSiteTasks>("Дата завершения", FindSearchKeywordParametersEarnSiteTasks.DateTimeEnd),
                new KeyValuePair<string, FindSearchKeywordParametersEarnSiteTasks>("Текст ошибки", FindSearchKeywordParametersEarnSiteTasks.ErrorMessage),
            };
            selectedItemSearchUserControlCombobox = searchUserControlListCombobox[0];

            findTasksInfo = new FindTasks(
                CurrentSelectedPageButtonValue,
                selectedSearchPageCountCombobox.Value,
                selectedSearchSortCountCombobox.Value,
                new FindFilterTasks()
                );

            FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Add(FindFilterParametersEarnSiteTasks.AssignedBy.ToString(), new List<int>());
            FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Add(FindFilterParametersEarnSiteTasks.Status.ToString(), new List<int>());
            FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Add(FindFilterParametersEarnSiteTasks.ResultStatus.ToString(), new List<int>());

            FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersEarnSiteTasks.AssignedBy.ToString()).First().Value.Add((int)TaskFrom.Bot);
            FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersEarnSiteTasks.AssignedBy.ToString()).First().Value.Add((int)TaskFrom.Management);

            FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersEarnSiteTasks.Status.ToString()).First().Value.Add(((int)TaskStatus.Created));
            FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersEarnSiteTasks.Status.ToString()).First().Value.Add(((int)TaskStatus.Started));
            FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersEarnSiteTasks.Status.ToString()).First().Value.Add(((int)TaskStatus.WaitingForCheckAccountHistoryProxy));
            FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersEarnSiteTasks.Status.ToString()).First().Value.Add(((int)TaskStatus.WaitingForNewProxy));
            FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersEarnSiteTasks.Status.ToString()).First().Value.Add(((int)TaskStatus.WaitingForCaptcha));
            FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersEarnSiteTasks.Status.ToString()).First().Value.Add(((int)TaskStatus.Executing));
            FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersEarnSiteTasks.Status.ToString()).First().Value.Add(((int)TaskStatus.Done));

            FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersEarnSiteTasks.ResultStatus.ToString()).First().Value.Add((int)TaskResultStatus.Unknown);
            FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersEarnSiteTasks.ResultStatus.ToString()).First().Value.Add((int)TaskResultStatus.Success);
            FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersEarnSiteTasks.ResultStatus.ToString()).First().Value.Add((int)TaskResultStatus.Error);


            //delete
            //TasksCollection.Add(new EarnSiteTaskModel(
            //    id: "GFASGDSGDSGDSFGSERGERSGSEG",
            //    authorId: "DSHFDSIVF43VWH78TETVEOTV",
            //    executorId: "732COHR4QVRWNTVHEWIPTV4",
            //    type: CommonModels.ProjectTask.ProjectTaskEnums.TaskType.EarningSiteWork,
            //    assignedBy: CommonModels.ProjectTask.ProjectTaskEnums.TaskFrom.Management,
            //    status: CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Executing,
            //    queuePosition: 0,
            //    createDateTime: DateTime.UtcNow,
            //    startDateTime: DateTime.UtcNow,
            //    endDateTime: DateTime.UtcNow,
            //    resultStatus: CommonModels.ProjectTask.ProjectTaskEnums.TaskResultStatus.Success,
            //    errorStatus: CommonModels.ProjectTask.ProjectTaskEnums.TaskErrorStatus.CaptchaError,
            //    errorMessage: "An error",
            //    actionAfterFinish: CommonModels.ProjectTask.ProjectTaskEnums.TaskActionAfterFinish.Renew
            //    ));
            //TasksCollection.Add(new EarnSiteTaskModel(
            //    id: "GFASGDSGDSGDSFGSERGERSGSEG",
            //    authorId: "DSHFDSIVF43VWH78TETVEOTV",
            //    executorId: "732COHR4QVRWNTVHEWIPTV4",
            //    type: CommonModels.ProjectTask.ProjectTaskEnums.TaskType.EarningSiteWork,
            //    assignedBy: CommonModels.ProjectTask.ProjectTaskEnums.TaskFrom.Bot,
            //    status: CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created,
            //    queuePosition: 1,
            //    createDateTime: DateTime.UtcNow,
            //    startDateTime: DateTime.UtcNow,
            //    endDateTime: null,
            //    resultStatus: CommonModels.ProjectTask.ProjectTaskEnums.TaskResultStatus.Error,
            //    errorStatus: CommonModels.ProjectTask.ProjectTaskEnums.TaskErrorStatus.AnotherError,
            //    errorMessage: "An error",
            //    actionAfterFinish: CommonModels.ProjectTask.ProjectTaskEnums.TaskActionAfterFinish.Remove
            //    ));
            #endregion

            #region Init Commands

            GetEarnSiteTasksCollectionCommand = new GetEarnSiteTasksCollectionCommand(this);

            NavigationButtonClickedCommand = new RelayCommand(execute =>
            {
                try
                {
                    string? buttonName = execute as string;
                    if (buttonName != null)
                    {
                        switch (buttonName)
                        {
                            case "NavigationPreviousButton":
                                CurrentSelectedPageButtonValue = PreviousPageButtonValue;
                                FindTasksInfo.SelectedPageNumber = CurrentSelectedPageButtonValue;
                                LoadTasksCollection();
                                break;
                            case "NavigationFirstButton":
                                CurrentSelectedPageButtonValue = FirstPageButtonValue;
                                FindTasksInfo.SelectedPageNumber = CurrentSelectedPageButtonValue;
                                LoadTasksCollection();
                                break;
                            case "NavigationSecondButton":
                                CurrentSelectedPageButtonValue = SecondPageButtonValue;
                                FindTasksInfo.SelectedPageNumber = CurrentSelectedPageButtonValue;
                                LoadTasksCollection();
                                break;
                            case "NavigationThirdButton":
                                CurrentSelectedPageButtonValue = ThirdPageButtonValue;
                                FindTasksInfo.SelectedPageNumber = CurrentSelectedPageButtonValue;
                                LoadTasksCollection();
                                break;
                            case "NavigationFourthButton":
                                CurrentSelectedPageButtonValue = FourthPageButtonValue;
                                FindTasksInfo.SelectedPageNumber = CurrentSelectedPageButtonValue;
                                LoadTasksCollection();
                                break;
                            case "NavigationFifthButton":
                                CurrentSelectedPageButtonValue = FifthPageButtonValue;
                                FindTasksInfo.SelectedPageNumber = CurrentSelectedPageButtonValue;
                                LoadTasksCollection();
                                break;
                            case "NavigationSixthButton":
                                CurrentSelectedPageButtonValue = SixthPageButtonValue;
                                FindTasksInfo.SelectedPageNumber = CurrentSelectedPageButtonValue;
                                LoadTasksCollection();
                                break;
                            case "NavigationNextButton":
                                CurrentSelectedPageButtonValue = NextPageButtonValue;
                                FindTasksInfo.SelectedPageNumber = CurrentSelectedPageButtonValue;
                                LoadTasksCollection();
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

            MarkOneElementCommand = new RelayCommand(execute =>
            {
                bool? booleanParam = (execute as bool?);
                if (booleanParam.HasValue)
                {
                    bool CheckOneCheckBoxIsChecked = booleanParam.Value;

                    if (CheckOneCheckBoxIsChecked)
                    {
                        MarkedTasksCount++;
                    }
                    else if (!CheckOneCheckBoxIsChecked)
                    {
                        MarkedTasksCount--;
                    }
                }
            },
            canExecute =>
            {
                return canExecute != null;
            });
            MarkAllElementsCommand = new RelayCommand(execute =>
            {
                bool? booleanParam = (execute as bool?);
                if (booleanParam.HasValue)
                {
                    bool CheckAllCheckBoxIsChecked = booleanParam.Value;

                    if (CheckAllCheckBoxIsChecked)
                    {
                        foreach (var task in TasksCollection) task.IsMarked = true;
                        MarkedTasksCount = TasksCollection.Count;
                    }
                    else if (!CheckAllCheckBoxIsChecked)
                    {
                        foreach (var task in TasksCollection) task.IsMarked = false;
                        MarkedTasksCount = 0;
                    }
                }
            },
            canExecute =>
            {
                return canExecute != null && TasksCollection.Count > 0;
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
                    EarnSiteTaskModel? model = execute as EarnSiteTaskModel;
                    CurrentSelectedTask = model;
                    StringBuilder stringBuilder = new StringBuilder();

                    stringBuilder.Append(nameof(model.Id) + ":");
                    stringBuilder.AppendLine(model!.Id);

                    stringBuilder.Append(nameof(model.AuthorId) + ":");
                    stringBuilder.AppendLine(model!.AuthorId);

                    stringBuilder.Append(nameof(model.ExecutorId) + ":");
                    stringBuilder.AppendLine(model!.ExecutorId);

                    stringBuilder.Append(nameof(model.Type) + ":");
                    stringBuilder.AppendLine(model.Type.ToString());

                    stringBuilder.Append(nameof(model.AssignedBy) + ":");
                    stringBuilder.AppendLine(model.AssignedBy.ToString());

                    stringBuilder.Append(nameof(model.Status) + ":");
                    stringBuilder.AppendLine(model.Status.ToString());

                    stringBuilder.Append(nameof(model.QueuePosition) + ":");
                    stringBuilder.AppendLine(model.QueuePosition.ToString());

                    stringBuilder.Append(nameof(model.CreateDateTime) + ":");
                    stringBuilder.AppendLine(model!.CreateDateTimePreviewFormat);

                    stringBuilder.Append(nameof(model.StartDateTime) + ":");
                    stringBuilder.AppendLine(model!.StartDateTimePreviewFormat);

                    stringBuilder.Append(nameof(model.EndDateTime) + ":");
                    stringBuilder.AppendLine(model!.EndDateTimePreviewFormat);

                    stringBuilder.Append(nameof(model.ResultStatus) + ":");
                    stringBuilder.AppendLine(model.ResultStatus.ToString());

                    stringBuilder.Append(nameof(model.ErrorStatus) + ":");
                    stringBuilder.AppendLine(model.ErrorStatus.ToString());

                    stringBuilder.Append(nameof(model.ErrorMessage) + ":");
                    stringBuilder.AppendLine(model.ErrorMessage);

                    stringBuilder.Append(nameof(model.ActionAfterFinish) + ":");
                    stringBuilder.AppendLine(model.ActionAfterFinish.ToString());

                    Clipboard.SetDataObject(stringBuilder.ToString());
                }
                catch { }
            },
            canExecute =>
            {
                return canExecute != null && CopyActionCanExecute;
            });

            OpenTaskCollectionFilterPanelCommand = new RelayCommand(execute =>
            {
                IsFilterPopupPanelOpen = true;

                var checkAssignedByList = FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersEarnSiteTasks.AssignedBy.ToString()).FirstOrDefault().Value;
                if (checkAssignedByList != null)
                {
                    if (checkAssignedByList.Contains(((int)TaskFrom.Bot)))
                        FilterItemTaskAssignedByBotIsChecked = true;
                    else
                        FilterItemTaskAssignedByBotIsChecked = false;

                    if (checkAssignedByList.Contains(((int)TaskFrom.Management)))
                        FilterItemTaskAssignedByManagementIsChecked = true;
                    else
                        FilterItemTaskAssignedByManagementIsChecked = false;
                }

                var checkStatusesList = FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersEarnSiteTasks.Status.ToString()).FirstOrDefault().Value;
                if (checkStatusesList != null)
                {
                    if (checkStatusesList.Contains(((int)TaskStatus.Created)))
                        FilterItemTaskStatusCreatedIsChecked = true;
                    else
                        FilterItemTaskStatusCreatedIsChecked = false;

                    if (checkStatusesList.Contains(((int)TaskStatus.Started)))
                        FilterItemTaskStatusStartedIsChecked = true;
                    else
                        FilterItemTaskStatusStartedIsChecked = false;

                    if (checkStatusesList.Contains(((int)TaskStatus.WaitingForCheckAccountHistoryProxy)))
                        FilterItemTaskStatusWaitingForCheckAccountHistoryProxyIsChecked = true;
                    else
                        FilterItemTaskStatusWaitingForCheckAccountHistoryProxyIsChecked = false;

                    if (checkStatusesList.Contains(((int)TaskStatus.WaitingForNewProxy)))
                        FilterItemTaskStatusWaitingForNewProxyIsChecked = true;
                    else
                        FilterItemTaskStatusWaitingForNewProxyIsChecked = false;

                    if (checkStatusesList.Contains(((int)TaskStatus.WaitingForCaptcha)))
                        FilterItemTaskStatusWaitingForCaptchaIsChecked = true;
                    else
                        FilterItemTaskStatusWaitingForCaptchaIsChecked = false;

                    if (checkStatusesList.Contains(((int)TaskStatus.Executing)))
                        FilterItemTaskStatusExecutingIsChecked = true;
                    else
                        FilterItemTaskStatusExecutingIsChecked = false;

                    if (checkStatusesList.Contains(((int)TaskStatus.Done)))
                        FilterItemTaskStatusDoneIsChecked = true;
                    else
                        FilterItemTaskStatusDoneIsChecked = false;
                }

                var checkResultStatusesList = FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersEarnSiteTasks.ResultStatus.ToString()).FirstOrDefault().Value;
                if (checkResultStatusesList != null)
                {
                    if (checkResultStatusesList.Contains(((int)TaskResultStatus.Unknown)))
                        FilterItemResultStatusUnknownIsChecked = true;
                    else
                        FilterItemResultStatusUnknownIsChecked = false;

                    if (checkResultStatusesList.Contains(((int)TaskResultStatus.Success)))
                        FilterItemResultStatusSuccessIsChecked = true;
                    else
                        FilterItemResultStatusSuccessIsChecked = false;

                    if (checkResultStatusesList.Contains(((int)TaskResultStatus.Error)))
                        FilterItemResultStatusErrorIsChecked = true;
                    else
                        FilterItemResultStatusErrorIsChecked = false;
                }
            },
            canExecute =>
            {
                //return (LoadingDataStatus != LoadDataStatusEnum.DataIsLoading.ToString() && !IsFilterPopupPanelOpen && !IsTaskInfoPopupPanelOpen);
                return (LoadingDataStatus != LoadDataStatusEnum.DataIsLoading.ToString() && !IsFilterPopupPanelOpen);
            });
            ApplyFilterTaskCollectionFilterPanelCommand = new RelayCommand(execute =>
            {
                // clear all filters
                FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersEarnSiteTasks.AssignedBy.ToString()).FirstOrDefault().Value.Clear();
                FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersEarnSiteTasks.Status.ToString()).FirstOrDefault().Value.Clear();
                FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersEarnSiteTasks.ResultStatus.ToString()).FirstOrDefault().Value.Clear();

                // add or remove AssignedBy filters
                if (FilterItemTaskAssignedByBotIsChecked)
                    FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersEarnSiteTasks.AssignedBy.ToString()).FirstOrDefault().Value.Add((int)TaskFrom.Bot);

                if (FilterItemTaskAssignedByManagementIsChecked)
                    FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersEarnSiteTasks.AssignedBy.ToString()).FirstOrDefault().Value.Add((int)TaskFrom.Management);

                // add or remove Status filters
                if (FilterItemTaskStatusCreatedIsChecked)
                    FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersEarnSiteTasks.Status.ToString()).FirstOrDefault().Value.Add((int)TaskStatus.Created);

                if (FilterItemTaskStatusStartedIsChecked)
                    FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersEarnSiteTasks.Status.ToString()).FirstOrDefault().Value.Add((int)TaskStatus.Started);

                if (FilterItemTaskStatusWaitingForCheckAccountHistoryProxyIsChecked)
                    FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersEarnSiteTasks.Status.ToString()).FirstOrDefault().Value.Add((int)TaskStatus.WaitingForCheckAccountHistoryProxy);

                if (FilterItemTaskStatusWaitingForNewProxyIsChecked)
                    FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersEarnSiteTasks.Status.ToString()).FirstOrDefault().Value.Add((int)TaskStatus.WaitingForNewProxy);

                if (FilterItemTaskStatusWaitingForCaptchaIsChecked)
                    FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersEarnSiteTasks.Status.ToString()).FirstOrDefault().Value.Add((int)TaskStatus.WaitingForCaptcha);

                if (FilterItemTaskStatusExecutingIsChecked)
                    FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersEarnSiteTasks.Status.ToString()).FirstOrDefault().Value.Add((int)TaskStatus.Executing);

                if (FilterItemTaskStatusDoneIsChecked)
                    FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersEarnSiteTasks.Status.ToString()).FirstOrDefault().Value.Add((int)TaskStatus.Done);

                // add or remove ResultStatus filters
                if (FilterItemResultStatusUnknownIsChecked)
                    FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersEarnSiteTasks.ResultStatus.ToString()).FirstOrDefault().Value.Add((int)TaskResultStatus.Unknown);

                if (FilterItemResultStatusSuccessIsChecked)
                    FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersEarnSiteTasks.ResultStatus.ToString()).FirstOrDefault().Value.Add((int)TaskResultStatus.Success);

                if (FilterItemResultStatusErrorIsChecked)
                    FindTasksInfo.FindFilterTasks.FindFiltersInTypeInt.Where(e => e.Key == FindFilterParametersEarnSiteTasks.ResultStatus.ToString()).FirstOrDefault().Value.Add((int)TaskResultStatus.Error);

                IsFilterPopupPanelOpen = false;

                LoadTasksCollection();
            },
            canExecute =>
            {
                //return (LoadingDataStatus != LoadDataStatusEnum.DataIsLoading.ToString() && IsFilterPopupPanelOpen && !IsBotInfoPopupPanelOpen);
                return (LoadingDataStatus != LoadDataStatusEnum.DataIsLoading.ToString() && IsFilterPopupPanelOpen);
            });
            CloseTaskCollectionFilterPanelCommand = new RelayCommand(execute =>
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
                    case FindSearchKeywordParametersEarnSiteTasks.Id:
                        FindTasksInfo.FindFilterTasks.FindSearchKeyword = new KeyValuePair<string, string>(FindSearchKeywordParametersEarnSiteTasks.Id.ToString(), SearchUserControlKeywordText);
                        LoadTasksCollection();
                        break;
                    case FindSearchKeywordParametersEarnSiteTasks.AuthorId:
                        FindTasksInfo.FindFilterTasks.FindSearchKeyword = new KeyValuePair<string, string>(FindSearchKeywordParametersEarnSiteTasks.AuthorId.ToString(), SearchUserControlKeywordText);
                        LoadTasksCollection();
                        break;
                    case FindSearchKeywordParametersEarnSiteTasks.ExecutorId:
                        FindTasksInfo.FindFilterTasks.FindSearchKeyword = new KeyValuePair<string, string>(FindSearchKeywordParametersEarnSiteTasks.ExecutorId.ToString(), SearchUserControlKeywordText);
                        LoadTasksCollection();
                        break;
                    case FindSearchKeywordParametersEarnSiteTasks.DateTimeCreate:
                        try
                        {
                            if (SearchUserControlKeywordText.ToLower().Contains("дата") && SearchUserControlKeywordText.ToLower().Contains("время"))
                            {
                                string[] date_and_time = SearchUserControlKeywordText.Split(' ');
                                string date_str = date_and_time[0].Replace("дата=", "");
                                string time_str = date_and_time[1].Replace("время=", "");
                                DateTime datetime;
                                TimeOnly time;
                                if (DateTime.TryParse(date_str, out datetime))
                                {
                                    if (TimeOnly.TryParse(time_str, out time))
                                    {
                                        datetime = datetime.AddHours(time.Hour);
                                        datetime = datetime.AddMinutes(time.Minute);
                                        datetime = datetime.AddSeconds(time.Second);

                                        FindTasksInfo.FindFilterTasks.FindSearchKeyword = new KeyValuePair<string, string>(FindSearchKeywordParametersEarnSiteTasks.DateTimeCreate.ToString(), SearchUserControlKeywordText);
                                        FindTasksInfo.FindFilterTasks.FindSearchKeywordDateTime = datetime;
                                        LoadTasksCollection();
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
                                    FindTasksInfo.FindFilterTasks.FindSearchKeyword = new KeyValuePair<string, string>(FindSearchKeywordParametersEarnSiteTasks.DateTimeCreate.ToString(), SearchUserControlKeywordText);
                                    FindTasksInfo.FindFilterTasks.FindSearchKeywordDateTime = datetime;
                                    LoadTasksCollection();
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

                                        FindTasksInfo.FindFilterTasks.FindSearchKeyword = new KeyValuePair<string, string>(FindSearchKeywordParametersEarnSiteTasks.DateTimeCreate.ToString(), SearchUserControlKeywordText);
                                        FindTasksInfo.FindFilterTasks.FindSearchKeywordDateTime = datetime;
                                        LoadTasksCollection();
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
                                    FindTasksInfo.FindFilterTasks.FindSearchKeyword = new KeyValuePair<string, string>(FindSearchKeywordParametersEarnSiteTasks.DateTimeCreate.ToString(), SearchUserControlKeywordText);
                                    FindTasksInfo.FindFilterTasks.FindSearchKeywordDateTime = datetime;
                                    LoadTasksCollection();
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
                    case FindSearchKeywordParametersEarnSiteTasks.ErrorMessage:
                        FindTasksInfo.FindFilterTasks.FindSearchKeyword = new KeyValuePair<string, string>(FindSearchKeywordParametersEarnSiteTasks.ErrorMessage.ToString(), SearchUserControlKeywordText);
                        LoadTasksCollection();
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

            OpenTaskInfoPanelCommand = new RelayCommand(execute =>
            {
                CurrentSelectedTask = execute;
                IsTaskInfoPopupPanelOpen = true;
            },
            canExecute =>
            {
                return (CheckTaskDetailsActionCanExecute && LoadingDataStatus != LoadDataStatusEnum.DataIsLoading.ToString() && canExecute != null && !IsFilterPopupPanelOpen && !IsTaskInfoPopupPanelOpen);
            });
            CloseTaskInfoPanelCommand = new RelayCommand(execute =>
            {
                IsTaskInfoPopupPanelOpen = false;
            },
            canExecute =>
            {
                return IsTaskInfoPopupPanelOpen;
            });


            DeleteTaskMainCommand = new DeleteTaskCommand(this);
            OpenDeleteOneTaskPanelCommand = new RelayCommand(execute =>
            {
                CurrentSelectedTask = execute;
                TasksCountToDelete = 1;
                DeleteTasksOneOrFewInfo = DeleteTasksOneOrFew.One;
                IsDeleteTaskPopupPanelOpen = true;
            },
            canExecute =>
            {
                return (LoadingDataStatus != LoadDataStatusEnum.DataIsLoading.ToString() && DeleteTaskActionCanExecute && canExecute != null && !IsDeleteTaskPopupPanelOpen && !IsFilterPopupPanelOpen && !IsTaskInfoPopupPanelOpen);
            });
            OpenDeleteFewTasksPanelCommand = new RelayCommand(execute =>
            {
                TasksCountToDelete = MarkedTasksCount;
                DeleteTasksOneOrFewInfo = DeleteTasksOneOrFew.Few;
                IsDeleteTaskPopupPanelOpen = true;
            },
            canExecute =>
            {
                return (LoadingDataStatus != LoadDataStatusEnum.DataIsLoading.ToString() && DeleteTaskActionCanExecute && DeleteTaskContextMenuActionCanExecute && !IsDeleteTaskPopupPanelOpen && !IsFilterPopupPanelOpen && !IsTaskInfoPopupPanelOpen);
            });
            CloseDeleteTaskPanelCommand = new RelayCommand(execute =>
            {
                IsDeleteTaskPopupPanelOpen = false;
            },
            canExecute =>
            {
                return IsDeleteTaskPopupPanelOpen;
            });


            CreateAndStartTasksCommand = new CreateAndStartEarnTasksCommand(this);
            CreateAndStartAutoregTasksCommand = new CreateAndStartAutoregTasksCommand(this);
            CreateAndStartWithdrawalOfMoneyTasksCommand = new CreateAndStartWithdrawalOfMoneyTasksCommand(this);

            #endregion

            #region init event handlers

            GetEarnSiteTasksCollectionCommand.ConnectionError += GetTaskCollectionCommand_ConnectionError;
            GetEarnSiteTasksCollectionCommand.GetTaskCollectionFailed += GetTaskCollectionCommand_GetTaskCollectionFailed;
            GetEarnSiteTasksCollectionCommand.GetTaskCollectionSuccessfully += GetTaskCollectionCommand_GetTaskCollectionSuccessfully;

            #endregion
        }

        private void GetTaskCollectionCommand_GetTaskCollectionSuccessfully(FoundedTasks tasks)
        {
            AllTasksCount = tasks.AllTasksCount;
            FirstElementNumberOfSelectedPage = tasks.FirstElementNumberOfCurrentPage;
            LastElementNumberOfSelectedPage = tasks.LastElementNumberOfCurrentPage;

            TasksCollection.Clear();
            MarkedTasksCount = 0;

            foreach (object task_ in tasks.Tasks)
            {
                var task = JsonConvert.DeserializeObject<dynamic>(task_.ToString()!);

                string id_ = task!.id;
                string authorId_ = task!.authorId;
                string executorId_ = task!.executorId;
                TaskType type_ = task!.type;
                TaskFrom assignedBy_ = task!.assignedBy;
                TaskStatus status_ = task!.status;
                int queuePosition_ = task!.queuePosition;
                DateTime createDateTime_ = task!.dateTimeCreate;
                DateTime? startDateTime_ = task!.dateTimeStart;
                DateTime? endDateTime_ = task!.dateTimeEnd;
                TaskResultStatus resultStatus_ = task!.resultStatus;
                TaskErrorStatus? errorStatus_ = task!.errorStatus;
                string errorMessage_ = task!.errorMessage;
                TaskActionAfterFinish actionAfterFinish_ = task!.actionAfterFinish;

                TasksCollection.Add(new EarnSiteTaskModel(
                    id_,
                    authorId_,
                    executorId_,
                    type_,
                    assignedBy_,
                    status_,
                    queuePosition_,
                    createDateTime_,
                    startDateTime_,
                    endDateTime_,
                    resultStatus_,
                    errorStatus_,
                    errorMessage_,
                    actionAfterFinish_
                    ));
            }

            ConfigureNavigationButtonElements(tasks.SelectedPageNumber, tasks.AllPagesCount);
        }

        private void GetTaskCollectionCommand_GetTaskCollectionFailed(string obj)
        {
            throw new NotImplementedException();
        }

        private void GetTaskCollectionCommand_ConnectionError()
        {
            //throw new NotImplementedException();
        }

        public void LoadTasksCollection()
        {
            if (GetEarnSiteTasksCollectionCommand.CanExecute(null))
            {
                GetEarnSiteTasksCollectionCommand.Execute(FindTasksInfo);
            }
        }

        public void ConfigureUserAction()
        {
            if (Services.ServiceStorage.UserService.CanUserExecuteAction(UserActions.CanLookEarnSiteTasks))
            {
                LoadAndLookTasksActionCanExecute = true;
            }
            else
            {
                LoadAndLookTasksActionCanExecute = false;
            }

            if (Services.ServiceStorage.UserService.CanUserExecuteAction(UserActions.CanCopyEarnSiteTasks))
            {
                CopyActionVisibility = Visibility.Visible;
                CopyActionCanExecute = true;
            }
            else
            {
                CopyActionVisibility = Visibility.Collapsed;
                CopyActionCanExecute = false;
            }

            if (Services.ServiceStorage.UserService.CanUserExecuteAction(UserActions.CanCheckDetailsEarnSiteTasks))
            {
                CheckTaskDetailsActionVisibility = Visibility.Visible;
                CheckTaskDetailsActionCanExecute = true;
            }
            else
            {
                CheckTaskDetailsActionVisibility = Visibility.Collapsed;
                CheckTaskDetailsActionCanExecute = false;
            }

            if (Services.ServiceStorage.UserService.CanUserExecuteAction(UserActions.CanDeleteEarnSiteTasks))
            {
                DeleteTaskActionVisibility = Visibility.Visible;
                DeleteTaskActionCanExecute = true;
            }
            else
            {
                DeleteTaskActionVisibility = Visibility.Collapsed;
                DeleteTaskActionCanExecute = false;
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
                    if (selectedPage == 1)
                    {
                        CurrentSelectedPageButton = 1;
                        NextPageButtonIsEnabled = true;
                        PreviousPageButtonIsEnabled = false;
                    }
                    else if (selectedPage == 2)
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
                        if (selectedPage == 1)
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
    }
}
