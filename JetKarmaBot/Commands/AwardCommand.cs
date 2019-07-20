using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Perfusion;
using JetKarmaBot.Services;
using NLog;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace JetKarmaBot.Commands
{
    class AwardCommand : IChatCommand
    {
        public IReadOnlyCollection<string> Names => new[] { "award", "revoke" };
        [Inject]
        private Logger log;

        public async Task<bool> Execute(CommandString cmd, MessageEventArgs args)
        {
            using (var db = Db.GetContext())
            {
                var currentLocale = Locale[(await db.Chats.FindAsync(args.Message.Chat.Id)).Locale];
                if (args.Message.ReplyToMessage == null)
                {
                    await Client.SendTextMessageAsync(args.Message.Chat.Id, currentLocale["jetkarmabot.award.errawardnoreply"]);
                    return true;
                }

                var awarder = args.Message.From;
                var recipient = args.Message.ReplyToMessage.From;

                bool awarding = cmd.Command == "award";

                if (awarder.Id == recipient.Id)
                {
                    await Client.SendTextMessageAsync(
                        args.Message.Chat.Id,
                        currentLocale["jetkarmabot.award.errawardself"],
                        replyToMessageId: args.Message.MessageId);
                    return true;
                }

                if (Me.Id == recipient.Id)
                {
                    await Client.SendTextMessageAsync(
                        args.Message.Chat.Id,
                        awarding
                        ? currentLocale["jetkarmabot.award.errawardbot"]
                        : currentLocale["jetkarmabot.award.errrevokebot"],
                        replyToMessageId: args.Message.MessageId);
                    return true;
                }

                var text = args.Message.Text;
                var awardTypeText = cmd.Parameters.FirstOrDefault();
                global::JetKarmaBot.Models.AwardType awardType = awardTypeText != null
                    ? await db.AwardTypes.FirstAsync(at => at.CommandName == awardTypeText)
                    : await db.AwardTypes.FindAsync((sbyte)1);
                DateTime cutoff = DateTime.Now - TimeSpan.FromMinutes(5);
                if (await db.Awards.Where(x => x.Date > cutoff && x.FromId == awarder.Id).CountAsync() >= 10)
                {
                    await Client.SendTextMessageAsync(
                    args.Message.Chat.Id,
                    currentLocale["jetkarmabot.award.ratelimit"],
                    replyToMessageId: args.Message.MessageId);
                    return true;
                }
                await db.Awards.AddAsync(new Models.Award()
                {
                    AwardTypeId = awardType.AwardTypeId,
                    Amount = (sbyte)(awarding ? 1 : -1),
                    FromId = awarder.Id,
                    ToId = recipient.Id,
                    ChatId = args.Message.Chat.Id
                });
                log.Debug($"Awarded {(awarding ? 1 : -1)}{awardType.Symbol} to {recipient.Username}");
                await db.SaveChangesAsync();

                var recUserName = (await db.Users.FindAsync(recipient.Id)).Username;

                string message = awarding
                    ? string.Format(currentLocale["jetkarmabot.award.awardmessage"], getLocalizedName(awardType, currentLocale), recUserName)
                    : string.Format(currentLocale["jetkarmabot.award.revokemessage"], getLocalizedName(awardType, currentLocale), recUserName);

                var currentCount = await db.Awards
                    .Where(aw => aw.ToId == recipient.Id && aw.AwardTypeId == awardType.AwardTypeId && aw.ChatId == args.Message.Chat.Id)
                    .SumAsync(aw => aw.Amount);

                var response = message + "\n" + String.Format(currentLocale["jetkarmabot.award.statustext"], recUserName, currentCount, awardType.Symbol);

                await Client.SendTextMessageAsync(
                    args.Message.Chat.Id,
                    response,
                    replyToMessageId: args.Message.MessageId);
                return true;
            }
        }

        private string getLocalizedName(global::JetKarmaBot.Models.AwardType awardType, Locale loc)
        {
            if (loc.ContainsKey($"jetkarmabot.awardtypes.accusative.{awardType.CommandName}"))
            {
                return loc[$"jetkarmabot.awardtypes.accusative.{awardType.CommandName}"];
            }
            else
            {
                return awardType.Name;
            }
        }

        [Inject] KarmaContextFactory Db { get; set; }
        [Inject] TelegramBotClient Client { get; set; }
        [Inject] Localization Locale { get; set; }
        User Me { get; }

        public string Description => "Awards/revokes an award to a user.";
        public string DescriptionID => "jetkarmabot.award.help";

        public IReadOnlyCollection<ChatCommandArgument> Arguments => new ChatCommandArgument[] {
            new ChatCommandArgument() {
                Name="awardtype",
                Required=false,
                Type=ChatCommandArgumentType.String,
                Description="The award to grant to/strip of the specified user",
                DescriptionID="jetkarmabot.award.awardtypehelp"
            }
        };

        public AwardCommand(User me)
        {
            Me = me;
        }
    }
}
