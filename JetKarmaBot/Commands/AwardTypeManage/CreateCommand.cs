using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetKarmaBot.Models;
using JetKarmaBot.Services;
using Perfusion;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JetKarmaBot.Commands.AwardTypeManage
{
    public class CreateCommand : IChatCommand
    {
        public IReadOnlyCollection<string> Names => new[] { "create" };

        public string Description => "Create an award type.";

        public string DescriptionID => "jetkarmabot.at.create.help";
        [Inject] TelegramBotClient Client { get; set; }
        [Inject] public KarmaContextFactory Db { get; set; }
        [Inject] public Localization Locale { get; set; }

        public IReadOnlyCollection<ChatCommandArgument> Arguments => new[] {
            new ChatCommandArgument() {
                Name="cmdname",
                Required=true,
                Type=ChatCommandArgumentType.String,
                Description="Name used when awarding to user.",
                DescriptionID="jetkarmabot.at.create.cmdnamehelp"
            },
            new ChatCommandArgument() {
                Name="nomname",
                Required=true,
                Type=ChatCommandArgumentType.String,
                Description="Name used when in currencies list.",
                DescriptionID="jetkarmabot.at.create.nomnamehelp"
            },
            new ChatCommandArgument() {
                Name="accname",
                Required=true,
                Type=ChatCommandArgumentType.String,
                Description="Name used in award message",
                DescriptionID="jetkarmabot.at.create.accnamehelp"
            },
            new ChatCommandArgument() {
                Name="symbol",
                Required=true,
                Type=ChatCommandArgumentType.String,
                Description="Symbol of the award type.",
                DescriptionID="jetkarmabot.at.create.symbolhelp"
            },
            new ChatCommandArgument() {
                Name="desc",
                Required=false,
                Type=ChatCommandArgumentType.String,
                Description="Description of the award type.",
                DescriptionID="jetkarmabot.at.create.symbolhelp"
            }
        };

        public ICommandRouter Router { get; set; }

        public async Task<bool> Execute(CommandString cmd, MessageEventArgs args)
        {
            using (var db = Db.GetContext())
            {
                var currentLocale = Locale[(await db.Chats.FindAsync(args.Message.Chat.Id)).Locale];

                ChatMember cm = await Client.GetChatMemberAsync(args.Message.Chat.Id, args.Message.From.Id);
                if (cm.Status != ChatMemberStatus.Administrator && cm.Status != ChatMemberStatus.Creator)
                {
                    await Client.SendTextMessageAsync(
                        args.Message.Chat.Id,
                        currentLocale["jetkarmabot.at.create.errperm"],
                        replyToMessageId: args.Message.MessageId);
                    return true;
                }

                if (cmd.Parameters.Length < 4 || cmd.Parameters.Length > 5)
                {
                    await Client.SendTextMessageAsync(
                        args.Message.Chat.Id,
                        currentLocale["jetkarmabot.at.create.errarg"],
                        replyToMessageId: args.Message.MessageId);
                    return true;
                }

                string cmdname = cmd.Parameters[0];
                string nomname = cmd.Parameters[1];
                string accname = cmd.Parameters[2];
                string symbol = cmd.Parameters[3];
                string desc = cmd.Parameters.Length == 4 ? "" : cmd.Parameters[4];


                await db.AwardTypes.AddAsync(new AwardType()
                {
                    CommandName = cmdname,
                    NominativeName = nomname,
                    AccusativeName = accname,
                    Symbol = symbol,
                    Description = desc,
                    ChatId = args.Message.Chat.Id
                });
                await db.SaveChangesAsync();

                await Client.SendTextMessageAsync(
                    args.Message.Chat.Id,
                    currentLocale[string.Format("jetkarmabot.at.create.success", cmdname)],
                    replyToMessageId: args.Message.MessageId);

                return true;
            }
        }
    }
}