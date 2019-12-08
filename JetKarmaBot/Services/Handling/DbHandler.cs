using System;
using System.Threading.Tasks;
using JetKarmaBot.Models;
using Microsoft.EntityFrameworkCore;

namespace JetKarmaBot.Services.Handling
{
    public class DbHandler : IRequestHandler
    {
        [Flags]
        public enum SaveType
        {
            From = 1 << 0,
            To = 1 << 1,
            Chat = 1 << 2,
            LateDbChanges = 1 << 3
        }
        private SaveType type;
        public DbHandler(SaveType type)
        {
            this.type = type;
        }
        public async Task Handle(RequestContext ctx, Func<RequestContext, Task> next)
        {
            KarmaContext db = ctx.Database;
            if (type.HasFlag(SaveType.From))
                await AddUserToDatabase(db, ctx.EventArgs.Message.From);
            if (type.HasFlag(SaveType.To))
                if (ctx.EventArgs.Message.ReplyToMessage != null)
                    await AddUserToDatabase(db, ctx.EventArgs.Message.ReplyToMessage.From);
            if (type.HasFlag(SaveType.Chat))
                if (!await db.Chats.AnyAsync(x => x.ChatId == ctx.EventArgs.Message.Chat.Id))
                    db.Chats.Add(new Models.Chat
                    {
                        ChatId = ctx.EventArgs.Message.Chat.Id
                    });
            await next(ctx);
            if (type.HasFlag(SaveType.LateDbChanges))
                await db.SaveChangesAsync();
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