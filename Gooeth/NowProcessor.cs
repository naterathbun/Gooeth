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
            var character = new NowCharacter();
            var reply = new SlashCommandReply();
            var action = GetAction(command.text);

            switch (action)
            {
                case NowActions.CreateCharacter:
                    character = GetCharacter(command.team_domain, command.user_name);
                    reply.text = String.Format("You are {0}, a powerful {1}. You are level {2}.", character.Name, character.Class, character.Level);
                    break;
                case NowActions.RerollCharacter:
                    character = RerollCharacter(command.team_domain, command.user_name);
                    reply.text = String.Format("You are {0}, a powerful {1}. You are level {2}.", character.Name, character.Class, character.Level);
                    break;
                case NowActions.Help:
                    reply.text = "/rpg.....Create or Show your character\n/rpg reroll.....Resets your character, class, stats and level\n/rpg leaderboard.....Lists top 10 characters\n/rpg fight {name}.....Fight your character against {name}\n/rpg help.....Shows this list of commands";
                    break;
                case NowActions.Leaderboard:
                    reply.text = GetLeaderboard(command.team_id);
                    break;
                case NowActions.Fight:
                    character = GetCharacter(command.team_domain, command.user_name);
                    reply.text = ResolveFight(character, command.team_id, command.text);
                    break;
                case NowActions.Error:
                default:
                    reply.text = "Error in command string.";
                    break;
            }
            return reply;
        }

        private string GetLeaderboard(string team)
        {
            return "";
        }

        private string ResolveFight(NowCharacter character, string team, string text)
        {
            var opponentName = text;
            var opponent = GetCharacter(team, opponentName);

            if (opponent != null)
            {
                var strength = StatCompare(character.Strength, opponent.Strength);
                var dexterity = StatCompare(character.Dexterity, opponent.Dexterity);
                var constitution = StatCompare(character.Constitution, opponent.Constitution);
                var intelligence = StatCompare(character.Intelligence, opponent.Intelligence);
                var wisdom = StatCompare(character.Wisdom, opponent.Wisdom);
                var charisma = StatCompare(character.Charisma, opponent.Charisma);
                var result = strength + dexterity + constitution + intelligence + wisdom + charisma;

                if (result >= 0)
                {
                    character.Level += 1;
                    _mongo.Save<NowCharacter>(character);
                    return string.Format("You defeated {0}, and are now level {1}!", opponent.Name, character.Level);
                }
                else
                    return string.Format("You were defeated by {0} in a brutal battle.", opponent.Name);
            }

            return "Opponent not found.";
        }

        private int StatCompare(int charStat, int oppStat)
        {            
            var fightRandomizer = new Random().Next(-3, 4);
            int result = (charStat - oppStat) + fightRandomizer;
            return result >= 0 ? 1 : -1;
        }

        private NowCharacter GetCharacter(string team, string name)
        {
            var id = team + name;
            var character = _mongo.GetById<NowCharacter>(id);

            if (character != null)
                return character;

            return CreateCharacter(team, name);
        }

        private NowCharacter CreateCharacter(string team, string name)
        {
            var character = new NowCharacter()
            {
                Id = team + name,
                Name = name,
                Class = GetClass(),
                Level = 1,
                Strength = GetStat(),
                Dexterity = GetStat(),
                Constitution = GetStat(),
                Intelligence = GetStat(),
                Wisdom = GetStat(),
                Charisma = GetStat()
            };

            _mongo.Save<NowCharacter>(character);

            return character;
        }

        private NowCharacter RerollCharacter(string id, string name)
        {
            var character = _mongo.GetById<NowCharacter>(id + name);

            if (character == null)
                return CreateCharacter(id, name);

            ResetCharacter(character);
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
            character.Intelligence = GetStat();
            character.Wisdom = GetStat();
            character.Charisma = GetStat();

            return character;
        }

        private NowActions GetAction(string commandText)
        {
            if (string.IsNullOrEmpty(commandText))
                return NowActions.CreateCharacter;
            
            var text = commandText.ToLower();

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
                "Plumber",
                "Mall Cop",
                "Televangelist",
                "Curling Megafan",
                "Garbage Man",
                "Hobo",
                "Street Sweeper",
                "Mailman",
                "Junior Developer",
                "Plastic Surgeon",
                "Furniture Salesman",
                "Geriatric Walmart Greeter",
                "LARPer",
                "Youtube Personality",
                "Child Actor",
                "Abercrombie Model",
                "Starbucks Barista",
                "VCR Repairman",
                "Supreme Court Justice",
                "Juggalo",

            };

            var random = new Random().Next(0, classes.Count);
            return classes[random];
        }
    }
}