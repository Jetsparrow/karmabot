using JetKarmaBot.Models;
using Perfusion;

namespace JetKarmaBot.Services
{
    public class KarmaContextFactory
    {
        [Inject] Container C { get; set; }

        public KarmaContext GetContext() => C.GetInstance<KarmaContext>();
    }
}
