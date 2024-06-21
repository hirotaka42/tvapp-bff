using System;
using Microsoft.EntityFrameworkCore;
using Xiao.TVapp.Bff.Models;

namespace Xiao.TVapp.Bff.Contexts
{
    public class StreamingUrlsContext : DbContext
    {
        public StreamingUrlsContext(DbContextOptions<StreamingUrlsContext> options)
            : base(options)
        {
            Database.Migrate();
        }

        public DbSet<StreamingUrls> StreamingUrls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StreamingUrls>().HasKey(s => s.Id);
            modelBuilder.Entity<StreamingUrls>().HasIndex(s => s.EpisodeId);
        }

        public override int SaveChanges()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is StreamingUrls && (
                        e.State == EntityState.Added
                        || e.State == EntityState.Modified));

            DateTime currentTime = DateTime.UtcNow;

            foreach (var entityEntry in entries)
            {
                ((StreamingUrls)entityEntry.Entity).UpdatedAt = currentTime;

                // 新たにEntityが追加された場合はCreatedAtに現在時刻を設定
                if (entityEntry.State == EntityState.Added)
                {
                    ((StreamingUrls)entityEntry.Entity).CreatedAt = currentTime;
                }
            }

            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is StreamingUrls && (
                        e.State == EntityState.Added
                        || e.State == EntityState.Modified));

            DateTime currentTime = DateTime.UtcNow;

            foreach (var entityEntry in entries)
            {
                ((StreamingUrls)entityEntry.Entity).UpdatedAt = currentTime;

                // 新たにEntityが追加された場合はCreatedAtに現在時刻を設定
                if (entityEntry.State == EntityState.Added)
                {
                    ((StreamingUrls)entityEntry.Entity).CreatedAt = currentTime;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}

