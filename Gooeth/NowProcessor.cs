using Gooeth.Models;
using Gooeth.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gooeth
{
    public class NowProcessor
    {
        private Mongo _mongo;

        public NowProcessor()
        {
            _mongo = new Mongo("NowCharacter");
        }

        public NowCharacter GetCharacter(string id, string name)
        {
            var character = _mongo.GetById<NowCharacter>(id);

            if (character != null)
                return character;

            return CreateCharacter(id, name);
        }

        private NowCharacter CreateCharacter(string id, string name)
        {
            var random = new Random();
            var character = new NowCharacter()
            {
                Id = id,
                Name = name,
                Class = GetRandomClass(),
                Level = 1,  
                Strength = random.Next(1,20),
                Dexterity = random.Next(1, 20),
                Constitution = random.Next(1, 20),
                Intellect = random.Next(1, 20),
                Wisdom = random.Next(1, 20),
                Charisma = random.Next(1, 20)
            };

            _mongo.Save<NowCharacter>(character);
            return character;
        }

        private string GetRandomClass()
        {
            var classes = new List<string>()
            {
                "Paladin",
                "Mage",
                "Rogue"
            };

            var random = new Random().Next(0, classes.Count);
            return classes[random];            
        }

    }
}