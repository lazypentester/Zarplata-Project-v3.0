using CommonModels.Client.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Server.Database.Models;

namespace Server.Database.Services
{
    public class BlockedMachinesService
    {
        private readonly IMongoCollection<ModelBlockedMachine> blockedMachinesCollection;

        public BlockedMachinesService(IOptions<ZarplataDatabaseSettings> zarplataDatabaseSettings)
        {
            var mongoClient = new MongoClient(zarplataDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(zarplataDatabaseSettings.Value.DatabaseName);

            blockedMachinesCollection = mongoDatabase.GetCollection<ModelBlockedMachine>(zarplataDatabaseSettings.Value.BlockedMachinesCollectionName);
        }

        public async Task<List<ModelBlockedMachine>> GetAsync() =>
            await blockedMachinesCollection.Find(_ => true).ToListAsync();

        public async Task<List<ModelBlockedMachine>> GetAsync(string clientIp, string machineIdentityKey) =>
            await blockedMachinesCollection.Find(blockedMachine => blockedMachine.IP == clientIp || blockedMachine.MACHINE.IDENTITY_KEY == machineIdentityKey).ToListAsync();

        public async Task CreateAsync(List<ModelBlockedMachine> machinesToBlock) =>
            await blockedMachinesCollection.InsertManyAsync(machinesToBlock);
    }
}
