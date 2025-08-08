using CommonModels.ProjectTask;
using CommonModels.ProjectTask.Platform.SocpublicCom.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Server.Database.Models;
using System.Threading.Tasks;

namespace Server.Database.Services
{
    public class RunTasksSettingsService
    {
        private readonly IMongoCollection<RunTasksSettings> runTasksSettingsCollection;

        public RunTasksSettingsService(IOptions<ZarplataDatabaseSettings> zarplataDatabaseSettings)
        {
            var mongoClient = new MongoClient(zarplataDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(zarplataDatabaseSettings.Value.DatabaseName);

            runTasksSettingsCollection = mongoDatabase.GetCollection<RunTasksSettings>(zarplataDatabaseSettings.Value.RunTasksSettingsCollectionName);
        }

        public async Task<List<RunTasksSettings>> GetAsync() =>
            await runTasksSettingsCollection.Find(_ => true).ToListAsync();

        public async Task<RunTasksSettings?> GetAsync(string id) =>
            await runTasksSettingsCollection.Find(account => account.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(RunTasksSettings setting) =>
            await runTasksSettingsCollection.InsertOneAsync(setting);

        public async Task CreateAsync(List<RunTasksSettings> settings) =>
            await runTasksSettingsCollection.InsertManyAsync(settings);

        public async Task UpdateAsync(string id, string field, string value) =>
            await runTasksSettingsCollection.UpdateOneAsync(x => x.Id == id, new BsonDocument("$set", new BsonDocument(field, value)));

        public async Task ReplaceAsync(string id, RunTasksSettings setting)
        {
            //var filter = Builders<RunTasksSettings>.Filter.Eq("_id", new ObjectId(id));
            await runTasksSettingsCollection.ReplaceOneAsync(x => x.Id == id, setting);
            //await earnSiteTasksCollection.ReplaceOneAsync(filter, task);
        }

        public async Task DeleteAsync(string id) =>
            await runTasksSettingsCollection.DeleteOneAsync(x => x.Id == id);
    }
}
