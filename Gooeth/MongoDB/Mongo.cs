using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gooeth.MongoDB
{
    public class Mongo
    {
        private MongoClient _client;
        private IMongoDatabase _database;

        public Mongo(string collection)
        {
            _client = new MongoClient("mongodb+srv://naterathbun:<Luminaire1!>@cluster0-h7xv4.gcp.mongodb.net/test?retryWrites=true");
            _database = _client.GetDatabase("gooeth");
        }

        public T GetById<T>(string id)
        {
            var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
            return _database.GetCollection<T>(typeof(T).Name).Find<T>(filter).FirstOrDefault();            
        }

        public List<T> GetMany<T>()
        {
            return _database.GetCollection<T>(typeof(T).Name).Find(r => true).ToList();
        }

        public void Save<T>(T value)
        {
            _database.GetCollection<T>(typeof(T).Name).InsertOne(value);
        }
        
        public void Delete<T>(string id)
        {
            var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
            _database.GetCollection<T>(typeof(T).Name).FindOneAndDelete<T>(filter);
        }

    }
}