using System;
using System.Collections.Generic;

namespace JetKarmaBot.Models
{
    public partial class Chat
    {
        public long ChatId { get; set; }
        public string Locale { get; set; }
        public bool IsAdministrator { get; set; }

        public virtual ICollection<Award> Awards { get; set; }
        public virtual ICollection<AwardType> AwardTypes { get; set; }
    }
}
