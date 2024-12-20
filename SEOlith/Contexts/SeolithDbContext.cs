using Microsoft.EntityFrameworkCore;
using SEOlith.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SEOlith.Contexts;

public class SeolithDbContext : DbContext
{
    public SeolithDbContext(DbContextOptions<SeolithDbContext> options)
    : base(options)
    {
    }

    public DbSet<SeoAudit> SeoAudits { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SeoAudit>(entity =>
        {
            entity.ToTable("seo_audits");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.WebsiteUrl)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.HeadingStructure)
                .HasColumnType("jsonb");

            entity.HasIndex(e => e.WebsiteUrl);
            entity.HasIndex(e => e.LastChecked);
        });
    }
}
