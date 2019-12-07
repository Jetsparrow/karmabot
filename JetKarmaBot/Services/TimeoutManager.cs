using Perfusion;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using JetKarmaBot.Models;
using System.Threading;

namespace JetKarmaBot.Services
{
    [Singleton]
    public class TimeoutManager
    {
        public struct TimeoutStats
        {
            public DateTime CooldownDate;
            public bool TimeoutMessaged;
        }
        [Inject] private KarmaContextFactory Db;
        [Inject] private Config cfg;
        public Dictionary<int, TimeoutStats> TimeoutCache = new Dictionary<int, TimeoutStats>();
        public async Task ApplyCost(string name, int uid, KarmaContext db)
        {
            if (!cfg.Timeout.CommandCostsSeconds.TryGetValue(name, out var costSeconds))
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
        public enum CheckResult
        {
            NonLimited, Limited, LimitedSent
        }
        public async Task<CheckResult> Check(int uid, KarmaContext db)
        {
            await PopulateStats(uid, db);
            DateTime debtLimit = DateTime.Now.AddSeconds(cfg.Timeout.DebtLimitSeconds);
            return TimeoutCache[uid].CooldownDate < debtLimit
               ? CheckResult.NonLimited
               : (TimeoutCache[uid].TimeoutMessaged ? CheckResult.LimitedSent : CheckResult.Limited);
        }
        public async Task SetMessaged(int uid, KarmaContext db)
        {
            await PopulateStats(uid, db);
            TimeoutCache[uid] = new TimeoutStats() { TimeoutMessaged = true, CooldownDate = TimeoutCache[uid].CooldownDate };
        }
        private async Task PopulateStats(int uid, KarmaContext db)
        {
            if (!TimeoutCache.ContainsKey(uid))
            {
                TimeoutCache[uid] = new TimeoutStats()
                {
                    CooldownDate = (await db.Users.FindAsync(uid)).CooldownDate
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
    }
}