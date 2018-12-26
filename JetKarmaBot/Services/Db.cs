using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using MySql.Data.MySqlClient;
using Perfusion;

namespace JetKarmaBot
{
    public class Db
    {
        Dictionary<long, Chat> m_Chats;
        public IReadOnlyDictionary<long, Chat> Chats => m_Chats;
        public void AddChat(Chat chat)
        {
            lock (m_SyncRoot)
                if (!m_Chats.ContainsKey(chat.ChatId))
                {
                    Conn.Execute(@"INSERT INTO chat
                            (chatid,locale)
                            VALUES
                            (@ChatId,@Locale)",
                        chat);
                    m_Chats.Add(chat.ChatId, chat);
                }
        }

        Dictionary<long, User> m_Users;
        public IReadOnlyDictionary<long, User> Users => m_Users;
        public void AddUser(User user)
        {
            lock (m_SyncRoot)
                if (!m_Users.ContainsKey(user.UserId))
                {
                    Conn.Execute(@"INSERT INTO user
                            (userid)
                            VALUES
                            (@UserId)",
                        user);
                    m_Users.Add(user.UserId, user);
                }
        }
        Dictionary<byte, AwardType> m_AwardTypes;
        public byte DefaultAwardTypeId { get; } = 1;
        public IReadOnlyDictionary<byte, AwardType> AwardTypes => m_AwardTypes;
        public IReadOnlyDictionary<string, AwardType> AwardTypesByCommandName { get; private set; }

        public int CountUserAwards(long userId, byte awardTypeId)
        {
            return Conn.QuerySingle<int?>
                (
                    "SELECT SUM(amount) FROM award WHERE toid = @userId AND awardtypeid = @awardTypeId",
                    new { userId, awardTypeId }
                ) ?? 0;
        }

        public void ChangeChatLocale(Chat chat, string locale)
        {
            lock (m_SyncRoot)
            {
                chat.Locale = locale;
                Conn.Execute(@"UPDATE chat
                            SET locale=@Locale
                            WHERE chatid=@ChatID",
                    chat);
            }
        }

        public struct UserAwardsReport
        {
            public int Amount { get; private set; }
            public byte AwardTypeId { get; private set; }
        }

        public IEnumerable<UserAwardsReport> CountAllUserAwards(long userId)
        {
            return Conn.Query<UserAwardsReport>
                (
                    @"SELECT SUM(amount) AS amount, t.awardtypeid
                FROM award a
                JOIN awardtype t on a.awardtypeid = t.awardtypeid
                WHERE toid = @userId
                GROUP BY awardtypeid;",
                    new { userId }
                );
        }

        public byte GetAwardTypeId(string name) => AwardTypesByCommandName.GetOrDefault(name)?.AwardTypeId ?? DefaultAwardTypeId;

        public bool AddAward(byte awardTypeId, long fromId, long toId, long chatId, int amount)
        {
            AddChat(new Chat() { ChatId = chatId });
            AddUser(new User() { UserId = fromId });
            AddUser(new User() { UserId = toId });

            int affected = Conn.ExecuteScalar<int>(
                @"INSERT INTO award
                (chatid, fromid, toid, awardtypeid, amount)
                VALUES
                (@chatId, @fromId, @toId, @awardTypeId, @amount)",
                new { awardTypeId, fromId, toId, chatId, amount });
            return affected == 1;
        }

        #region types
        public class Chat
        {
            public long ChatId { get; set; }
            public string Locale { get; set; }
        }

        public class User
        {
            public long UserId { get; set; }
        }

        public class AwardType
        {
            public byte AwardTypeId { get; set; }
            public string CommandName { get; set; }
            public string Name { get; set; }
            public string Symbol { get; set; }
            public string Description { get; set; }
        }

        public class Award
        {
            public int AwardId { get; set; }
            public byte AwardTypeId { get; set; }
            public long FromId { get; set; }
            public long ToId { get; set; }
            public long ChatId { get; set; }
            public byte Amount { get; set; }
        }

        #endregion

        #region service
        [Inject]
        public Db(Config cfg)
        {
            Log("Initializing...");
            Conn = new MySqlConnection(cfg.ConnectionString);
            Conn.ExecuteScalar("select 1");
            Load();
            Log("Initialized!");
        }

        object m_SyncRoot = new object();

        IDbConnection Conn { get; }
        void Load()
        {
            Log("Populating cache...");
            m_Chats = Conn.Query<Chat>("SELECT * FROM chat").ToDictionary(u => u.ChatId);
            m_Users = Conn.Query<User>("SELECT * FROM user").ToDictionary(s => s.UserId);
            m_AwardTypes = Conn.Query<AwardType>("SELECT * FROM awardtype").ToDictionary(c => c.AwardTypeId);
            AwardTypesByCommandName = m_AwardTypes.Values.ToDictionary(kvp => kvp.CommandName);
            Log("Cache populated!");
        }
        #endregion

        void Log(string Message) => Console.WriteLine($"[{nameof(Db)}]: {Message}");
    }
}