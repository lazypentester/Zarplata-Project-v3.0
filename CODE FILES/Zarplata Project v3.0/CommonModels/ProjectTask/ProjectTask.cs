
using TaskStatus = CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus;
using static CommonModels.ProjectTask.ProjectTaskEnums;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace CommonModels.ProjectTask
{
    public abstract class ProjectTask
    {
        [BsonId]
        [BsonElement("Id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } = null;
        public string AuthorId { get; set; }
        public string? ExecutorId { get; set; } = null;
        public TaskType Type { get; set; }
        public TaskFrom AssignedBy { get; set; }
        public TaskStatus Status { get; set; }
        public int QueuePosition { get; set; } = 0;
        public DateTime DateTimeCreate { get; set; }
        public DateTime? DateTimeStart { get; set; } = null;
        public DateTime? DateTimeEnd { get; set; } = null;
        public TaskResultStatus? ResultStatus { get; set; } = TaskResultStatus.Unknown;
        public TaskErrorStatus? ErrorStatus { get; set; } = null;
        public string? ErrorMessage { get; set; } = null;
        public TaskActionAfterFinish ActionAfterFinish { get; set; }
        public int? ErrorTaskCount { get; set; } = null;

        public ProjectTask(string authorId, TaskFrom assignedBy, TaskStatus status, TaskActionAfterFinish actionAfterFinish)
        {
            AuthorId = authorId;
            AssignedBy = assignedBy;
            Status = status;

            DateTimeCreate = DateTime.UtcNow;
            ActionAfterFinish = actionAfterFinish;
        }

        public ProjectTask(string authorId, string executorId, TaskFrom assignedBy, TaskStatus status, TaskActionAfterFinish actionAfterFinish)
        {
            AuthorId = authorId;
            ExecutorId = executorId;
            AssignedBy = assignedBy;
            Status = status;

            DateTimeCreate = DateTime.UtcNow;
            ActionAfterFinish = actionAfterFinish;
        }
    }
}
