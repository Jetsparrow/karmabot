using System;
using System.Collections.Generic;

namespace JetKarmaBot.Models
{
    public partial class User
    {
        public User()
        {
            AwardFrom = new HashSet<Award>();
            AwardTo = new HashSet<Award>();
        }

        public long Userid { get; set; }
        public string Username { get; set; }

        public ICollection<Award> AwardFrom { get; set; }
        public ICollection<Award> AwardTo { get; set; }
    }
}
