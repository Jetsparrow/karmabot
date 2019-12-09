using System;
using System.Threading.Tasks;
using Perfusion;

namespace JetKarmaBot.Services.Handling
{
    public class DatabaseHandler : IRequestHandler
    {
        [Inject] private KarmaContextFactory Db;
        public async Task Handle(RequestContext ctx, Func<RequestContext, Task> next)
        {
            using (var db = Db.GetContext())
            {
                ctx.Features.Add(db);
                await next(ctx);
                await db.SaveChangesAsync();
            }
        }
    }
}