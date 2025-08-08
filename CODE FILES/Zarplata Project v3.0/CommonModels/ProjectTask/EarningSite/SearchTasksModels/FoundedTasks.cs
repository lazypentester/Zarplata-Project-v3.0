using CommonModels.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.ProjectTask.EarningSite.SearchTasksModels
{
    public class FoundedTasks
    {
        public int SelectedPageNumber { get; set; }
        public List<dynamic> Tasks { get; set; }
        public int AllPagesCount { get; set; }
        public int AllTasksCount { get; set; }
        public int FirstElementNumberOfCurrentPage { get; set; }
        public int LastElementNumberOfCurrentPage { get; set; }
        public string FoundedTasksStatusText { get; set; }
        public bool FoundedTasksStatus { get; set; }

        public FoundedTasks(List<dynamic> tasks, int selectedPageNumber, int allTasksCount, int allPagesCount, int firstElementNumberOfCurrentPage, int lastElementNumberOfCurrentPage, string foundedTasksStatusText, bool foundedTasksStatus)
        {
            Tasks = tasks;
            SelectedPageNumber = selectedPageNumber;
            AllTasksCount = allTasksCount;
            AllPagesCount = allPagesCount;
            FirstElementNumberOfCurrentPage = firstElementNumberOfCurrentPage;
            LastElementNumberOfCurrentPage = lastElementNumberOfCurrentPage;
            FoundedTasksStatusText = foundedTasksStatusText;
            FoundedTasksStatus = foundedTasksStatus;
        }
    }
}
