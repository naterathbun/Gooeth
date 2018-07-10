using System;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Gooeth.Models;
using Gooeth.MongoDB;

namespace Gooeth
{
    public class NowProcessor
    {
        private Mongo _mongo;
        private HttpClient _client;
        private Random _random;

        public NowProcessor()
        {
            _mongo = new Mongo("NowCharacter");
            _client = new HttpClient();
            _random = new Random();
        }


        public void Process(SlashCommandPayload command)
        {
            var character = new NowCharacter();
            var reply = new SlashCommandReply();
            var action = GetAction(command.text);

            switch (action)
            {
                case NowActions.CreateCharacter:
                    character = GetCharacter(command.team_domain, command.user_name);
                    reply.text = String.Format("Behold {0}, a powerful {1} (Level {2}).\nStr: {3}, Dex: {4}, Con: {5}, Int: {6}, Wis: {7}, Cha: {8}", character.Name, character.Class, character.Level, character.Strength, character.Dexterity, character.Constitution, character.Intelligence, character.Wisdom, character.Charisma);
                    reply.response_type = "in_channel";
                    break;
                case NowActions.RerollCharacter:
                    character = RerollCharacter(command.team_domain, command.user_name);
                    reply.text = String.Format("{0} has rerolled {1} and returned to level {2}.\nStr: {3}, Dex: {4}, Con: {5}, Int: {6}, Wis: {7}, Cha: {8}", character.Name, character.Class, character.Level, character.Strength, character.Dexterity, character.Constitution, character.Intelligence, character.Wisdom, character.Charisma);
                    reply.response_type = "in_channel";
                    break;
                case NowActions.Help:
                    reply.text = "/rpg.....Create or show off your character\n/rpg reroll.....Reset your character, class, stats and level\n/rpg leaderboard.....List top 10 characters\n/rpg fight {name}.....Fight your character against {name}\n/rpg help.....Show this list of commands";
                    reply.response_type = "ephemeral";
                    break;
                case NowActions.Leaderboard:
                    reply.text = GetLeaderboard(command.team_domain);
                    reply.response_type = "ephemeral";
                    break;
                case NowActions.Fight:
                    character = GetCharacter(command.team_domain, command.user_name);
                    reply.text = Fight(command.team_domain, character.Name, command.text);
                    reply.response_type = "in_channel";
                    break;
                case NowActions.Error:
                default:
                    reply.text = "Error in command string.";
                    reply.response_type = "ephemeral";
                    break;
            }

            SendPost(reply, command.response_url);
        }

        private async void SendPost(SlashCommandReply reply, string url)
        {
            var json = JsonConvert.SerializeObject(reply);
            var content = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
            await _client.PostAsync(url, content);
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

        private NowCharacter RerollCharacter(string team, string name)
        {
            var id = team + name;
            var character = _mongo.GetById<NowCharacter>(id);

            if (character == null)
                return CreateCharacter(team, name);

            ResetCharacter(character);
            _mongo.Save<NowCharacter>(character, id);

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

        private string GetLeaderboard(string team)
        {
            var leaders = _mongo.GetMany<NowCharacter>().ToList();

            var leaderboardSize = leaders.Count < 3 ? leaders.Count : 3;

            var leaderBoard = leaders
                .Where(x => x.Id.StartsWith(team))
                .OrderByDescending(x => x.Level)
                .ToList()
                .GetRange(0, leaderboardSize);

            var text = "Leaderboard for " + team + ":\n";
            int counter = 1;

            foreach (var character in leaders)
            {
                text = text + counter + ": " + character.Name + ", " + character.Class + ", Level " + character.Level + "\n";
                counter++;
            }

            return text;
        }

        private string Fight(string team, string characterName, string text)
        {
            var character = _mongo.GetById<NowCharacter>(team + characterName);
            var opponent = GetCharacter(team, text.Split(' ').Last());

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
                    _mongo.Save<NowCharacter>(character, character.Id);
                    return string.Format("The {0} {1} (Level {2}) has defeated the {3} {4} (Level {5}). Congrats!", character.Class, character.Name, character.Level, opponent.Class, opponent.Name, opponent.Level);
                }
                else
                    return string.Format("The {0} {1} (Level {2}) lost a fight to the {3} {4} (Level {5}). Too bad!", character.Class, character.Name, character.Level, opponent.Class, opponent.Name, opponent.Level);
            }

            return "Opponent not found.";
        }

        private int StatCompare(int charStat, int oppStat)
        {
            var randomizer = _random.Next(-3, 4);
            int result = (charStat - oppStat) + randomizer;
            return result >= 0 ? 1 : -1;
        }

        private int GetStat()
        {            
            return _random.Next(1, 20);
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

            var random = _random.Next(0, classes.Count);
            return classes[random];
        }

        private void ResetGame()
        {
            _mongo.Drop<NowCharacter>();
        }
    }
}