using Microsoft.EntityFrameworkCore;

namespace domain.Models
{
    public partial class DatabaseContext : DbContext
    {
        public DatabaseContext()
        {
        }

        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Bans> Bans { get; set; }
        public virtual DbSet<Verifications> Verifications { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySQL("server=localhost;port=3306;user=root;password=root;database=database");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bans>(entity =>
            {
                entity.ToTable("bans");

                entity.HasIndex(e => e.Userid)
                    .HasName("uid");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.BannedOn)
                    .HasColumnName("banned_on")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.ByName)
                    .HasColumnName("by_name")
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.ByUserid)
                    .HasColumnName("by_userid")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.ExpiresOn).HasColumnName("expires_on");

                entity.Property(e => e.IsExpired)
                    .IsRequired()
                    .HasColumnName("is_expired")
                    .HasColumnType("enum('Y','N')")
                    .HasDefaultValueSql("'N'");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.Reason)
                    .HasColumnName("reason")
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.Userid)
                    .HasColumnName("userid")
                    .HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<Verifications>(entity =>
            {
                entity.HasKey(e => e.Userid)
                    .HasName("PRIMARY");

                entity.ToTable("verifications");

                entity.Property(e => e.Userid)
                    .HasColumnName("userid")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.DeletedOn).HasColumnName("deleted_on");

                entity.Property(e => e.ForumId)
                    .HasColumnName("forum_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ForumName)
                    .HasColumnName("forum_name")
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.VerifiedBy)
                    .HasColumnName("verified_by")
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.VerifiedOn)
                    .HasColumnName("verified_on")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasQueryFilter(e => e.DeletedOn == null);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
