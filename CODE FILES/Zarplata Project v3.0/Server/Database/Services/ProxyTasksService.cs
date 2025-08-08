using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Server.Database.Models;

namespace Server.Database.Services
{
    public class ProxyTasksService
    {
        private readonly IMongoCollection<dynamic> proxyTasksCollection;

        public ProxyTasksService(IOptions<ZarplataDatabaseSettings> zarplataDatabaseSettings)
        {
            var mongoClient = new MongoClient(zarplataDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(zarplataDatabaseSettings.Value.DatabaseName);

            proxyTasksCollection = mongoDatabase.GetCollection<dynamic>(zarplataDatabaseSettings.Value.ProxyTasksCollectionName);
        }

        public async Task<List<dynamic>?> GetAsync() =>
            await proxyTasksCollection.Find(Builders<dynamic>.Filter.Exists("_id", true)).ToListAsync();

        public async Task<dynamic?> GetAsync(string id)
        {
            var filter = Builders<dynamic>.Filter.Eq("_id", new ObjectId(id));
            return await proxyTasksCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<dynamic>> GetOneDisconnectedAsync() =>
            await proxyTasksCollection.Find(Builders<dynamic>.Filter.Eq("Status", CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Executing)).ToListAsync();

        public async Task CreateAsync(dynamic task) =>
            await proxyTasksCollection.InsertOneAsync(task);

        public async Task UpdateAsync(string id, string field, string value)
        {
            var filter = Builders<dynamic>.Filter.Eq("_id", new ObjectId(id));
            await proxyTasksCollection.UpdateOneAsync(filter, new BsonDocument("$set", new BsonDocument(field, value)));
        }

        public async Task UpdateAsync(string id, string field, int value)
        {
            var filter = Builders<dynamic>.Filter.Eq("_id", new ObjectId(id));
            await proxyTasksCollection.UpdateOneAsync(filter, new BsonDocument("$set", new BsonDocument(field, value)));
        }

        public async Task ReplaceAsync(string id, dynamic task)
        {
            var filter = Builders<dynamic>.Filter.Eq("_id", new ObjectId(id));
            await proxyTasksCollection.ReplaceOneAsync(filter, task);
        }

        public async Task DeleteAsync(string id)
        {
            var filter = Builders<dynamic>.Filter.Eq("_id", new ObjectId(id));
            await proxyTasksCollection.DeleteOneAsync(filter);

        }

        public async Task DeleteAsync()
        {
            var filter = Builders<dynamic>.Filter.Exists("_id", true);
            await proxyTasksCollection.DeleteManyAsync(filter);
        }
    }
}
