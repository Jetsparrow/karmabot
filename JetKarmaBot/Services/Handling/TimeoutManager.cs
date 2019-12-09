using Perfusion;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using JetKarmaBot.Models;
using System.Threading;
using System.Linq;

namespace JetKarmaBot.Services.Handling
{
    [Singleton]
    public class TimeoutManager : IRequestHandler
    {
        public class PreDbThrowout : IRequestHandler
        {
            public TimeoutManager Timeout { get; }
            public PreDbThrowout(TimeoutManager timeout)
            {
                Timeout = timeout;
            }

            public async Task Handle(RequestContext ctx, Func<RequestContext, Task> next)
            {
                int uid = ctx.EventArgs.Message.From.Id;
                if (Timeout.TimeoutCache.TryGetValue(uid, out var stats))
                {
                    DateTime debtLimit = DateTime.Now.AddSeconds(Timeout.cfg.Timeout.DebtLimitSeconds);
                    if (debtLimit < stats.CooldownDate && stats.TimeoutMessaged)
                        return;
                }
                await next(ctx);
            }
        }
        public struct TimeoutStats
        {
            public DateTime CooldownDate;
            public bool TimeoutMessaged;
        }
        [Inject] private KarmaContextFactory Db;
        [Inject] private Config cfg;
        [Inject] private Localization Locale;
        public Dictionary<int, TimeoutStats> TimeoutCache = new Dictionary<int, TimeoutStats>();
        private async Task ApplyCost(string name, bool succeded, int uid, KarmaContext db)
        {
            if (!cfg.Timeout.CommandCostsSeconds.TryGetValue(name + (succeded ? " (OK)" : "(ERR)"), out var costSeconds))
                if (!cfg.Timeout.CommandCostsSeconds.TryGetValue(name, out costSeconds))
                    if (!cfg.Timeout.CommandCostsSeconds.TryGetValue("Default", out costSeconds))
                    {
                        throw new LocalizationException("Default key not present");
                    }
            await PopulateStats(uid, db);
            DateTime debtLimit = DateTime.Now.AddSeconds(cfg.Timeout.DebtLimitSeconds);
            if (TimeoutCache[uid].CooldownDate >= debtLimit)
                //Programming error
                throw new NotImplementedException();
            TimeoutCache[uid] = new TimeoutStats()
            {
                CooldownDate = (TimeoutCache[uid].CooldownDate <= DateTime.Now ? DateTime.Now : TimeoutCache[uid].CooldownDate).AddSeconds(costSeconds),
                TimeoutMessaged = false
            };
            TimeoutCache[uid] = TimeoutCache[uid];
        }

        private async Task PopulateStats(int uid, KarmaContext db)
        {
            if (!TimeoutCache.ContainsKey(uid))
            {
                TimeoutCache[uid] = new TimeoutStats()
                {
                    CooldownDate = (await db.Users.FindAsync(uid))?.CooldownDate ?? DateTime.Now
                };
            }
        }
        public async Task Save(CancellationToken ct = default(CancellationToken))
        {
            using (KarmaContext db = Db.GetContext())
            {
                foreach (int i in TimeoutCache.Keys)
                {
                    (await db.Users.FindAsync(new object[] { i }, ct)).CooldownDate = TimeoutCache[i].CooldownDate;
                }
                await db.SaveChangesAsync(ct);
            }
        }
        public async Task SaveLoop(CancellationToken ct = default(CancellationToken))
        {
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(cfg.Timeout.SaveIntervalSeconds * 1000, ct);
                await Save(ct);
            }
        }

        public async Task Handle(RequestContext ctx, Func<RequestContext, Task> next)
        {
            int uid = ctx.EventArgs.Message.From.Id;
            KarmaContext db = ctx.GetFeature<KarmaContext>();
            await PopulateStats(uid, db);
            DateTime debtLimit = DateTime.Now.AddSeconds(cfg.Timeout.DebtLimitSeconds);
            if (debtLimit < TimeoutCache[uid].CooldownDate)
            {
                if (!TimeoutCache[uid].TimeoutMessaged)
                {
                    Locale currentLocale = ctx.GetFeature<Locale>();
                    await ctx.SendMessage(currentLocale["jetkarmabot.ratelimit"]);
                    TimeoutCache[uid] = new TimeoutStats() { TimeoutMessaged = true, CooldownDate = TimeoutCache[uid].CooldownDate };
                }
                return;
            }

            await next(ctx);

            var routerFeature = ctx.GetFeature<ChatCommandRouter.Feature>();
            await ApplyCost(getTypeName(routerFeature.CommandType), routerFeature.Succeded, uid, db);
        }
        private string getTypeName(Type t)
        {
            return (t.DeclaringType == null ? t.Namespace + "." + t.Name : getTypeName(t.DeclaringType) + "." + t.Name)
            + (t.GenericTypeArguments.Length > 0 ? "<" + string.Join(",", t.GenericTypeArguments.Select(getTypeName)) + ">" : "");
        }
    }
}