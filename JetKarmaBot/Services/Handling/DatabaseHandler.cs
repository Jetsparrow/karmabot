using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Perfusion;

namespace JetKarmaBot.Services.Handling
{
    public class DatabaseHandler : IRequestHandler
    {
        [Inject] private KarmaContextFactory Db;
        [Inject] private Localization Locale;
        public async Task Handle(RequestContext ctx, Func<RequestContext, Task> next)
        {
            using (var db = Db.GetContext())
            {
                ctx.AddFeature(db); // KarmaContext
                ctx.AddFeature(Locale[(await db.Chats.FindAsync(ctx.EventArgs.Message.Chat.Id))?.Locale ?? "ru-ru"]); // Locale
                await next(ctx);
                await db.SaveChangesAsync();
            }
        }
    }
}