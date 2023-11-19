using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Perfusion;

namespace JetKarmaBot.Models
{
    [Transient]
    public partial class KarmaContext : DbContext
    {
        public KarmaContext()
        {
        }

        public KarmaContext(DbContextOptions<KarmaContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Award> Awards { get; set; }
        public virtual DbSet<AwardType> AwardTypes { get; set; }
        public virtual DbSet<Chat> Chats { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Award>(entity =>
            {
                entity.ToTable("award");

                entity.HasIndex(e => e.AwardId)
                    .HasDatabaseName("awardid_UNIQUE")
                    .IsUnique();

                entity.HasIndex(e => e.AwardTypeId)
                    .HasDatabaseName("fk_awardtype_idx");

                entity.HasIndex(e => e.ChatId)
                    .HasDatabaseName("fk_chat_idx");

                entity.HasIndex(e => e.FromId)
                    .HasDatabaseName("fk_from_idx");

                entity.HasIndex(e => e.ToId)
                    .HasDatabaseName("fk_to_idx");

                entity.Property(e => e.AwardId)
                    .HasColumnName("awardid")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Amount)
                    .HasColumnName("amount")
                    .HasColumnType("tinyint(3)")
                    .HasDefaultValueSql("'1'");

                entity.Property(e => e.AwardTypeId)
                    .HasColumnName("awardtypeid")
                    .HasColumnType("tinyint(3)");

                entity.Property(e => e.ChatId)
                    .HasColumnName("chatid")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Date)
                    .HasColumnName("date")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.FromId)
                    .HasColumnName("fromid")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.ToId)
                    .HasColumnName("toid")
                    .HasColumnType("bigint(20)");

                entity.HasOne(d => d.AwardType)
                    .WithMany(p => p.Awards)
                    .HasForeignKey(d => d.AwardTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_awardtype");

                entity.HasOne(d => d.Chat)
                    .WithMany(p => p.Awards)
                    .HasForeignKey(d => d.ChatId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_chat");

                entity.HasOne(d => d.From)
                    .WithMany(p => p.AwardsFrom)
                    .HasForeignKey(d => d.FromId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_from");

                entity.HasOne(d => d.To)
                    .WithMany(p => p.AwardsTo)
                    .HasForeignKey(d => d.ToId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_to");
            });

            modelBuilder.Entity<AwardType>(entity =>
            {
                entity.ToTable("awardtype");

                entity.HasIndex(e => e.AwardTypeId)
                    .HasDatabaseName("awardtypeid_UNIQUE")
                    .IsUnique();

                entity.HasIndex(e => e.CommandName)
                    .HasDatabaseName("commandname_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.AwardTypeId)
                    .HasColumnName("awardtypeid")
                    .HasColumnType("tinyint(3)");

                entity.Property(e => e.CommandName)
                    .IsRequired()
                    .HasColumnName("commandname")
                    .HasColumnType("varchar(35)");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnName("description")
                    .HasColumnType("text");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasColumnType("varchar(32)");

                entity.Property(e => e.Symbol)
                    .IsRequired()
                    .HasColumnName("symbol")
                    .HasColumnType("varchar(16)");
            });

            modelBuilder.Entity<Chat>(entity =>
            {
                entity.ToTable("chat");

                entity.Property(e => e.ChatId)
                    .HasColumnName("chatid")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Locale)
                    .IsRequired()
                    .HasColumnName("locale")
                    .HasColumnType("varchar(10)")
                    .HasDefaultValueSql("'ru-RU'");

                entity.Property(e => e.IsAdministrator)
                    .HasColumnName("isadministrator")
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValueSql("'0'");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user");

                entity.Property(e => e.UserId)
                    .HasColumnName("userid")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Username)
                    .HasColumnName("username")
                    .HasColumnType("varchar(45)");

                entity.Property(e => e.CooldownDate)
                    .HasColumnName("cooldowndate")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }
    }
}
