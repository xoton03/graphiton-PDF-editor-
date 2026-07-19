using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Win32;
using Grafiton.Models;

namespace Grafiton.Services;

public class FileService : IFileService
{
    private static readonly string AppDataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Grafiton"
    );

    private static readonly string RecentFilesPath = Path.Combine(AppDataFolder, "recent_files.json");
    private const int MaxRecentFiles = 15;

    public FileService()
    {
        if (!Directory.Exists(AppDataFolder))
        {
            Directory.CreateDirectory(AppDataFolder);
        }
    }

    public List<RecentFile> GetRecentFiles()
    {
        try
        {
            if (!File.Exists(RecentFilesPath))
                return new List<RecentFile>();

            string json = File.ReadAllText(RecentFilesPath);
            var list = JsonSerializer.Deserialize<List<RecentFile>>(json);
            return list?.Where(f => File.Exists(f.FilePath))
                       .OrderByDescending(f => f.LastOpened)
                       .ToList() ?? new List<RecentFile>();
        }
        catch
        {
            return new List<RecentFile>();
        }
    }

    public void AddRecentFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return;

        try
        {
            var list = GetRecentFiles();
            list.RemoveAll(f => string.Equals(f.FilePath, filePath, StringComparison.OrdinalIgnoreCase));

            list.Insert(0, new RecentFile
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                LastOpened = DateTime.Now
            });

            if (list.Count > MaxRecentFiles)
            {
                list = list.Take(MaxRecentFiles).ToList();
            }

            string json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
            File.ReadAllText(RecentFilesPath); // test read
            File.WriteAllText(RecentFilesPath, json);
        }
        catch
        {
            try
            {
                string json = JsonSerializer.Serialize(new List<RecentFile>
                {
                    new RecentFile
                    {
                        FilePath = filePath,
                        FileName = Path.GetFileName(filePath),
                        LastOpened = DateTime.Now
                    }
                });
                File.WriteAllText(RecentFilesPath, json);
            }
            catch { }
        }
    }

    public void RemoveRecentFile(string filePath)
    {
        try
        {
            var list = GetRecentFiles();
            list.RemoveAll(f => string.Equals(f.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
            string json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(RecentFilesPath, json);
        }
        catch { }
    }

    public void ClearRecentFiles()
    {
        try
        {
            if (File.Exists(RecentFilesPath))
            {
                File.Delete(RecentFilesPath);
            }
        }
        catch { }
    }

    public string? OpenFileDialog()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Fichiers PDF (*.pdf)|*.pdf|Tous les fichiers (*.*)|*.*",
            Title = "Ouvrir un document PDF",
            Multiselect = false
        };

        bool? result = dialog.ShowDialog();
        return result == true ? dialog.FileName : null;
    }
}
