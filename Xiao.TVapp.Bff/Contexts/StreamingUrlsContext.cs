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
    }
}

