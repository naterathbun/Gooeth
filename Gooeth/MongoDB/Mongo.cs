using MongoDB.Driver;
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

        public Mongo()
        {
            _client = new MongoClient("mongodb+srv://naterathbun:<Luminaire1!>@cluster0-h7xv4.gcp.mongodb.net/test?retryWrites=true");
            _database = _client.GetDatabase("gooeth");            
        }

        public void AddOrUpdate(string info)
        {
            
        }

        public void Delete(string info)
        {

        }


    }
}