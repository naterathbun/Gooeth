using Gooeth.Models;
using Gooeth.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Gooeth.Controllers
{
    public class NowController : ApiController
    {
        private NowProcessor _nowProcessor;

        public NowController()
        {
            _nowProcessor = new NowProcessor();
        }

        [HttpGet()]
        [Route("api/now/{id}")]
        public string Get(string id)
        {
            return "Your character is " + id + ". Updated 7/5.";
        }
        
        [HttpPost()]
        [Route("api/now")]
        public string Post(SlashCommandPayload request)
        {
            var character = _nowProcessor.GetCharacter(request.user_id, request.user_name);
            var message = string.Format("Your character is {0}, a mighty {1}. You are level {2}.", character.Name, character.Class, character.Level);
            return message;
        }
    }
}

//- /NOW                Create or get character info
//- /NOW -reroll        Re-Roll character
//- /NOW -fight {name}	Fight against another character
//- /NOW -whois {name}  Get info on another character
//- /NOW -leaderboard   See the leaderboard
//- /NOW -help          Get list of helpful information