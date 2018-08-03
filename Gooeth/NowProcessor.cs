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
                case NowActions.Checkin:
                    reply.text = GenerateCheckin();
                    reply.response_type = "in_channel";
                    break;
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
                    reply.text = "/rpg..... Create or show off your character\n/rpg reroll..... Reset your character, class, stats and level\n/rpg leaderboard..... List top 10 characters on your server\n/rpg fight {name}..... Fight your character against {name}\n/rpg help..... Show this list of commands";
                    reply.response_type = "ephemeral";
                    break;
                case NowActions.Leaderboard:
                    reply.text = GetLeaderboard(command.team_domain);
                    reply.response_type = "ephemeral";
                    break;
                case NowActions.Fight:
                    character = GetCharacter(command.team_domain, command.user_name);
                    reply.text = Fight(command.team_domain, character.Name, command.text);
                    reply.response_type = reply.text == "Opponent not found." ? "ephemeral" : "in_channel";
                    break;
                case NowActions.Error:
                default:
                    reply.text = "Error in command string.";
                    reply.response_type = "ephemeral";
                    break;
            }

            SendPost(reply, command.response_url);
        }

        private string GenerateCheckin()
        {
            var verb = new List<string>()
            {
                "working on",
                "occupied with",
                "looking into",
                "investigating",
                "refactoring",
                "trying to figure out",
                "fixing Chad's errors on",
                "undoing what Dustin did to",
                "updating",
                "converting",
                "removing old code from",
                "debugging",
                "trying to make sense of",
                "cleaning up",
                "writing tests for",
                "figuring out a bug with",
                "fixing a bug on",
                "learning more about",
                "adding new functionality to",
                "converting V11 code for",
                "publishing Nuget packages for",
                "reviewing a PR for",
                "in a meeting with ops about",
                "helping people with",
                "doing prod support for",
                "writing up a post mortem on",
                "recycling the app pools for",
                "putting together a mockup for",
            };

            var taskName = new List<string>()
            {
                "Azure",
                "Giordanos",
                "Web Transmission",
                "the Frontend",
                "Angular",
                "OComm",
                "the Service Bus queues",
                "Onosys.Core",
                "Geocoding",
                "Squirrel",
                "Authorize.Net",
                "Vantiv Litle",
                "Fishbowl",
                "Coupon Generation",
                "old V11 sites",
                "Rackspace",
                "Cloudflare",
                "the BP custom integration",
                "some old Eddie code",
                "Nothing Bundt onboarding",
                "Toppings Calculator",
                "Delivery Fee Calculator",
                "a potential new client",
                "Azure Functions",
                "IP Printer issues",
                "Delayed Orders"
            };

            var status = new List<string>()
            {
                "I should be done this afternoon",
                "It's taking longer than I anticipated",
                "I'll be wrapping it up shortly",
                "I'm going to need some help",
                "It's a bit more complicated than I originally thought",
                "I just need to write some tests and I'm done",
                "I have a lot more to do",
                "It's going well",
                "I'm totally lost",
                "No problems",
                "No issues",
                "Chugging along",
                "Everything is hunky dory",
                "I'll need someone to review",
                "Just finishing up",
                "All good",
                "Having some trouble",
                "I have no idea how to proceed",
                "I think we need to talk to Ops and get their input",
                "This needs reviewing"
            };

            var random = new Random();

            return string.Format("I'm {0} {1}. {2}.", verb[random.Next(0, verb.Count)], taskName[random.Next(0, taskName.Count)], status[random.Next(0, status.Count)]);
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

            if (text.Contains("checkin"))
                return NowActions.Checkin;

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

        private NowCharacter GetOpponent(string team, string name)
        {
            var id = team + name;
            return _mongo.GetById<NowCharacter>(id);
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
            var leaders = _mongo.GetMany<NowCharacter>()
                .Where(x => x.Id.StartsWith(team))
                .OrderByDescending(x => x.Level)
                .ToList();

            var leaderboardSize = leaders.Count < 10 ? leaders.Count : 10;

            var topThree = leaders.GetRange(0, leaderboardSize).ToList();

            var text = "Leaderboard for " + team + ":\n";
            int counter = 1;

            foreach (var character in topThree)
            {
                text = text + counter + ": " + character.Name + ", " + character.Class + ", Level " + character.Level + "\n";
                counter++;
            }

            return text;
        }

        private string Fight(string team, string characterName, string text)
        {
            var character = _mongo.GetById<NowCharacter>(team + characterName);
            var opponent = GetOpponent(team, text.Split(' ').Last());

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
                    if (character.Level > (opponent.Level + 25))
                        return string.Format("The {0} {1} (Level {2}) has crushed the {3} {4} (Level {5}). Such an easy victory granted no experience.", character.Class, character.Name, character.Level, opponent.Class, opponent.Name, opponent.Level);

                    character.Level += 1;
                    _mongo.Save<NowCharacter>(character, character.Id);
                    return string.Format("The {0} {1} (Level {2}) has defeated the {3} {4} (Level {5}). Congrats!", character.Class, character.Name, character.Level, opponent.Class, opponent.Name, opponent.Level);
                }
                else 
                {
                    if (character.Level > opponent.Level)
                    {
                        character.Level += 1;
                        _mongo.Save<NowCharacter>(character, character.Id);
                        return string.Format("The {0} {1} (Level {2}) lost a fight to the {3} {4} (Level {5}) and has been weakened!", character.Class, character.Name, character.Level, opponent.Class, opponent.Name, opponent.Level);
                    }
                    return string.Format("The {0} {1} (Level {2}) lost a fight to the {3} {4} (Level {5}). Too bad!", character.Class, character.Name, character.Level, opponent.Class, opponent.Name, opponent.Level);
                }
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
                "Trenchcoat Watch Salesman",
                "Taylor Swift Roadie",
                "Master Hacker",
                "Pizza Toppings Critic",
                "Dog Walker",
                "Dustin Sarcasm Translator",
                "Hamster Rancher",
                "Dumpster Diver",
            };

            var random = _random.Next(0, classes.Count);
            return classes[random];
        }

        private void ResetGame()
        {
            _mongo.Drop<NowCharacter>();
        }

        private void DeleteChar(string team, string name)
        {
            var id = team + name;
            _mongo.Delete<NowCharacter>(id);
        }
    }
}