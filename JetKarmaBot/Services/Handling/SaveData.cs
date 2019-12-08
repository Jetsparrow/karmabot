using System;
using System.Threading.Tasks;
using JetKarmaBot.Models;
using Microsoft.EntityFrameworkCore;

namespace JetKarmaBot.Services.Handling
{
    public class SaveData : IRequestHandler
    {
        public async Task Handle(RequestContext ctx, Func<RequestContext, Task> next)
        {
            KarmaContext db = ctx.Database;
            await AddUserToDatabase(db, ctx.EventArgs.Message.From);
            if (ctx.EventArgs.Message.ReplyToMessage != null)
                await AddUserToDatabase(db, ctx.EventArgs.Message.ReplyToMessage.From);
            if (!await db.Chats.AnyAsync(x => x.ChatId == ctx.EventArgs.Message.Chat.Id))
                db.Chats.Add(new Models.Chat
                {
                    ChatId = ctx.EventArgs.Message.Chat.Id
                });
            await next(ctx);
        }
        private async Task AddUserToDatabase(KarmaContext db, Telegram.Bot.Types.User u)
        {
            string un;
            if (u.Username == null)
                un = u.FirstName + (u.LastName != null ? " " + u.LastName : "");
            else
                un = "@" + u.Username;
            if (!await db.Users.AnyAsync(x => x.UserId == u.Id))
                await db.Users.AddAsync(new Models.User { UserId = u.Id, Username = un });
            else
                (await db.Users.FindAsync(u.Id)).Username = un;
        }
    }
}