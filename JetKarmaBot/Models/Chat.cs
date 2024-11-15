namespace JetKarmaBot.Models;

public partial class Chat
{
    public Chat()
    {
        Awards = new HashSet<Award>();
    }

    public long ChatId { get; set; }
    public string Locale { get; set; } = "ru-RU";
    public bool IsAdministrator { get; set; }

    public virtual ICollection<Award> Awards { get; set; }
}
