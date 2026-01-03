using System.IO;
using Microsoft.EntityFrameworkCore;
using FlowClip.Models;

namespace FlowClip.Data;

/// <summary>
/// Entity Framework Core database context for FlowClip.
/// </summary>
public class FlowClipDbContext : DbContext
{
    /// <summary>
    /// Clipboard history entries.
    /// </summary>
    public DbSet<ClipboardEntry> ClipboardEntries { get; set; } = null!;

    /// <summary>
    /// Application settings.
    /// </summary>
    public DbSet<AppSettings> Settings { get; set; } = null!;

    /// <summary>
    /// Gets the path to the database file.
    /// </summary>
    public static string DatabasePath
    {
        get
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var flowClipPath = Path.Combine(appDataPath, "FlowClip");
            Directory.CreateDirectory(flowClipPath);
            return Path.Combine(flowClipPath, "flowclip.db");
        }
    }

    /// <summary>
    /// Gets the path to the images folder.
    /// </summary>
    public static string ImagesPath
    {
        get
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var imagesPath = Path.Combine(appDataPath, "FlowClip", "Images");
            Directory.CreateDirectory(imagesPath);
            return imagesPath;
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={DatabasePath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ClipboardEntry configuration
        modelBuilder.Entity<ClipboardEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.ContentType)
                  .HasConversion<string>()
                  .HasMaxLength(20);
            entity.Property(e => e.Preview).HasMaxLength(200);
            entity.Property(e => e.ColorHex).HasMaxLength(9);
            entity.Property(e => e.ContentHash).HasMaxLength(64);

            // Indexes for common queries
            entity.HasIndex(e => e.CopiedAt);
            entity.HasIndex(e => e.IsPinned);
            entity.HasIndex(e => e.ContentHash);
        });

        // AppSettings configuration
        modelBuilder.Entity<AppSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Theme).HasMaxLength(20).HasDefaultValue("Dark");
            entity.Property(e => e.HistoryLimit).HasDefaultValue(50);

            // Seed default settings
            entity.HasData(new AppSettings
            {
                Id = 1,
                HistoryLimit = 50,
                HotkeyModifiers = 5, // Ctrl + Shift
                HotkeyKey = 0x56,    // V
                RunOnStartup = false,
                Theme = "Dark",
                IsPanelExpanded = false
            });
        });
    }
}
