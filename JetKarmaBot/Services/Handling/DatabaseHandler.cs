using System;
using System.Threading.Tasks;
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
                ctx.Features.Add(db); // KarmaContext
                ctx.Features.Add(Locale[(await db.Chats.FindAsync(ctx.EventArgs.Message.Chat.Id)).Locale]); // Locale
                await next(ctx);
                await db.SaveChangesAsync();
            }
        }
    }
}