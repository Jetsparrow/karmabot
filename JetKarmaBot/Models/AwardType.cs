using System;
using System.Collections.Generic;

namespace JetKarmaBot.Models
{
    public partial class AwardType
    {
        public AwardType()
        {
            Awards = new HashSet<Award>();
        }

        public sbyte AwardTypeId { get; set; }
        public string CommandName { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Award> Awards { get; set; }
    }
}
