using System.ComponentModel.DataAnnotations.Schema;

namespace JetKarmaBot.Models;

public partial class User
{
    public User()
    {
        AwardsFrom = new HashSet<Award>();
        AwardsTo = new HashSet<Award>();
    }

    public long UserId { get; set; }
    public string Username { get; set; }
    public DateTime CooldownDate { get; set; }
    [InverseProperty("From")]
    public virtual ICollection<Award> AwardsFrom { get; set; }
    [InverseProperty("To")]
    public virtual ICollection<Award> AwardsTo { get; set; }
}
