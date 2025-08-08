using CommonModels.ProjectTask.EarningSite.SocpublicCom.Models.WithdrawalOfMoneyModels;
using CommonModels.ProjectTask.Platform.SocpublicCom.TaskType.SelectiveTaskWithAuth;
using CommonModels.User.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Server.Database.Models;
using static CommonModels.ProjectTask.EarningSite.SocpublicCom.TaskType.GSelectiveTaskWithAuth.SelectiveTask.WithdrawMoney.WithdrawMoneyGroupSelectiveTaskWithAuth;

namespace Server.Database.Services
{
    public class PlatformInternalAccountTaskService
    {
        private readonly IMongoCollection<PlatformInternalAccountTaskModel> platformInternalAccountTasksCollection;

        public PlatformInternalAccountTaskService(IOptions<ZarplataDatabaseSettings> zarplataDatabaseSettings)
        {
            var mongoClient = new MongoClient(zarplataDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(zarplataDatabaseSettings.Value.DatabaseName);

            platformInternalAccountTasksCollection = mongoDatabase.GetCollection<PlatformInternalAccountTaskModel>(zarplataDatabaseSettings.Value.PlatformInternalAccountTasksCollectionName);
        }

        public async Task<List<PlatformInternalAccountTaskModel>> GetAsync() =>
            await platformInternalAccountTasksCollection.Find(_ => true).ToListAsync();

        public async Task<PlatformInternalAccountTaskModel?> GetAsync(string id) =>
            await platformInternalAccountTasksCollection.Find(task => task.Id == id).FirstOrDefaultAsync();

        public async Task<PlatformInternalAccountTaskModel?> GetBySlaveAccountLoginAsync(string account_login) =>
            await platformInternalAccountTasksCollection.Find(task => task.SlaveAccountWhoCreateTaskLogin == account_login).FirstOrDefaultAsync();

        public async Task<PlatformInternalAccountTaskModel?> GetPriceBySlaveAccountLoginAsync(string account_login) =>
            await platformInternalAccountTasksCollection.Find(task => task.SlaveAccountWhoCreateTaskLogin == account_login).FirstOrDefaultAsync();

        //public async Task<string?> GetPriceBySlaveAccountLoginAsync(string account_login) =>
        //    await platformInternalAccountTasksCollection.Find(task => task.SlaveAccountWhoCreateTaskLogin == account_login).FirstOrDefaultAsync().ContinueWith(t => t.Result?.price_user);

        public async Task CreateAsync(PlatformInternalAccountTaskModel task) =>
            await platformInternalAccountTasksCollection.InsertOneAsync(task);

        public async Task CreateAsync(List<PlatformInternalAccountTaskModel> tasks) =>
            await platformInternalAccountTasksCollection.InsertManyAsync(tasks);

        public async Task UpdateAsync(string id, string field, string value) =>
            await platformInternalAccountTasksCollection.UpdateOneAsync(x => x.Id == id, new BsonDocument("$set", new BsonDocument(field, value)));
        public async Task UpdateBySlaveAccLoginAsync(string login, string field, string value) =>
            await platformInternalAccountTasksCollection.UpdateOneAsync(x => x.SlaveAccountWhoCreateTaskLogin == login, new BsonDocument("$set", new BsonDocument(field, value)));

        public async Task ReplaceAsync(string id, PlatformInternalAccountTaskModel task) =>
            await platformInternalAccountTasksCollection.ReplaceOneAsync(x => x.Id == id, task);

        public async Task DeleteAsync(string id) =>
            await platformInternalAccountTasksCollection.DeleteOneAsync(x => x.Id == id);


        //public async Task<dynamic?> TESTUPDATEASYNC()
        //{
        //    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! НОВЫЙ СПОСОБ ПИСАТЬ ЗАПРОСЫ В БД С МУЛЬТИ УСЛОВИЯМИ!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //    //var filterBuilder = Builders<PlatformInternalAccountTaskModel>.Filter;
        //    //var statusFilter = filterBuilder.Eq("MainAccountWhoExecuteTaskLogin", "dashshendel");
        //    //var internalStatusFilter = filterBuilder.Eq(f => f.InternalNumberId == null);
        //    //var filter = statusFilter & internalStatusFilter;

        //    //return await platformInternalAccountTasksCollection.UpdateManyAsync(f => f.InternalNumberId == null && f.MainAccountWhoExecuteTaskLogin == "dashshendel", new BsonDocument("$set", new BsonDocument("MainAccountWhoExecuteTaskLogin", "ikuc365")));
        //    await platformInternalAccountTasksCollection.UpdateManyAsync(f => f.MainAccountWhoExecuteTaskLogin == "dashshendel", new BsonDocument("$set", new BsonDocument("MainAccountWhoExecuteTaskLogin", "ikuc365")));

        //    await platformInternalAccountTasksCollection.UpdateManyAsync(f => f.ip_filter == "all", new BsonDocument("$set", new BsonDocument("work_filter", "white")));
        //    await platformInternalAccountTasksCollection.UpdateManyAsync(f => f.ip_filter == "all", new BsonDocument("$set", new BsonDocument("family_filter", "no")));
        //    await platformInternalAccountTasksCollection.UpdateManyAsync(f => f.ip_filter == "all", new BsonDocument("$set", new BsonDocument("gender_filter", "male")));
        //    await platformInternalAccountTasksCollection.UpdateManyAsync(f => f.ip_filter == "all", new BsonDocument("$set", new BsonDocument("age_from", "30")));
        //    return await platformInternalAccountTasksCollection.UpdateManyAsync(f => f.ip_filter == "all", new BsonDocument("$set", new BsonDocument("age_to", "32")));
        //}
    }
}
