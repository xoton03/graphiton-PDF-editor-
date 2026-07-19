using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Grafiton.Models;

namespace Grafiton.Services;

public class LibraryService : ILibraryService
{
    private static readonly string AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Grafiton");
    private static readonly string LibraryJsonPath = Path.Combine(AppDataFolder, "library.json");

    public async Task<ObservableCollection<LibraryEntry>> GetLibraryEntriesAsync()
    {
        if (!File.Exists(LibraryJsonPath))
            return new ObservableCollection<LibraryEntry>();

        try
        {
            string json = await File.ReadAllTextAsync(LibraryJsonPath);
            var list = JsonSerializer.Deserialize<ObservableCollection<LibraryEntry>>(json) ?? new ObservableCollection<LibraryEntry>();
            return list;
        }
        catch
        {
            return new ObservableCollection<LibraryEntry>();
        }
    }

    public async Task AddOrUpdateEntryAsync(string filePath, bool isFavorite = false)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) return;

        var entries = await GetLibraryEntriesAsync();
        var existing = entries.FirstOrDefault(e => e.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase));

        var fi = new FileInfo(filePath);

        if (existing != null)
        {
            existing.LastOpened = DateTime.Now;
            existing.FileSizeBytes = fi.Length;
            if (isFavorite) existing.IsFavorite = true;
        }
        else
        {
            entries.Add(new LibraryEntry
            {
                FilePath = filePath,
                FileName = fi.Name,
                IsFavorite = isFavorite,
                AddedDate = DateTime.Now,
                LastOpened = DateTime.Now,
                FileSizeBytes = fi.Length
            });
        }

        await SaveEntriesAsync(entries);
    }

    public async Task ToggleFavoriteAsync(string filePath)
    {
        var entries = await GetLibraryEntriesAsync();
        var entry = entries.FirstOrDefault(e => e.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase));
        if (entry != null)
        {
            entry.IsFavorite = !entry.IsFavorite;
            await SaveEntriesAsync(entries);
        }
    }

    public async Task RemoveEntryAsync(string filePath)
    {
        var entries = await GetLibraryEntriesAsync();
        var entry = entries.FirstOrDefault(e => e.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase));
        if (entry != null)
        {
            entries.Remove(entry);
            await SaveEntriesAsync(entries);
        }
    }

    private static async Task SaveEntriesAsync(ObservableCollection<LibraryEntry> entries)
    {
        Directory.CreateDirectory(AppDataFolder);
        string json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(LibraryJsonPath, json);
    }
}
