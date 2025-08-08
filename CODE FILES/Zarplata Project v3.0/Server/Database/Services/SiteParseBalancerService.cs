using CommonModels.ProjectTask.ProxyCombiner.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Server.Database.Models;

namespace Server.Database.Services
{
    public class SiteParseBalancerService
    {
        private readonly IMongoCollection<SiteParseBalancer> siteParseBalancerCollection;

        public SiteParseBalancerService(IOptions<ZarplataDatabaseSettings> zarplataDatabaseSettings)
        {
            var mongoClient = new MongoClient(zarplataDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(zarplataDatabaseSettings.Value.DatabaseName);

            siteParseBalancerCollection = mongoDatabase.GetCollection<SiteParseBalancer>(zarplataDatabaseSettings.Value.SiteParseBalancerCollectionName);
        }

        public async Task<List<SiteParseBalancer>> GetAsync() =>
            await siteParseBalancerCollection.Find(_ => true).ToListAsync();

        public async Task<SiteParseBalancer?> GetAsync(string id) =>
            await siteParseBalancerCollection.Find(proxy => proxy.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(SiteParseBalancer balancer) =>
            await siteParseBalancerCollection.InsertOneAsync(balancer);

        public async Task CreateAsync(List<SiteParseBalancer> balancer) =>
            await siteParseBalancerCollection.InsertManyAsync(balancer);

        /*        public async Task UpdateAsync(string id, string field, string value) =>
                    await siteParseBalancerCollection.UpdateOneAsync(x => x.Id == id, new BsonDocument("$set", new BsonDocument(field, value)));*/

        public async Task UpdateAsync(string id, Dictionary<string, object> fieldsAndValues, string command = "$set") // ???
        {
            BsonDocument fieldAndValueCollections = new BsonDocument();

            fieldAndValueCollections.AddRange(fieldsAndValues);

            //await siteParseBalancerCollection.UpdateOneAsync(x => x.Id == id, new BsonDocument("$set", new BsonDocument { { "", "" }, { "", "" } }));
            await siteParseBalancerCollection.UpdateOneAsync(x => x.Id == id, new BsonDocument(command, fieldAndValueCollections));
        }

        public async Task ReplaceAsync(string id, SiteParseBalancer balancer) =>
            await siteParseBalancerCollection.ReplaceOneAsync(x => x.Id == id, balancer);

        public async Task DeleteAsync(string id) =>
            await siteParseBalancerCollection.DeleteOneAsync(x => x.Id == id);
    }
}
