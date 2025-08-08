using CommonModels.Client.Models;
using CommonModels.User.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Server.Database.Models;

namespace Server.Database.Services
{
    public class UsersService
    {
        private readonly IMongoCollection<UserModel> usersCollection;

        public UsersService(IOptions<ZarplataDatabaseSettings> zarplataDatabaseSettings)
        {
            var mongoClient = new MongoClient(zarplataDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(zarplataDatabaseSettings.Value.DatabaseName);

            usersCollection = mongoDatabase.GetCollection<UserModel>(zarplataDatabaseSettings.Value.UsersCollectionName);
        }

        public async Task<List<UserModel>> GetAsync() =>
            await usersCollection.Find(_ => true).ToListAsync();

        public async Task<UserModel?> GetAsync(string id) =>
            await usersCollection.Find(user => user.Id == id).FirstOrDefaultAsync();

        public async Task<UserModel?> GetByUsernameAsync(string username) =>
            await usersCollection.Find(user => user.Username == username).FirstOrDefaultAsync();

        public async Task CreateAsync(UserModel user) =>
            await usersCollection.InsertOneAsync(user);

        public async Task UpdateAsync(string id, string field, string value) =>
            await usersCollection.UpdateOneAsync(x => x.Id == id, new BsonDocument("$set", new BsonDocument(field, value)));

        public async Task ReplaceAsync(string id, UserModel user) =>
            await usersCollection.ReplaceOneAsync(x => x.Id == id, user);

        public async Task DeleteAsync(string id) =>
            await usersCollection.DeleteOneAsync(x => x.Id == id);










        public async Task TEMPMETHOD()
        {
            //await userSessionsCollection.UpdateManyAsync(_ => true, Builders<SessionModel>.Update.Rename("StartDatetime", "StartDateTime"));
            //await userSessionsCollection.UpdateManyAsync(_ => true, Builders<SessionModel>.Update.Rename("ExpiresDatetime", "ExpiresDateTime"));
            //await clientsCollection.DeleteManyAsync(_ => true);
        }
    }
}
