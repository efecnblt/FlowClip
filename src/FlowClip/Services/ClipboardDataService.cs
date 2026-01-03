using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FlowClip.Data;
using FlowClip.Helpers;
using FlowClip.Models;
using FlowClip.Services.Interfaces;

namespace FlowClip.Services;

/// <summary>
/// Service for managing clipboard history data persistence.
/// </summary>
public class ClipboardDataService : IClipboardDataService
{
    private readonly IServiceProvider _serviceProvider;

    public ClipboardDataService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private FlowClipDbContext CreateContext()
    {
        var scope = _serviceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<FlowClipDbContext>();
    }

    /// <inheritdoc/>
    public async Task<ClipboardEntry> AddEntryAsync(ClipboardEntry entry)
    {
        using var context = CreateContext();

        entry.CopiedAt = DateTime.UtcNow;
        context.ClipboardEntries.Add(entry);
        await context.SaveChangesAsync();

        return entry;
    }

    /// <inheritdoc/>
    public async Task<List<ClipboardEntry>> GetEntriesAsync(int limit = 50)
    {
        using var context = CreateContext();

        return await context.ClipboardEntries
            .OrderByDescending(e => e.IsPinned)
            .ThenByDescending(e => e.CopiedAt)
            .Take(limit)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task DeleteEntryAsync(int id)
    {
        using var context = CreateContext();

        var entry = await context.ClipboardEntries.FindAsync(id);
        if (entry != null)
        {
            // Delete associated image file if exists
            if (entry.ContentType == ClipboardContentType.Image)
            {
                ImageHelper.DeleteImage(entry.ImagePath);
            }

            context.ClipboardEntries.Remove(entry);
            await context.SaveChangesAsync();
        }
    }

    /// <inheritdoc/>
    public async Task ClearAllAsync()
    {
        using var context = CreateContext();

        // Get all non-pinned entries
        var entries = await context.ClipboardEntries
            .Where(e => !e.IsPinned)
            .ToListAsync();

        // Delete associated image files
        foreach (var entry in entries.Where(e => e.ContentType == ClipboardContentType.Image))
        {
            ImageHelper.DeleteImage(entry.ImagePath);
        }

        context.ClipboardEntries.RemoveRange(entries);
        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task TogglePinAsync(int id)
    {
        using var context = CreateContext();

        var entry = await context.ClipboardEntries.FindAsync(id);
        if (entry != null)
        {
            entry.IsPinned = !entry.IsPinned;
            await context.SaveChangesAsync();
        }
    }

    /// <inheritdoc/>
    public async Task<ClipboardEntry?> FindByHashAsync(string hash)
    {
        using var context = CreateContext();

        return await context.ClipboardEntries
            .FirstOrDefaultAsync(e => e.ContentHash == hash);
    }

    /// <inheritdoc/>
    public async Task MoveToTopAsync(int id)
    {
        using var context = CreateContext();

        var entry = await context.ClipboardEntries.FindAsync(id);
        if (entry != null)
        {
            entry.CopiedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }

    /// <inheritdoc/>
    public async Task EnforceHistoryLimitAsync(int limit)
    {
        using var context = CreateContext();

        // Get entries that exceed the limit (excluding pinned)
        var entriesToDelete = await context.ClipboardEntries
            .Where(e => !e.IsPinned)
            .OrderByDescending(e => e.CopiedAt)
            .Skip(limit)
            .ToListAsync();

        if (entriesToDelete.Count == 0)
            return;

        // Delete associated image files
        foreach (var entry in entriesToDelete.Where(e => e.ContentType == ClipboardContentType.Image))
        {
            ImageHelper.DeleteImage(entry.ImagePath);
        }

        context.ClipboardEntries.RemoveRange(entriesToDelete);
        await context.SaveChangesAsync();
    }
}
