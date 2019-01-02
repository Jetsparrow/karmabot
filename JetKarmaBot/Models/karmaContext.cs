using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JetKarmaBot.Models
{
    public partial class KarmaContext : DbContext
    {
        public KarmaContext()
        {
        }

        public KarmaContext(DbContextOptions<KarmaContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Award> Award { get; set; }
        public virtual DbSet<Awardtype> Awardtype { get; set; }
        public virtual DbSet<Chat> Chat { get; set; }
        public virtual DbSet<User> User { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Award>(entity =>
            {
                entity.ToTable("award");

                entity.HasIndex(e => e.Awardid)
                    .HasName("awardid_UNIQUE")
                    .IsUnique();

                entity.HasIndex(e => e.Awardtypeid)
                    .HasName("fk_awardtype_idx");

                entity.HasIndex(e => e.Chatid)
                    .HasName("fk_chat_idx");

                entity.HasIndex(e => e.Fromid)
                    .HasName("fk_from_idx");

                entity.HasIndex(e => e.Toid)
                    .HasName("fk_to_idx");

                entity.Property(e => e.Awardid)
                    .HasColumnName("awardid")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Amount)
                    .HasColumnName("amount")
                    .HasColumnType("tinyint(3)")
                    .HasDefaultValueSql("'1'");

                entity.Property(e => e.Awardtypeid)
                    .HasColumnName("awardtypeid")
                    .HasColumnType("tinyint(3)");

                entity.Property(e => e.Chatid)
                    .HasColumnName("chatid")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Date)
                    .HasColumnName("date")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.Fromid)
                    .HasColumnName("fromid")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Toid)
                    .HasColumnName("toid")
                    .HasColumnType("bigint(20)");

                entity.HasOne(d => d.Awardtype)
                    .WithMany(p => p.Award)
                    .HasForeignKey(d => d.Awardtypeid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_awardtype");

                entity.HasOne(d => d.Chat)
                    .WithMany(p => p.Award)
                    .HasForeignKey(d => d.Chatid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_chat");

                entity.HasOne(d => d.From)
                    .WithMany(p => p.AwardFrom)
                    .HasForeignKey(d => d.Fromid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_from");

                entity.HasOne(d => d.To)
                    .WithMany(p => p.AwardTo)
                    .HasForeignKey(d => d.Toid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_to");
            });

            modelBuilder.Entity<Awardtype>(entity =>
            {
                entity.ToTable("awardtype");

                entity.HasIndex(e => e.Awardtypeid)
                    .HasName("awardtypeid_UNIQUE")
                    .IsUnique();

                entity.HasIndex(e => e.Commandname)
                    .HasName("commandname_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.Awardtypeid)
                    .HasColumnName("awardtypeid")
                    .HasColumnType("tinyint(3)");

                entity.Property(e => e.Commandname)
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

                entity.Property(e => e.Chatid)
                    .HasColumnName("chatid")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Locale)
                    .IsRequired()
                    .HasColumnName("locale")
                    .HasColumnType("varchar(10)")
                    .HasDefaultValueSql("'ru-RU'");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user");

                entity.Property(e => e.Userid)
                    .HasColumnName("userid")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Username)
                    .HasColumnName("username")
                    .HasColumnType("varchar(45)");
            });
        }
    }
}
