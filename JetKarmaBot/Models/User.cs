using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace JetKarmaBot.Models
{
    public partial class User
    {
        public User()
        {
            AwardsFrom = new HashSet<Award>();
            AwardsTo = new HashSet<Award>();
        }

        public int UserId { get; set; }
        public string Username { get; set; }
        [InverseProperty("From")]
        public virtual ICollection<Award> AwardsFrom { get; set; }
        [InverseProperty("To")]
        public virtual ICollection<Award> AwardsTo { get; set; }
    }
}
