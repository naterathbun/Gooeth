using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gooeth.Models
{
    public class SlashCommandReply
    {
        public string response_type { get; set; }
        public string text { get; set; }
        public string[] attachments { get; set; }
    }
}