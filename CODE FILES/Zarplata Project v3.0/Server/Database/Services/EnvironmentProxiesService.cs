using CommonModels.Client.Models;
using CommonModels.ProjectTask.ProxyCombiner.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Server.Database.Models;
using static CommonModels.ProjectTask.ProxyCombiner.Models.EnvironmentProxy;

namespace Server.Database.Services
{
    public class EnvironmentProxiesService
    {
        private readonly IMongoCollection<EnvironmentProxy> environmentProxiesCollection;

        public EnvironmentProxiesService(IOptions<ZarplataDatabaseSettings> zarplataDatabaseSettings)
        {
            var mongoClient = new MongoClient(zarplataDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(zarplataDatabaseSettings.Value.DatabaseName);

            environmentProxiesCollection = mongoDatabase.GetCollection<EnvironmentProxy>(zarplataDatabaseSettings.Value.EnvironmentProxiesCollectionName);
        }

        public async Task<List<EnvironmentProxy>> GetAsync() =>
            await environmentProxiesCollection.Find(_ => true).ToListAsync();

        public async Task<List<EnvironmentProxy>> GetAsync(EPMarker marker) =>
            await environmentProxiesCollection.Find(proxy => proxy.Marker == marker).ToListAsync();

        //public async Task<EnvironmentProxy?> GetAsync(string id) =>
        //    await environmentProxiesCollection.Find(proxy => proxy.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(EnvironmentProxy proxy) =>
            await environmentProxiesCollection.InsertOneAsync(proxy);

        //public async Task UpdateAsync(string id, string field, string value) =>
        //    await environmentProxiesCollection.UpdateOneAsync(x => x.Id == id, new BsonDocument("$set", new BsonDocument(field, value)));

        //public async Task ReplaceAsync(string id, EnvironmentProxy proxy) =>
        //    await environmentProxiesCollection.ReplaceOneAsync(x => x.Id == id, proxy);

        //public async Task DeleteAsync(string id) =>
        //    await environmentProxiesCollection.DeleteOneAsync(x => x.Id == id);
    }
}
