using CommonModels.Client.Models;
using CommonModels.Client.Models.SearchBotsModels;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Server.Database.Models;
using static CommonModels.Client.Models.SearchBotsModels.SearchBotEnums;
using System.Text.RegularExpressions;
using static CommonModels.ProjectTask.EarningSite.SearchTasksModels.SearchTaskEnums;
using CommonModels.ProjectTask.Platform.SocpublicCom.TaskType.GroupSelectiveTaskWithAuth;
using CommonModels.ProjectTask.Platform.SocpublicCom.TaskType.SelectiveTaskWithAuth;
using CommonModels.ProjectTask.EarningSite.SocpublicCom;
using CommonModels.ProjectTask.EarningSite.SocpublicCom.TaskType.GSelectiveTaskWithAuth.SelectiveTask.WithdrawMoney;
using static CommonModels.ProjectTask.EarningSite.SocpublicCom.TaskType.GSelectiveTaskWithAuth.SelectiveTask.WithdrawMoney.WithdrawMoneyGroupSelectiveTaskWithAuth;

namespace Server.Database.Services
{
    public class EarnSiteTasksService
    {
        private readonly IMongoCollection<dynamic> earnSiteTasksCollection;

        public EarnSiteTasksService(IOptions<ZarplataDatabaseSettings> zarplataDatabaseSettings)
        {
            var mongoClient = new MongoClient(zarplataDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(zarplataDatabaseSettings.Value.DatabaseName);

            earnSiteTasksCollection = mongoDatabase.GetCollection<dynamic>(zarplataDatabaseSettings.Value.EarnSiteTasksCollectionName);
        }

        public async Task<List<dynamic>> GetAsync() =>
            await earnSiteTasksCollection.Find(Builders<dynamic>.Filter.Exists("_id", true)).ToListAsync();

        public async Task<dynamic?> GetOneCreatedAsync() =>
            await earnSiteTasksCollection.Find(Builders<dynamic>.Filter.Eq("Status", CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created)).FirstOrDefaultAsync();

        //public async Task<dynamic?> GetOneCreatedWirthdrawForSlaveAsync()
        //{
        //    List<dynamic> tasks = await earnSiteTasksCollection.Find(Builders<dynamic>.Filter.Eq("Status", CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created)).ToListAsync();
        //    return tasks.Where(t => t.InternalStatus == WithdrawalInternalStatus.Created).FirstOrDefault();
        //}

        public async Task<dynamic?> GetOneCreatedWirthdrawForSlaveAsync()
        {
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! НОВЫЙ СПОСОБ ПИСАТЬ ЗАПРОСЫ В БД С МУЛЬТИ УСЛОВИЯМИ!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            var filterBuilder = Builders<dynamic>.Filter;
            var statusFilter = filterBuilder.Eq("Status", CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created);
            var internalStatusFilter = filterBuilder.Eq("InternalStatus", WithdrawalInternalStatus.Created);
            var filter = statusFilter & internalStatusFilter;

            dynamic? task = await earnSiteTasksCollection.Find(filter).FirstOrDefaultAsync();
            return task;
        }

        public async Task<dynamic?> GetOneForWithdrawByMain(string acc_main_id, string acc_main_login, string acc_main_bot_id)
        {
            List<dynamic> tasks = await earnSiteTasksCollection.Find(Builders<dynamic>.Filter.Eq("InternalStatus", WithdrawalInternalStatus.CompletedSuccessfullBySlave)).ToListAsync();
            return tasks.Where(t => t.DatabaseMainAccountId == acc_main_id && t.DatabaseMainAccountLogin == acc_main_login && t.IdOfTheBotAttachedToThisMainAccount == acc_main_bot_id).FirstOrDefault();
        }

        public async Task<bool> CheckIfNoMoreTasksExistForWithdrawByMain(string acc_main_id, string acc_main_login, string acc_main_bot_id)
        {
            List<dynamic> tasks = await earnSiteTasksCollection.Find(Builders<dynamic>.Filter.Eq("DatabaseMainAccountLogin", acc_main_login)).ToListAsync();
            return tasks.Where(t => t.Status != CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Done).Count() > 0 ? false : true;
        }

        public async Task<List<dynamic>> GetWithFiltersAndSortByCreateDateAsync(
                    Dictionary<string, List<int>> filtersInTypeInt,
                    Dictionary<string, List<string>> filtersInTypeString,
                    KeyValuePair<string, string> searchKeywordFilter,
                    DateTime? searchKeywordDateTime,
                    FindSortEarnSiteTasks sortTasks)
        {
            BsonArray subfiltersCollection = new BsonArray();

            // add IN filters Type Int
            foreach (var f in filtersInTypeInt)
            {
                subfiltersCollection.Add(new BsonDocument(f.Key, new BsonDocument("$in", new BsonArray(f.Value))));
            }

            // add IN filters Type String
            foreach (var f in filtersInTypeString)
            {
                subfiltersCollection.Add(new BsonDocument(f.Key, new BsonDocument("$in", new BsonArray(f.Value))));
            }

            // add search keyword (REGEX) or Datetime filter
            if (!string.IsNullOrEmpty(searchKeywordFilter.Value))
            {
                if (searchKeywordFilter.Key == FindSearchKeywordParametersEarnSiteTasks.DateTimeCreate.ToString() && searchKeywordDateTime.HasValue)
                {
                    subfiltersCollection.Add(new BsonDocument(searchKeywordFilter.Key, new BsonDocument("$gte", searchKeywordDateTime.Value)));
                    subfiltersCollection.Add(new BsonDocument(searchKeywordFilter.Key, new BsonDocument("$lt", searchKeywordDateTime.Value.AddDays(1))));
                }
                else if (searchKeywordFilter.Key == FindSearchKeywordParametersEarnSiteTasks.Id.ToString())
                {
                    subfiltersCollection.Add(new BsonDocument("_id", new BsonDocument("$eq", new BsonObjectId(new ObjectId(searchKeywordFilter.Value)))));
                }
                else
                {
                    subfiltersCollection.Add(new BsonDocument(searchKeywordFilter.Key, new BsonDocument("$regex", new BsonRegularExpression(new Regex(searchKeywordFilter.Value, RegexOptions.IgnoreCase)))));
                }
            }

            // add sort
            string sortField = nameof(FindSearchKeywordParametersEarnSiteTasks.DateTimeCreate);
            int sortValue = -1;
            if (sortTasks == FindSortEarnSiteTasks.NewFirst)
            {
                sortValue = -1;
            }
            else if (sortTasks == FindSortEarnSiteTasks.OldFirst)
            {
                sortValue = 1;
            }

            BsonDocument filter = new BsonDocument("$and", subfiltersCollection);

            return await earnSiteTasksCollection.Find(filter).Sort(new BsonDocument(sortField, sortValue)).ToListAsync();
        }

        public async Task<dynamic?> GetAsync(string id)
        {
            var filter = Builders<dynamic>.Filter.Eq("_id", new ObjectId(id));
            return await earnSiteTasksCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(dynamic task) =>
            await earnSiteTasksCollection.InsertOneAsync(task);

        public async Task CreateAsync(List<GroupSelectiveTaskWithAuth> tasks) =>
            await earnSiteTasksCollection.InsertManyAsync(tasks);

        public async Task CreateAsync(List<WithdrawMoneyGroupSelectiveTaskWithAuth> tasks) =>
            await earnSiteTasksCollection.InsertManyAsync(tasks);

        public async Task CreateAsync(List<SocpublicComAutoregTask> tasks) =>
            await earnSiteTasksCollection.InsertManyAsync(tasks);

        public async Task UpdateAsync(string id, string field, string value)
        {
            var filter = Builders<dynamic>.Filter.Eq("_id", new ObjectId(id));
            await earnSiteTasksCollection.UpdateOneAsync(filter, new BsonDocument("$set", new BsonDocument(field, value)));
        }

        public async Task UpdateAsync(string id, string field, int value)
        {
            var filter = Builders<dynamic>.Filter.Eq("_id", new ObjectId(id));
            await earnSiteTasksCollection.UpdateOneAsync(filter, new BsonDocument("$set", new BsonDocument(field, value)));
        }

        public async Task ReplaceAsync(string id, dynamic task)
        {
            var filter = Builders<dynamic>.Filter.Eq("_id", new ObjectId(id));
            await earnSiteTasksCollection.ReplaceOneAsync(filter, task);
        }

        public async Task<List<dynamic>> GetTasksByExecutorIdAsync(string id) =>
            await earnSiteTasksCollection.Find(Builders<dynamic>.Filter.Eq("ExecutorId", id)).ToListAsync();

        public async Task DeleteAsync(List<string> tasksIdList)
        {
            List<ObjectId> objectIds = new List<ObjectId>();
            foreach(var taskid in tasksIdList)
            {
                objectIds.Add(ObjectId.Parse(taskid));
            }

            var filter = Builders<dynamic>.Filter.In("_id", objectIds);
            await earnSiteTasksCollection.DeleteManyAsync(filter);
        }

        public async Task DeleteAsync(string id)
        {
            var filter = Builders<dynamic>.Filter.Eq("_id", new ObjectId(id));
            await earnSiteTasksCollection.DeleteOneAsync(filter);
        }

        public async Task DeleteAsync()
        {
            var filter = Builders<dynamic>.Filter.Exists("_id", true);
            await earnSiteTasksCollection.DeleteManyAsync(filter);
        }

        public async Task DeleteManyByAccLoginAsync(List<string> logins)
        {
            var filter = Builders<dynamic>.Filter.In("Account.Login", logins);
            await earnSiteTasksCollection.DeleteManyAsync(filter);
        }

        public async Task DeleteTempmethodAsync()
        {
            var filter = Builders<dynamic>.Filter.Eq("Account.EmailIsConfirmed", false);
            await earnSiteTasksCollection.DeleteManyAsync(filter);
        }
    }
}
