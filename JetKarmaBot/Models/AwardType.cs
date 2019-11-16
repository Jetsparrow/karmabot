using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

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
        public long ChatId { get; set; }
        [Column("name")]
        public string NominativeName { get; set; }
        [Column("accname")]
        public string AccusativeName { get; set; }
        public string Symbol { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Award> Awards { get; set; }
        [ForeignKey("ChatId")]
        public virtual Chat Chat { get; set; }
    }
}
