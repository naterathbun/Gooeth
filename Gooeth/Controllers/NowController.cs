﻿using Gooeth.Models;
using Gooeth.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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
        [Route("api/now")]
        public string Get()
        {
            return "NOW is online.";
        }
        
        [HttpPost()]
        [Route("api/now")]
        public IHttpActionResult Post(SlashCommandPayload request)
        {
            Task.Run(async () =>
            {
                _nowProcessor.Process(request);
            });
            return Ok();
        }        
    }
}