using System;
using System.IO;
using PdfSharpCore.Fonts;

namespace Grafiton.Services;

public class FontResolver : IFontResolver
{
    public string DefaultFontName => "Arial";

    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        string name = familyName.ToLowerInvariant().Trim();
        string faceName = name switch
        {
            "segoe ui" or "segoeui" => isBold && isItalic ? "segoeuiz" : isBold ? "segoeuib" : isItalic ? "segoeuii" : "segoeui",
            "times new roman" or "times" => isBold && isItalic ? "timesbi" : isBold ? "timesbd" : isItalic ? "timesi" : "times",
            "calibri" => isBold && isItalic ? "calibriz" : isBold ? "calibrib" : isItalic ? "calibrii" : "calibri",
            _ => isBold && isItalic ? "arialbi" : isBold ? "arialbd" : isItalic ? "ariali" : "arial"
        };

        return new FontResolverInfo(faceName);
    }

    public byte[]? GetFont(string faceName)
    {
        string fontsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
        string fontPath = Path.Combine(fontsFolder, $"{faceName.ToLowerInvariant()}.ttf");

        if (!File.Exists(fontPath))
        {
            fontPath = Path.Combine(fontsFolder, "arial.ttf");
        }

        if (File.Exists(fontPath))
        {
            return File.ReadAllBytes(fontPath);
        }

        return null;
    }
}
