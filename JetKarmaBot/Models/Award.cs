using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace JetKarmaBot.Models
{
    public partial class Award
    {
        public int AwardId { get; set; }
        public long ChatId { get; set; }
        public int FromId { get; set; }
        public int ToId { get; set; }
        public sbyte AwardTypeId { get; set; }
        public sbyte Amount { get; set; }
        public DateTime Date { get; set; }
        [ForeignKey("AwardTypeId")]
        public virtual AwardType AwardType { get; set; }
        [ForeignKey("ChatId")]
        public virtual Chat Chat { get; set; }
        [ForeignKey("FromId")]
        public virtual User From { get; set; }
        [ForeignKey("ToId")]
        public virtual User To { get; set; }
    }
}
