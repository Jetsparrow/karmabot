using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace JetKarmaBot.Commands
{
    public class DefineCommand : IChatCommand
    {
        Dictionary<string, string> m_Definitions = new Dictionary<string, string>()
        {
            { "AbstractSingletonProxyFactoryBean", "*Convenient* superclass for FactoryBean types that produce singleton-scoped proxy objects." }
        };

        ITelegramBotClient m_client;
        public DefineCommand(ITelegramBotClient client)
        {
            m_client = client;
        }

        public IReadOnlyCollection<string> Names => new[] {"define" };

        public bool Execute(object sender, MessageEventArgs messageEventArgs)
        {
            var commandTerms = messageEventArgs.Message.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var chatId = messageEventArgs.Message.Chat.Id;
            foreach (var term in commandTerms.Skip(1))
            {
                if (m_Definitions.ContainsKey(term))
                {
                    m_client.SendTextMessageAsync(chatId, m_Definitions[term], parseMode: ParseMode.Markdown);
                    return true;
                }
            }
            m_client.SendTextMessageAsync(chatId, "idk lol");
            return false;
        }
    }
}
