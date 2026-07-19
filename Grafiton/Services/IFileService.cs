using System.Collections.Generic;
using Grafiton.Models;

namespace Grafiton.Services;

public interface IFileService
{
    List<RecentFile> GetRecentFiles();
    void AddRecentFile(string filePath);
    void RemoveRecentFile(string filePath);
    void ClearRecentFiles();
    string? OpenFileDialog();
    string[]? OpenMultipleFilesDialog();
    string? OpenImageFileDialog();
}
