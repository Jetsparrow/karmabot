using System;
using System.Threading;
using System.Threading.Tasks;
using JetBotLib;
using JetKarmaBot.Models;
using Perfusion;

namespace JetKarmaBot.Services
{
    public class KarmaTimeoutManager : TimeoutManager
    {
        [Inject] Config cfg;
        [Inject] KarmaContextFactory Db;
        public override int SaveIntervalSeconds => cfg.Timeout.SaveIntervalSeconds;

        public override int DebtLimitSeconds => cfg.Timeout.DebtLimitSeconds;

        public override int GetCostSeconds(string name, bool status)
        {
            if (!cfg.Timeout.CommandCostsSeconds.TryGetValue(name + (status ? " (OK)" : "(ERR)"), out var costSeconds))
                if (!cfg.Timeout.CommandCostsSeconds.TryGetValue(name, out costSeconds))
                    if (!cfg.Timeout.CommandCostsSeconds.TryGetValue("Default", out costSeconds))
                    {
                        throw new LocalizationException("Default key not present");
                    }
            return costSeconds;
        }

        public override async Task Save(CancellationToken ct = default(CancellationToken))
        {
            using (KarmaContext db = Db.GetContext())
            {
                foreach (int uid in TimeoutCache.Keys)
                {
                    (await db.Users.FindAsync(new object[] { uid }, ct)).CooldownDate = TimeoutCache[uid].CooldownDate;
                }
                await db.SaveChangesAsync(ct);
            }
        }

        protected override async Task LoadUser(int uid, RequestContext ctx = null)
        {
            KarmaContext db = ctx.GetFeature<KarmaContext>();
            if (!TimeoutCache.ContainsKey(uid))
            {
                TimeoutCache[uid] = new TimeoutStats()
                {
                    CooldownDate = (await db.Users.FindAsync(uid))?.CooldownDate ?? DateTime.Now
                };
            }
        }
    }
}