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
    public class SetParameterCommand : IChatCommand
    {
        public IReadOnlyCollection<string> Names => new[] { "set" };

        public string Description => "Set parameter of award type.";

        public string DescriptionID => "jetkarmabot.at.set.help";
        [Inject] TelegramBotClient Client { get; set; }
        [Inject] public KarmaContextFactory Db { get; set; }
        [Inject] public Localization Locale { get; set; }

        public IReadOnlyCollection<ChatCommandArgument> Arguments => new[] {
            new ChatCommandArgument() {
                Name="award",
                Required=true,
                Type=ChatCommandArgumentType.String,
                Description="The award to change",
                DescriptionID="jetkarmabot.at.set.awardhelp"
            },
            new ChatCommandArgument() {
                Name="param",
                Required=true,
                Type=ChatCommandArgumentType.String,
                Description="The parameter to change. Can be nomname, accname, symbol or desc",
                DescriptionID="jetkarmabot.at.set.paramhelp"
            },
            new ChatCommandArgument() {
                Name="value",
                Required=true,
                Type=ChatCommandArgumentType.String,
                Description="The value to change param to.",
                DescriptionID="jetkarmabot.at.set.valuehelp"
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
                        currentLocale["jetkarmabot.at.set.errperm"],
                        replyToMessageId: args.Message.MessageId);
                    return true;
                }

                if (cmd.Parameters.Length != 3)
                {
                    await Client.SendTextMessageAsync(
                        args.Message.Chat.Id,
                        currentLocale["jetkarmabot.at.set.errarg"],
                        replyToMessageId: args.Message.MessageId);
                    return true;
                }

                AwardType awardType = await db.AwardTypes.FirstOrDefaultAsync(x => x.CommandName == cmd.Parameters[0] && x.ChatId == args.Message.Chat.Id);

                if (awardType == null)
                {
                    await Client.SendTextMessageAsync(
                        args.Message.Chat.Id,
                        currentLocale["jetkarmabot.at.set.errinvcn"],
                        replyToMessageId: args.Message.MessageId);
                    return true;
                }

                switch (cmd.Parameters[1])
                {
                    case "nomname":
                        awardType.NominativeName = cmd.Parameters[2];
                        break;
                    case "accname":
                        awardType.AccusativeName = cmd.Parameters[2];
                        break;
                    case "symbol":
                        awardType.Symbol = cmd.Parameters[2];
                        break;
                    case "desc":
                        awardType.Description = cmd.Parameters[2];
                        break;
                    default:
                        await Client.SendTextMessageAsync(
                            args.Message.Chat.Id,
                            currentLocale["jetkarmabot.at.set.errinvparamname"],
                            replyToMessageId: args.Message.MessageId);
                        return true;
                }
                await db.SaveChangesAsync();
                await Client.SendTextMessageAsync(
                                args.Message.Chat.Id,
                                string.Format(currentLocale["jetkarmabot.at.set.success"], cmd.Parameters[0],
                                  currentLocale["jetkarmabot.at.set." + cmd.Parameters[2]]),
                                replyToMessageId: args.Message.MessageId);
                return true;
            }
        }
    }
}
