using Gooeth.Models;
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
        // Get Info
        [HttpGet()]
        [Route("api/now/{id}")]
        public string Get(string id)
        {
            return "Your character is " + id + ".";
        }

        //
        [HttpPost()]
        [Route("api/now")]
        public string Post(NowRequest request)
        {
            return "stuff";
        }        
    }
}

//- /NOW                Create or get character info
//- /NOW -reroll        Re-Roll character
//- /NOW -fight {name}	Fight against another character
//- /NOW -whois {name}  Get info on another character
//- /NOW -leaderboard   See the leaderboard
//- /NOW -help          Get list of helpful information