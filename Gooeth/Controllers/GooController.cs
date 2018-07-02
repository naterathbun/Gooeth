using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Gooeth.Controllers
{
    public class GooController : ApiController
    {
        // GET A LIST OF ALL ITEMS
        [HttpGet()]
        [Route("api/goo")]
        public string[] Get()
        {
            return new string[] { "First", "Second", "Third", "Fourth" };
        }

        // GET A SPECIFIC ITEM
        [HttpGet()]
        [Route("api/goo/{id}")]
        public string Get(string id)
        {
            return "The ID you entered is " + id;
        }

        // SAVE A NEW THING
        [HttpPost()]
        [Route("api/goo")]
        public void Post(string value)
        {
            
        }        
    }
}