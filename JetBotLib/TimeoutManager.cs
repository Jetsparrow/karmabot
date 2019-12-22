using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace JetBotLib
{
    public abstract class TimeoutManager : IRequestHandler
    {
        public class Feature
        {
            public double Multiplier = 1;
        }
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
                    DateTime debtLimit = DateTime.Now.AddSeconds(Timeout.DebtLimitSeconds);
                    if (debtLimit < stats.CooldownDate && stats.TimeoutMessaged)
                        return;
                }
                await next(ctx);
            }
        }
        public class TimeoutStats
        {
            public DateTime CooldownDate;
            public bool TimeoutMessaged;
            protected Dictionary<Type, object> features = new Dictionary<Type, object>();
            public IReadOnlyDictionary<Type, object> Features => features;
            public T GetFeature<T>()
            {
                if (!Features.ContainsKey(typeof(T)))
                    features.Add(typeof(T), Activator.CreateInstance(typeof(T)));
                return (T)Features[typeof(T)];
            }
        }
        public Dictionary<int, TimeoutStats> TimeoutCache = new Dictionary<int, TimeoutStats>();

        public abstract Task Save(CancellationToken ct = default(CancellationToken));
        protected abstract Task LoadUser(int uid, RequestContext ctx = null);
        public abstract int SaveIntervalSeconds { get; }
        public abstract int DebtLimitSeconds { get; }
        public abstract int GetCostSeconds(string name, bool status);

        public async Task SaveLoop(CancellationToken ct = default(CancellationToken))
        {
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(SaveIntervalSeconds * 1000, ct);
                await Save(ct);
            }
        }

        public async Task Handle(RequestContext ctx, Func<RequestContext, Task> next)
        {
            int uid = ctx.EventArgs.Message.From.Id;
            await LoadUser(uid, ctx);
            DateTime debtLimit = DateTime.Now.AddSeconds(DebtLimitSeconds);
            if (debtLimit < TimeoutCache[uid].CooldownDate)
            {
                if (!TimeoutCache[uid].TimeoutMessaged)
                {
                    Locale currentLocale = ctx.GetFeature<Locale>();
                    await ctx.SendMessage(currentLocale["jetkarmabot.ratelimit"]);
                    TimeoutCache[uid].TimeoutMessaged = true;
                }
                return;
            }
            Feature feature = new Feature();
            ctx.Features.Add(feature);

            await next(ctx);

            var routerFeature = ctx.GetFeature<ChatCommandRouter.Feature>();
            if (feature.Multiplier == 0) return;
            int cost = GetCostSeconds(getTypeName(routerFeature.CommandType), routerFeature.Succeded);
            if (TimeoutCache[uid].CooldownDate < DateTime.Now) TimeoutCache[uid].CooldownDate = DateTime.Now;
            TimeoutCache[uid].CooldownDate = TimeoutCache[uid].CooldownDate.AddSeconds(cost);
            TimeoutCache[uid].TimeoutMessaged = false;
        }
        private string getTypeName(Type t)
        {
            return (t.DeclaringType == null ? t.Namespace + "." + t.Name : getTypeName(t.DeclaringType) + "." + t.Name)
            + (t.GenericTypeArguments.Length > 0 ? "<" + string.Join(",", t.GenericTypeArguments.Select(getTypeName)) + ">" : "");
        }
    }
}