using DesktopWPFManagementApp.MVVM.ViewModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonModels.Client.Client;
using static CommonModels.ProjectTask.ProjectTaskEnums;
using TaskStatus = CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus;

namespace DesktopWPFManagementApp.Models
{
    public class EarnSiteTaskModel : ViewModelBase
    {
        private bool isMarked = false;
        public bool IsMarked
        {
            get { return isMarked; }
            set
            {
                isMarked = value;
                OnPropertyChanged();
            }
        }
        public string Id { get; set; }
        public string AuthorId { get; set; }
        public string ExecutorId { get; set; }
        public TaskType Type { get; set; }
        public TaskFrom AssignedBy { get; set; }
        private TaskStatus status;
        public TaskStatus Status
        {
            get { return status; }
            set
            {
                status = value;
                OnPropertyChanged();
            }
        }
        public int QueuePosition { get; set; }
        public DateTime CreateDateTime { get; set; }
        public string CreateDateTimePreviewFormat { get; set; }
        public DateTime? StartDateTime { get; set; }
        public string StartDateTimePreviewFormat { get; set; }
        public DateTime? EndDateTime { get; set; }
        public string EndDateTimePreviewFormat { get; set; }
        private TaskResultStatus resultStatus;
        public TaskResultStatus ResultStatus
        {
            get { return resultStatus; }
            set
            {
                resultStatus = value;
                OnPropertyChanged();
            }
        }
        public TaskErrorStatus? ErrorStatus { get; set; }
        public string ErrorMessage { get; set; }
        public TaskActionAfterFinish ActionAfterFinish { get; set; }

        public EarnSiteTaskModel(string id, string authorId, string executorId, TaskType type, TaskFrom assignedBy, TaskStatus status, int queuePosition, DateTime createDateTime, DateTime? startDateTime, DateTime? endDateTime, TaskResultStatus resultStatus, TaskErrorStatus? errorStatus, string errorMessage, TaskActionAfterFinish actionAfterFinish)
        {
            Id = id;
            AuthorId = authorId;
            ExecutorId = executorId;
            Type = type;
            AssignedBy = assignedBy;
            Status = status;
            this.QueuePosition = queuePosition;
            CreateDateTime = createDateTime;
            CreateDateTimePreviewFormat = CreateDateTime.ToShortDateString() + " - " + CreateDateTime.ToShortTimeString();
            StartDateTime = startDateTime;
            if (StartDateTime == null)
            {
                StartDateTimePreviewFormat = "-";
            }
            else
            {
                StartDateTimePreviewFormat = StartDateTime?.ToShortDateString() + " - " + StartDateTime?.ToShortTimeString();
            } 
            EndDateTime = endDateTime;
            if(EndDateTime == null)
            {
                EndDateTimePreviewFormat = "-";
            }
            else
            {
                EndDateTimePreviewFormat = EndDateTime?.ToShortDateString() + " - " + EndDateTime?.ToShortTimeString();
            }
            ResultStatus = resultStatus;
            ErrorStatus = errorStatus;
            ErrorMessage = errorMessage;
            ActionAfterFinish = actionAfterFinish;
        }
    }
}
