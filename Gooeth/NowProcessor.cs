using Gooeth.Models;
using Gooeth.MongoDB;
using System;
using System.Collections.Generic;

namespace Gooeth
{
    public class NowProcessor
    {
        private Mongo _mongo;

        public NowProcessor()
        {
            _mongo = new Mongo("NowCharacter");
        }


        public SlashCommandReply Process(SlashCommandPayload command)
        {
            var action = GetAction(command.text);

            var character = GetCharacter(command.user_id, command.user_name);
            return FormatReply(character, NowActions.CreateCharacter);





            return new SlashCommandReply();
        }

        public NowCharacter GetCharacter(string id, string name)
        {
            var character = _mongo.GetById<NowCharacter>(id);

            if (character != null)
                return character;

            return CreateCharacter(id, name);
        }

        public NowCharacter RerollCharacter(string id, string name)
        {
            var character = _mongo.GetById<NowCharacter>(id);

            if (character == null)
                return CreateCharacter(id, name);

            ResetCharacter(character);
            _mongo.Save<NowCharacter>(character);

            return character;
        }

        public SlashCommandReply FormatReply(NowCharacter character, NowActions action)
        {
            var reply = new SlashCommandReply();







            return reply;
        }


        private NowCharacter CreateCharacter(string id, string name)
        {
            var character = new NowCharacter()
            {
                Id = id,
                Name = name,
                Class = GetClass(),
                Level = 1,
                Strength = GetStat(),
                Dexterity = GetStat(),
                Constitution = GetStat(),
                Intellect = GetStat(),
                Wisdom = GetStat(),
                Charisma = GetStat()
            };

            _mongo.Save<NowCharacter>(character);

            return character;
        }

        private NowCharacter ResetCharacter(NowCharacter character)
        {
            character.Level = 1;
            character.Class = GetClass();
            character.Strength = GetStat();
            character.Dexterity = GetStat();
            character.Constitution = GetStat();
            character.Intellect = GetStat();
            character.Wisdom = GetStat();
            character.Charisma = GetStat();

            return character;
        }

        private NowActions GetAction(string commandText)
        {
            var text = commandText.ToLower();

            if (text == null || text == "")
                return NowActions.CreateCharacter;

            if (text.Contains("help"))
                return NowActions.Help;

            if (text.Contains("reroll"))
                return NowActions.RerollCharacter;

            if (text.Contains("leaderboard"))
                return NowActions.Leaderboard;

            if (text.Contains("fight"))
                return NowActions.Fight;

            return NowActions.Error;
        }

        private int GetStat()
        {
            var random = new Random();
            return random.Next(1, 20);
        }

        private string GetClass()
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