using System;
using System.Collections.Generic;

namespace JetKarmaBot.Models
{
    public partial class Chat
    {
        public Chat()
        {
            Award = new HashSet<Award>();
        }

        public long Chatid { get; set; }
        public string Locale { get; set; }

        public ICollection<Award> Award { get; set; }
    }
}
