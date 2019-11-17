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

                string awardTypeText = null;
                int recipientId = default(int);
                foreach (string arg in cmd.Parameters)
                {
                    if (arg.StartsWith('@'))
                    {
                        if (recipientId != default(int))
                        {
                            await Client.SendTextMessageAsync(args.Message.Chat.Id, currentLocale["jetkarmabot.award.errdup"]);
                            return true;
                        }
                        recipientId = await db.Users.Where(x => x.Username == arg).Select(x => x.UserId).FirstOrDefaultAsync();
                        if (recipientId == default(int))
                        {
                            await Client.SendTextMessageAsync(args.Message.Chat.Id, currentLocale["jetkarmabot.award.errbadusername"]);
                            return true;
                        }
                    }
                    else
                    {
                        if (awardTypeText == null)
                            awardTypeText = arg;
                        else
                        {
                            await Client.SendTextMessageAsync(args.Message.Chat.Id, currentLocale["jetkarmabot.award.errdup"]);
                            return true;
                        }
                    }
                }

                if (args.Message.ReplyToMessage != null && recipientId == default(int))
                {
                    recipientId = args.Message.ReplyToMessage.From.Id;
                }

                if (recipientId == default(int))
                {
                    await Client.SendTextMessageAsync(args.Message.Chat.Id, currentLocale["jetkarmabot.award.errawardnoreply"]);
                    return true;
                }

                var awarder = args.Message.From;

                bool awarding = cmd.Command == "award";

                if (awarder.Id == recipientId)
                {
                    await Client.SendTextMessageAsync(
                        args.Message.Chat.Id,
                        currentLocale["jetkarmabot.award.errawardself"],
                        replyToMessageId: args.Message.MessageId);
                    return true;
                }

                if (Router.Me.Id == recipientId)
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
                string awardTypeAcc, awardTypeSym;
                sbyte? awardTypeId;
                if (awardTypeText == null || awardTypeText == "star")
                {
                    awardTypeAcc = currentLocale["jetkarmabot.star.accusative"];
                    awardTypeSym = "★";
                    awardTypeId = null;
                }
                else
                {
                    var awardType = await db.AwardTypes.FirstAsync(at => at.CommandName == awardTypeText && at.ChatId == args.Message.Chat.Id);
                    awardTypeAcc = awardType.AccusativeName;
                    awardTypeSym = awardType.Symbol;
                    awardTypeId = awardType.AwardTypeId;
                }

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
                    AwardTypeId = awardTypeId,
                    Amount = (sbyte)(awarding ? 1 : -1),
                    FromId = awarder.Id,
                    ToId = recipientId,
                    ChatId = args.Message.Chat.Id
                });
                await db.SaveChangesAsync();

                var recUserName = (await db.Users.FindAsync(recipientId)).Username;

                log.Debug($"Awarded {(awarding ? 1 : -1)}{awardTypeSym} to {recUserName}");

                string message = string.Format(currentLocale[
                  awarding ? "jetkarmabot.award.awardmessage" : "jetkarmabot.award.revokemessage"], awardTypeAcc, recUserName);

                var currentCount = await db.Awards
                    .Where(aw => aw.ToId == recipientId && aw.AwardTypeId == awardTypeId && aw.ChatId == args.Message.Chat.Id)
                    .SumAsync(aw => aw.Amount);

                var response = message + "\n" + String.Format(currentLocale["jetkarmabot.award.statustext"], recUserName, currentCount, awardTypeSym);

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
                return awardType.NominativeName;
            }
        }

        [Inject] KarmaContextFactory Db { get; set; }
        [Inject] TelegramBotClient Client { get; set; }
        [Inject] Localization Locale { get; set; }

        public string Description => "Awards/revokes an award to a user.";
        public string DescriptionID => "jetkarmabot.award.help";

        public IReadOnlyCollection<ChatCommandArgument> Arguments => new ChatCommandArgument[] {
            new ChatCommandArgument() {
                Name="awardtype",
                Required=false,
                Type=ChatCommandArgumentType.String,
                Description="The award to grant to/strip of the specified user",
                DescriptionID="jetkarmabot.award.awardtypehelp"
            },
            new ChatCommandArgument() {
                Name="to",
                Required=false,
                Type=ChatCommandArgumentType.String,
                Description="The user to award it to.",
                DescriptionID="jetkarmabot.award.tohelp"
            }
        };

        public ICommandRouter Router { get; set; }
    }
}
