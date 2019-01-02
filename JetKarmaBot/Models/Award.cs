using System;
using System.Collections.Generic;

namespace JetKarmaBot.Models
{
    public partial class Award
    {
        public int Awardid { get; set; }
        public long Chatid { get; set; }
        public long Fromid { get; set; }
        public long Toid { get; set; }
        public sbyte Awardtypeid { get; set; }
        public sbyte Amount { get; set; }
        public DateTime Date { get; set; }

        public Awardtype Awardtype { get; set; }
        public Chat Chat { get; set; }
        public User From { get; set; }
        public User To { get; set; }
    }
}
