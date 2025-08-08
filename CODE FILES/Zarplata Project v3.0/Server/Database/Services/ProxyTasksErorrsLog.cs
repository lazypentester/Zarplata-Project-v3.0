using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Server.Database.Models;

namespace Server.Database.Services
{
    public class ProxyTasksErorrsLog
    {
        private readonly IMongoCollection<dynamic> proxyTasksErrorsLogCollection;

        public ProxyTasksErorrsLog(IOptions<ZarplataDatabaseSettings> zarplataDatabaseSettings)
        {
            var mongoClient = new MongoClient(zarplataDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(zarplataDatabaseSettings.Value.DatabaseName);

            proxyTasksErrorsLogCollection = mongoDatabase.GetCollection<dynamic>(zarplataDatabaseSettings.Value.ProxyTasksErrorsLogCollectionName);
        }

        public async Task<List<dynamic>?> GetAsync() =>
            await proxyTasksErrorsLogCollection.Find(Builders<dynamic>.Filter.Exists("_id", true)).ToListAsync();

        public async Task<dynamic?> GetAsync(string id)
        {
            var filter = Builders<dynamic>.Filter.Eq("_id", new ObjectId(id));
            return await proxyTasksErrorsLogCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(dynamic error) =>
            await proxyTasksErrorsLogCollection.InsertOneAsync(error);

        public async Task UpdateAsync(string id, string field, string value)
        {
            var filter = Builders<dynamic>.Filter.Eq("_id", new ObjectId(id));
            await proxyTasksErrorsLogCollection.UpdateOneAsync(filter, new BsonDocument("$set", new BsonDocument(field, value)));
        }

        public async Task UpdateAsync(string id, string field, int value)
        {
            var filter = Builders<dynamic>.Filter.Eq("_id", new ObjectId(id));
            await proxyTasksErrorsLogCollection.UpdateOneAsync(filter, new BsonDocument("$set", new BsonDocument(field, value)));
        }

        public async Task ReplaceAsync(string id, dynamic error)
        {
            var filter = Builders<dynamic>.Filter.Eq("_id", new ObjectId(id));
            await proxyTasksErrorsLogCollection.ReplaceOneAsync(filter, error);
        }

        public async Task DeleteAsync(string id)
        {
            var filter = Builders<dynamic>.Filter.Eq("_id", new ObjectId(id));
            await proxyTasksErrorsLogCollection.DeleteOneAsync(filter);
        }

        public async Task DeleteAsync()
        {
            var filter = Builders<dynamic>.Filter.Exists("_id", true);
            await proxyTasksErrorsLogCollection.DeleteManyAsync(filter);
        }


    }
}
