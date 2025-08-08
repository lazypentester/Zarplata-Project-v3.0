using CommonModels.ProjectTask.EarningSite;
using CommonModels.ProjectTask.Platform;
using CommonModels.ProjectTask.ProxyCombiner.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Server.Database.Models;

namespace Server.Database.Services
{
    public class AccountReservedProxyService
    {
        private readonly IMongoCollection<AccountReservedProxy> accountReservedProxiesCollection;

        public AccountReservedProxyService(IOptions<ZarplataDatabaseSettings> zarplataDatabaseSettings)
        {
            var mongoClient = new MongoClient(zarplataDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(zarplataDatabaseSettings.Value.DatabaseName);

            accountReservedProxiesCollection = mongoDatabase.GetCollection<AccountReservedProxy>(zarplataDatabaseSettings.Value.AccountReservedProxiesCollectionName);
        }

        public async Task<List<AccountReservedProxy>> GetAsync() =>
            await accountReservedProxiesCollection.Find(_ => true).ToListAsync();

        public async Task<AccountReservedProxy?> GetAsync(string id) =>
            await accountReservedProxiesCollection.Find(proxy => proxy.Id == id).FirstOrDefaultAsync();

        public async Task<List<AccountReservedProxy>?> GetAsync(EarningSiteTaskEnums.EarningSiteEnum reservedPlatform) =>
            await accountReservedProxiesCollection.Find(proxy => proxy.ReservedPlatform == reservedPlatform).ToListAsync();

        public async Task CreateAsync(AccountReservedProxy proxy) =>
            await accountReservedProxiesCollection.InsertOneAsync(proxy);

        public async Task UpdateAsync(string id, string field, string value) =>
            await accountReservedProxiesCollection.UpdateOneAsync(x => x.Id == id, new BsonDocument("$set", new BsonDocument(field, value)));

        public async Task ReplaceAsync(string id, AccountReservedProxy proxy) =>
            await accountReservedProxiesCollection.ReplaceOneAsync(x => x.Id == id, proxy);

        public async Task DeleteAsync(string id) =>
            await accountReservedProxiesCollection.DeleteOneAsync(x => x.Id == id);
    }
}
