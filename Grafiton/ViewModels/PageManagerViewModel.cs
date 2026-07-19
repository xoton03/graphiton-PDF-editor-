using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grafiton.Models;
using Grafiton.Services;

namespace Grafiton.ViewModels;

public partial class PageManagerViewModel : ObservableObject
{
    private readonly IPdfEditService _pdfEditService;
    private readonly IPdfRenderService _pdfRenderService;
    private string _currentFilePath = string.Empty;

    [ObservableProperty]
    private ObservableCollection<PageThumbnailModel> _thumbnails = new();

    [ObservableProperty]
    private PageThumbnailModel? _selectedPage;

    [ObservableProperty]
    private bool _isOpen;

    [ObservableProperty]
    private bool _isProcessing;

    public event Action<string>? DocumentModified;
    public event Action<int>? PageSelected;

    public PageManagerViewModel(IPdfEditService pdfEditService, IPdfRenderService pdfRenderService)
    {
        _pdfEditService = pdfEditService;
        _pdfRenderService = pdfRenderService;
    }

    public async void LoadDocument(string filePath, int pageCount)
    {
        _currentFilePath = filePath;
        Thumbnails.Clear();

        for (int i = 1; i <= pageCount; i++)
        {
            Thumbnails.Add(new PageThumbnailModel
            {
                PageNumber = i,
                RotationAngle = 0,
                IsSelected = false
            });
        }

        // Asynchronously render real thumbnail images
        for (uint i = 0; i < pageCount; i++)
        {
            uint pageIndex = i;
            var thumbnailBitmap = await _pdfRenderService.RenderPageThumbnailAsync(filePath, pageIndex, 180);
            if (pageIndex < Thumbnails.Count && thumbnailBitmap != null)
            {
                Thumbnails[(int)pageIndex].ThumbnailImage = thumbnailBitmap;
            }
        }
    }

    [RelayCommand]
    private void SelectPage(PageThumbnailModel? item)
    {
        if (item != null)
        {
            foreach (var t in Thumbnails) t.IsSelected = false;
            item.IsSelected = true;
            SelectedPage = item;
            PageSelected?.Invoke(item.PageNumber);
        }
    }

    [RelayCommand]
    private void RotateLeft(PageThumbnailModel? item)
    {
        if (item != null)
        {
            item.RotationAngle = (item.RotationAngle - 90 + 360) % 360;
        }
    }

    [RelayCommand]
    private void RotateRight(PageThumbnailModel? item)
    {
        if (item != null)
        {
            item.RotationAngle = (item.RotationAngle + 90) % 360;
        }
    }

    [RelayCommand]
    private void DeletePage(PageThumbnailModel? item)
    {
        if (item != null)
        {
            Thumbnails.Remove(item);
            for (int i = 0; i < Thumbnails.Count; i++)
            {
                Thumbnails[i].PageNumber = i + 1;
            }
        }
    }

    [RelayCommand]
    private async Task SaveChangesAsync()
    {
        if (string.IsNullOrEmpty(_currentFilePath) || !File.Exists(_currentFilePath)) return;

        IsProcessing = true;

        try
        {
            string tempOutput = Path.Combine(Path.GetTempPath(), $"grafiton_edited_{Guid.NewGuid():N}.pdf");

            var newOrder = Thumbnails.Select(t => t.PageNumber - 1).ToList();
            await _pdfEditService.ReorderPagesAsync(_currentFilePath, newOrder, tempOutput);

            var rotations = new Dictionary<int, int>();
            for (int i = 0; i < Thumbnails.Count; i++)
            {
                if (Thumbnails[i].RotationAngle != 0)
                {
                    rotations[i] = Thumbnails[i].RotationAngle;
                }
            }

            if (rotations.Count > 0)
            {
                string tempRotated = Path.Combine(Path.GetTempPath(), $"grafiton_rot_{Guid.NewGuid():N}.pdf");
                await _pdfEditService.RotatePagesAsync(tempOutput, rotations, tempRotated);
                File.Delete(tempOutput);
                tempOutput = tempRotated;
            }

            File.Copy(tempOutput, _currentFilePath, overwrite: true);
            File.Delete(tempOutput);

            DocumentModified?.Invoke(_currentFilePath);
        }
        catch { }
        finally
        {
            IsProcessing = false;
        }
    }
}
