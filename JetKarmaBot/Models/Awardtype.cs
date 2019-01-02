using System;
using System.Collections.Generic;

namespace JetKarmaBot.Models
{
    public partial class Awardtype
    {
        public Awardtype()
        {
            Award = new HashSet<Award>();
        }

        public sbyte Awardtypeid { get; set; }
        public string Commandname { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public string Description { get; set; }

        public ICollection<Award> Award { get; set; }
    }
}
