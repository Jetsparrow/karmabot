using JetKarmaBot.Models;

namespace JetKarmaBot.Services;

public class KarmaContextFactory
{
    [Inject] IContainer C { get; set; }

    public KarmaContext GetContext() => C.GetInstance<KarmaContext>();
}
