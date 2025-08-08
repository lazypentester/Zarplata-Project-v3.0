using CommonModels.User.Models;
using CommonModels.User.Session;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Server.Database.Models;

namespace Server.Database.Services
{
    public class UserSessionService
    {
        private readonly IMongoCollection<SessionModel> userSessionsCollection;

        public UserSessionService(IOptions<ZarplataDatabaseSettings> zarplataDatabaseSettings)
        {
            var mongoClient = new MongoClient(zarplataDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(zarplataDatabaseSettings.Value.DatabaseName);

            userSessionsCollection = mongoDatabase.GetCollection<SessionModel>(zarplataDatabaseSettings.Value.UserSessionsCollectionName);
        }

        public async Task<List<SessionModel>> GetAsync() =>
            await userSessionsCollection.Find(_ => true).ToListAsync();

        public async Task<SessionModel?> GetAsync(string id) =>
            await userSessionsCollection.Find(session => session.Id == id).FirstOrDefaultAsync();

        public async Task<List<SessionModel>> GetByUserIdAsync(string user_id) =>
            await userSessionsCollection.Find(session => session.UserId == user_id).ToListAsync();

        public async Task<SessionModel?> GetByRefreshTokenAsync(string refresh_token) =>
            await userSessionsCollection.Find(session => session.RefreshToken == refresh_token).FirstOrDefaultAsync();

        public async Task CreateAsync(SessionModel session) =>
            await userSessionsCollection.InsertOneAsync(session);

        public async Task UpdateAsync(string id, string field, string value) =>
            await userSessionsCollection.UpdateOneAsync(x => x.Id == id, new BsonDocument("$set", new BsonDocument(field, value)));

        public async Task UpdateAsync(string id, Dictionary<string, object> fieldsAndValues, string command = "$set")
        {
            BsonDocument fieldAndValueCollections = new BsonDocument();

            fieldAndValueCollections.AddRange(fieldsAndValues);

            //await siteParseBalancerCollection.UpdateOneAsync(x => x.Id == id, new BsonDocument("$set", new BsonDocument { { "", "" }, { "", "" } }));
            await userSessionsCollection.UpdateOneAsync(x => x.Id == id, new BsonDocument(command, fieldAndValueCollections));
        }

        public async Task ReplaceAsync(string id, SessionModel session) =>
            await userSessionsCollection.ReplaceOneAsync(x => x.Id == id, session);

        public async Task DeleteAsync(string id) =>
            await userSessionsCollection.DeleteOneAsync(x => x.Id == id);

        public async Task DeleteByRefreshTokenAsync(string refresh_token) =>
            await userSessionsCollection.DeleteOneAsync(x => x.RefreshToken == refresh_token);

        public async Task DeleteManyAsync(List<string> ids) 
        {
            FilterDefinition<SessionModel>? filter = Builders<SessionModel>.Filter.In(s => s.Id, ids);
            await userSessionsCollection.DeleteManyAsync(filter);
        }








        public async Task TEMPMETHOD()
        {
            //await userSessionsCollection.UpdateManyAsync(_ => true, Builders<SessionModel>.Update.Rename("StartDatetime", "StartDateTime"));
            //await userSessionsCollection.UpdateManyAsync(_ => true, Builders<SessionModel>.Update.Rename("ExpiresDatetime", "ExpiresDateTime"));
            //await clientsCollection.DeleteManyAsync(_ => true);
        }
    }
}
