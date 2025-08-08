using AngleSharp.Dom;
using CommonModels.Client;
using CommonModels.Client.Models;
using CommonModels.Client.Models.SearchBotsModels;
using CommonModels.Client.Models.SearchBotsModels.FilterModels;
using CommonModels.User.Session;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Server.Database.Models;
using System.ComponentModel;
using System.Data;
using System.Text.Json;
using System.Text.RegularExpressions;
using static CommonModels.Client.Client;
using static CommonModels.Client.Machine;
using static CommonModels.Client.Models.SearchBotsModels.SearchBotEnums;

namespace Server.Database.Services
{
    public class ClientsService
    {
        private readonly IMongoCollection<ModelClient> clientsCollection;

        public ClientsService(IOptions<ZarplataDatabaseSettings> zarplataDatabaseSettings)
        {
            var mongoClient = new MongoClient(zarplataDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(zarplataDatabaseSettings.Value.DatabaseName);

            clientsCollection = mongoDatabase.GetCollection<ModelClient>(zarplataDatabaseSettings.Value.ClientsCollectionName);
        }

        public async Task<List<ModelClient>> GetAsync() =>
            await clientsCollection.Find(_ => true).ToListAsync();

        public async Task<List<ModelClient>> GetWithFiltersAndSortByRegistrationDateAsync(
            Dictionary<string, List<int>> filtersInTypeInt, 
            Dictionary<string, List<string>> filtersInTypeString, 
            KeyValuePair<string, string> searchKeywordFilter,
            DateTime? searchKeywordDateTime,
            FindSortBots sortBots)
        {
            BsonArray subfiltersCollection = new BsonArray();

            // add IN filters Type Int
            foreach (var f in filtersInTypeInt)
            {
                subfiltersCollection.Add(new BsonDocument(f.Key, new BsonDocument("$in", new BsonArray(f.Value))));
            }

            // add IN filters Type String
            foreach (var f in filtersInTypeString)
            {
                subfiltersCollection.Add(new BsonDocument(f.Key, new BsonDocument("$in", new BsonArray(f.Value))));
            }

            // add search keyword (REGEX) or Datetime filter
            if (!string.IsNullOrEmpty(searchKeywordFilter.Value))
            {
                if (searchKeywordFilter.Key == FindSearchKeywordParametersBots.RegistrationDateTime.ToString() && searchKeywordDateTime.HasValue)
                {
                    subfiltersCollection.Add(new BsonDocument(searchKeywordFilter.Key, new BsonDocument("$gte", searchKeywordDateTime.Value)));
                    subfiltersCollection.Add(new BsonDocument(searchKeywordFilter.Key, new BsonDocument("$lt", searchKeywordDateTime.Value.AddDays(1))));
                }
                else if(searchKeywordFilter.Key == FindSearchKeywordParametersBots.Id.ToString())
                {
                    subfiltersCollection.Add(new BsonDocument("_id", new BsonDocument("$eq", new BsonObjectId(new ObjectId(searchKeywordFilter.Value)))));
                }
                else
                {
                    subfiltersCollection.Add(new BsonDocument(searchKeywordFilter.Key, new BsonDocument("$regex", new BsonRegularExpression(new Regex(searchKeywordFilter.Value, RegexOptions.IgnoreCase)))));
                }
            }

            // add sort
            string sortField = nameof(FindSearchKeywordParametersBots.RegistrationDateTime);
            int sortValue = -1;
            if(sortBots == SearchBotEnums.FindSortBots.NewFirst)
            {
                sortValue = -1;
            }
            else if (sortBots == SearchBotEnums.FindSortBots.OldFirst)
            {
                sortValue = 1;
            }

            BsonDocument filter = new BsonDocument("$and", subfiltersCollection);

            return await clientsCollection.Find(filter).Sort(new BsonDocument(sortField, sortValue)).ToListAsync();
        }

        public async Task<List<ModelClient>?> GetByMachineIdentityKeyAsync(List<string> machineIdentityKeys)
        {
            var filter = Builders<ModelClient>.Filter.In(bot => bot.MACHINE.IDENTITY_KEY, machineIdentityKeys);
            return await clientsCollection.Find(filter).ToListAsync();
        }

        public async Task<ModelClient?> GetAsync(string id) =>
            await clientsCollection.Find(client => client.ID == id).FirstOrDefaultAsync();

        public async Task<bool> ChangeBotsStatusToFree(List<string> botsIdList)
        {
            bool change_success = false;

            try
            {
                var filter = Builders<ModelClient>.Filter.In(bot => bot.ID, botsIdList);
                await clientsCollection.UpdateManyAsync(filter, Builders<ModelClient>.Update.Set(bot => bot.Status, ClientStatus.Free));

                change_success = true;
            }
            catch { }

            return change_success;
        }

        public async Task<bool> ChangeBotsStatusToAtWork(List<string> botsIdList)
        {
            bool change_success = false;

            try
            {
                var filter = Builders<ModelClient>.Filter.In(bot => bot.ID, botsIdList);
                await clientsCollection.UpdateManyAsync(filter, Builders<ModelClient>.Update.Set(bot => bot.Status, ClientStatus.AtWork));

                change_success = true;
            }
            catch { }

            return change_success;
        }

        public async Task<bool> ChangeBotsStatusToStopped(List<string> botsIdList)
        {
            bool change_success = false;

            try
            {
                var filter = Builders<ModelClient>.Filter.In(bot => bot.ID, botsIdList);
                await clientsCollection.UpdateManyAsync(filter, Builders<ModelClient>.Update.Set(bot => bot.Status, ClientStatus.Stopped));

                change_success = true;
            }
            catch { }

            return change_success;
        }

        public async Task CreateAsync(ModelClient client) =>
            await clientsCollection.InsertOneAsync(client);

        public async Task CreateAsync(List<ModelClient> clients) =>
            await clientsCollection.InsertManyAsync(clients);

        public async Task UpdateAsync(string id, string field, DateTime value) =>
            await clientsCollection.UpdateOneAsync(x => x.ID == id, new BsonDocument("$set", new BsonDocument(field, value)));

        public async Task UpdateAsync(string id, string field, string value) =>
            await clientsCollection.UpdateOneAsync(x => x.ID == id, new BsonDocument("$set", new BsonDocument(field, value)));

        public async Task UpdateAsync(string id, string field, int value) =>
            await clientsCollection.UpdateOneAsync(x => x.ID == id, new BsonDocument("$set", new BsonDocument(field, value)));

        public async Task ReplaceAsync(string id, ModelClient client) =>
            await clientsCollection.ReplaceOneAsync(x => x.ID == id, client);

        public async Task DeleteAsync(string id) =>
            await clientsCollection.DeleteOneAsync(x => x.ID == id);

        public async Task DeleteAsync(List<string> botsIdList)
        {
            var filter = Builders<ModelClient>.Filter.In(bot => bot.ID, botsIdList);
            await clientsCollection.DeleteManyAsync(filter);
        }






        //public async Task TEMPMETHOD()
        //{
        //    //await clientsCollection.UpdateManyAsync(_ => true, Builders<ModelClient>.Update.Rename("MachineName", "Machine.MachineName"));
        //    await clientsCollection.UpdateManyAsync(_ => true, Builders<ModelClient>.Update.Rename("BusyStatus", "Status"));
        //    //await clientsCollection.DeleteManyAsync(_ => true);
        //}


        //public async Task TEMPMETHOD()
        //{
        //    await clientsCollection.UpdateManyAsync(el => el.MACHINE.OS_PLATFORM_TEXT == "Linux", Builders<ModelClient>.Update.Set("Machine.OSPlatform", Platform.Linux));
        //    await clientsCollection.UpdateManyAsync(el => el.MACHINE.OS_PLATFORM_TEXT == "Windows", Builders<ModelClient>.Update.Set("Machine.OSPlatform", Platform.Windows));
        //    await clientsCollection.UpdateManyAsync(el => el.MACHINE.OS_PLATFORM_TEXT == "MacOS", Builders<ModelClient>.Update.Set("Machine.OSPlatform", Platform.MacOS));
        //    await clientsCollection.UpdateManyAsync(el => (el.MACHINE.OS_PLATFORM_TEXT != "MacOS" && el.MACHINE.OS_PLATFORM_TEXT != "Windows" && el.MACHINE.OS_PLATFORM_TEXT != "Linux"), Builders<ModelClient>.Update.Set("Machine.OSPlatform", Platform.Other));
        //}

        //public async Task TEMPMETHOD()
        //{
        //    await clientsCollection.UpdateManyAsync(new BsonDocument("Role", "0"), Builders<ModelClient>.Update.Set("Machine.OSPlatform", Platform.Linux));
        //}
    }
}
