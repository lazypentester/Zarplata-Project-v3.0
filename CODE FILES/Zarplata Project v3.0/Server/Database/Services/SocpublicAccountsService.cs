using CommonModels.Client.Models;
using CommonModels.ProjectTask.Platform.SocpublicCom.Models;
using CommonModels.ProjectTask.Platform.SocpublicCom.Models.AccountInternalModels;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Server.Database.Models;

namespace Server.Database.Services
{
    public class SocpublicAccountsService
    {
        private readonly IMongoCollection<Account> accountsCollection;

        public SocpublicAccountsService(IOptions<ZarplataDatabaseSettings> zarplataDatabaseSettings)
        {
            var mongoClient = new MongoClient(zarplataDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(zarplataDatabaseSettings.Value.DatabaseName);

            accountsCollection = mongoDatabase.GetCollection<Account>(zarplataDatabaseSettings.Value.SocpublicAccountsCollectionName);
        }

        public async Task<List<Account>> GetAsync() =>
            await accountsCollection.Find(_ => true).ToListAsync();

        public async Task<Account?> GetAsync(string id) =>
            await accountsCollection.Find(account => account.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Account account) =>
            await accountsCollection.InsertOneAsync(account);

        public async Task CreateAsync(List<Account> accounts) =>
            await accountsCollection.InsertManyAsync(accounts);

        public async Task UpdateAsync(string id, string field, string value) =>
            await accountsCollection.UpdateOneAsync(x => x.Id == id, new BsonDocument("$set", new BsonDocument(field, value)));

        public async Task ReplaceAsync(string id, Account account) =>
            await accountsCollection.ReplaceOneAsync(x => x.Id == id, account);

        public async Task DeleteAsync(string id) =>
            await accountsCollection.DeleteOneAsync(x => x.Id == id);

        public async Task<bool> CheckUserAgentOnUnique(string useragent) =>
            await accountsCollection.Find(acc => acc.RegedUseragent != null && acc.RegedUseragent.Useragent != null && acc.RegedUseragent.Useragent == useragent).AnyAsync() ? false : true;

        public async Task TempMethodUpdateAsync()
        {
            //var filter = Builders<Account>.Filter.Eq(acc => acc.Login, "liiwlkxy");
            //await accountsCollection.UpdateOneAsync(_ => true, new BsonDocument("$set", new BsonDocument("PersonalPrivateProxy", BsonType.Null)));
            //await accountsCollection.UpdateOneAsync(_ => true, new BsonDocument("$set", new BsonDocument("PersonalPrivateProxy", BsonType.Null)));
            //await accountsCollection.UpdateManyAsync(_ => true, Builders<Account>.Update.Set(acc => acc.History, new List<OneLogAccounResult>()));
        }

        public async Task TempMethodDeleteAsync(string login, string email)
        {
            //var filter = Builders<Account>.Filter.Eq(acc => acc.Login, "liiwlkxy");
            //await accountsCollection.UpdateOneAsync(_ => true, new BsonDocument("$set", new BsonDocument("PersonalPrivateProxy", BsonType.Null)));
            //await accountsCollection.UpdateOneAsync(_ => true, new BsonDocument("$set", new BsonDocument("PersonalPrivateProxy", BsonType.Null)));
            //await accountsCollection.UpdateManyAsync(_ => true, Builders<Account>.Update.Set(acc => acc.History, new List<OneLogAccounResult>()));

            //var filter = Builders<Account>.Filter.Eq(acc => acc.Login, "liiwlkxy");
            //await accountsCollection.DeleteOneAsync(acc => acc.Login == login && acc.Email != null && acc.Email.Address == email && ((acc.EmailIsConfirmed.HasValue && acc.EmailIsConfirmed.Value == false) || (acc.EmailIsConfirmed.HasValue == false)));

        }
    }
}
