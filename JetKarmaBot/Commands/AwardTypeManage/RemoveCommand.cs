using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetKarmaBot.Models;
using JetKarmaBot.Services;
using Microsoft.EntityFrameworkCore;
using Perfusion;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JetKarmaBot.Commands.AwardTypeManage
{
    public class RemoveCommand : IChatCommand
    {
        public IReadOnlyCollection<string> Names => new[] { "remove" };

        public string Description => "Remove an award type.";

        public string DescriptionID => "jetkarmabot.at.remove.help";
        [Inject] TelegramBotClient Client { get; set; }
        [Inject] public KarmaContextFactory Db { get; set; }
        [Inject] public Localization Locale { get; set; }

        public IReadOnlyCollection<ChatCommandArgument> Arguments => new[] {
            new ChatCommandArgument() {
                Name="award",
                Required=true,
                Type=ChatCommandArgumentType.String,
                Description="The award to remove",
                DescriptionID="jetkarmabot.award.cmdnamehelp"
            }
        };

        public async Task<bool> Execute(ICommandRouter route, CommandString cmd, MessageEventArgs args)
        {
            using (var db = Db.GetContext())
            {
                var currentLocale = Locale[(await db.Chats.FindAsync(args.Message.Chat.Id)).Locale];

                ChatMember cm = await Client.GetChatMemberAsync(args.Message.Chat.Id, args.Message.From.Id);
                if (cm.Status != ChatMemberStatus.Administrator && cm.Status != ChatMemberStatus.Creator)
                {
                    await Client.SendTextMessageAsync(
                        args.Message.Chat.Id,
                        currentLocale["jetkarmabot.at.remove.errperm"],
                        replyToMessageId: args.Message.MessageId);
                    return true;
                }

                if (cmd.Parameters.Length < 1 || cmd.Parameters.Length > 1)
                {
                    await Client.SendTextMessageAsync(
                        args.Message.Chat.Id,
                        currentLocale["jetkarmabot.at.remove.errarg"],
                        replyToMessageId: args.Message.MessageId);
                    return true;
                }

                AwardType awardType = await db.AwardTypes.FirstAsync(x => x.CommandName == cmd.Parameters[0]);

                if (awardType == null)
                {
                    await Client.SendTextMessageAsync(
                        args.Message.Chat.Id,
                        currentLocale["jetkarmabot.at.remove.errinvcn"],
                        replyToMessageId: args.Message.MessageId);
                    return true;
                }

                await db.Database.ExecuteSqlCommandAsync("DELETE FROM award WHERE awardtypeid = @p0", awardType.AwardTypeId); // No delete by predicate in ef core yet..s

                db.AwardTypes.Remove(awardType);

                await db.SaveChangesAsync();

                await Client.SendTextMessageAsync(
                    args.Message.Chat.Id,
                    currentLocale[string.Format("jetkarmabot.at.remove.success", cmd.Parameters[0])],
                    replyToMessageId: args.Message.MessageId);
                return true;
            }
        }
    }
}