using System;
using System.Collections.Generic;
using System.Linq;
using Perfusion;
using JetKarmaBot.Services.Handling;
using NLog;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace JetKarmaBot.Commands
{
    class AwardCommand : IChatCommand
    {
        public IReadOnlyCollection<string> Names => new[] { "award", "revoke" };
        [Inject] private Logger log;

        public async Task<bool> Execute(RequestContext ctx)
        {
            var db = ctx.Database;
            var currentLocale = Locale[(await db.Chats.FindAsync(ctx.EventArgs.Message.Chat.Id)).Locale];

            var awarder = ctx.EventArgs.Message.From;
            string awardTypeText = null;
            int recipientId = default(int);
            foreach (string arg in ctx.Command.Parameters)
            {
                if (arg.StartsWith('@'))
                {
                    if (recipientId != default(int))
                    {
                        await ctx.SendMessage(currentLocale["jetkarmabot.award.errdup"]);
                        return false;
                    }
                    recipientId = await db.Users.Where(x => x.Username == arg).Select(x => x.UserId).FirstOrDefaultAsync();
                    if (recipientId == default(int))
                    {
                        await ctx.SendMessage(currentLocale["jetkarmabot.award.errbadusername"]);
                        return false;
                    }
                }
                else
                {
                    if (awardTypeText == null)
                        awardTypeText = arg;
                    else
                    {
                        await ctx.SendMessage(currentLocale["jetkarmabot.award.errdup"]);
                        return false;
                    }
                }
            }

            if (ctx.EventArgs.Message.ReplyToMessage != null && recipientId == default(int))
            {
                recipientId = ctx.EventArgs.Message.ReplyToMessage.From.Id;
            }

            if (recipientId == default(int))
            {
                await ctx.SendMessage(currentLocale["jetkarmabot.award.errawardnoreply"]);
                return false;
            }


            bool awarding = ctx.Command.Command == "award";

            if (awarder.Id == recipientId)
            {
                await ctx.SendMessage(currentLocale["jetkarmabot.award.errawardself"]);
                return false;
            }

            if (ctx.GetFeature<ChatCommandRouter.Feature>().Router.Me.Id == recipientId)
            {
                await ctx.SendMessage(awarding
                    ? currentLocale["jetkarmabot.award.errawardbot"]
                    : currentLocale["jetkarmabot.award.errrevokebot"]);
                return false;
            }

            var text = ctx.EventArgs.Message.Text;
            global::JetKarmaBot.Models.AwardType awardType = awardTypeText != null
                ? await db.AwardTypes.FirstAsync(at => at.CommandName == awardTypeText)
                : await db.AwardTypes.FindAsync((sbyte)1);

            var prevCount = await db.Awards
                .Where(aw => aw.ToId == recipientId && aw.AwardTypeId == awardType.AwardTypeId && aw.ChatId == ctx.EventArgs.Message.Chat.Id)
                .SumAsync(aw => aw.Amount);

            await db.Awards.AddAsync(new Models.Award()
            {
                AwardTypeId = awardType.AwardTypeId,
                Amount = (sbyte)(awarding ? 1 : -1),
                FromId = awarder.Id,
                ToId = recipientId,
                ChatId = ctx.EventArgs.Message.Chat.Id
            });

            var recUserName = (await db.Users.FindAsync(recipientId)).Username;

            log.Debug($"Awarded {(awarding ? 1 : -1)}{awardType.Symbol} to {recUserName}");

            string message = awarding
                ? string.Format(currentLocale["jetkarmabot.award.awardmessage"], getLocalizedName(awardType, currentLocale), recUserName)
                : string.Format(currentLocale["jetkarmabot.award.revokemessage"], getLocalizedName(awardType, currentLocale), recUserName);


            var response = message + "\n" + String.Format(currentLocale["jetkarmabot.award.statustext"], recUserName, prevCount + (awarding ? 1 : -1), awardType.Symbol);

            await ctx.SendMessage(response);
            return true;
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
    }
}
