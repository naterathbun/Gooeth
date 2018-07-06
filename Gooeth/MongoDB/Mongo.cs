using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace Gooeth.MongoDB
{
    public class Mongo
    {
        private MongoClient _client;
        private IMongoDatabase _database;


        public Mongo(string collection)
        {            
            var mongoUrl = new MongoUrl("mongodb://nate:tubaphone5@cluster0-shard-00-00-h7xv4.gcp.mongodb.net:27017,cluster0-shard-00-01-h7xv4.gcp.mongodb.net:27017,cluster0-shard-00-02-h7xv4.gcp.mongodb.net:27017/test?ssl=true&replicaSet=Cluster0-shard-0&authSource=admin&retryWrites=true");
            _client = new MongoClient(mongoUrl);
            _database = _client.GetDatabase("gooeth");
        }

        public T GetById<T>(string id)
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            return _database.GetCollection<T>(typeof(T).Name).Find<T>(filter).FirstOrDefault();
        }

        public IEnumerable<T> GetMany<T>()
        {
            return _database.GetCollection<T>(typeof(T).Name).Find(r => true).ToList();
        }

        public void Save<T>(T value, string id = null)
        {
            var collection = _database.GetCollection<T>(typeof(T).Name);

            if (id == null)
                collection.InsertOne(value);
            else
                collection.ReplaceOne(Builders<T>.Filter.Eq("_id", id), value);
        }

        public void Delete<T>(string id)
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            _database.GetCollection<T>(typeof(T).Name).FindOneAndDelete<T>(filter);
        }
    }
}