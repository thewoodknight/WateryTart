using Avalonia.Media;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WateryTart.Core.Utilities;

namespace WateryTart.Core.Services;

public partial class ColourService : ReactiveObject
{
    private readonly HttpClient _httpClient;

    [Reactive] public partial Color ColourA { get; set; }
    [Reactive] public partial Color ColourB { get; set; }
    [Reactive] public partial Color ColourC { get; set; }
    [Reactive] public partial Color ColourD { get; set; }
    [Reactive] public partial SolidColorBrush ColourAccent { get; set; }
    [Reactive] public partial ColourChosen LastPick { get; set; }

    [Reactive] public partial string LastId { get; set; } = string.Empty;

    public ColourService()
    {
        _httpClient = new HttpClient();

        ColourA = FromHex("092127");
        ColourB = FromHex("144753");
        ColourAccent = new SolidColorBrush(FromHex("FFFFFF"));
        LastPick = ColourChosen.AB;
    }

    public async Task Update(string id, string url)
    {
        if (id != LastId)
        {
            LastId = id;

#pragma warning disable CS4014 
            await GetDominateColours(url);
#pragma warning restore CS4014 
        }
    }


    private async Task GetDominateColours(string url)
    {
        try
        {
            var result = await _httpClient.GetByteArrayAsync(url);
            Stream stream = new MemoryStream(result);

            var image = new Avalonia.Media.Imaging.Bitmap(stream);

            var colors = await DominantColorExtractor.GetDominantColorsAsync(image, colorSimilarityThreshold: 1);

            if (LastPick == ColourChosen.AB)
            {
                ColourC = colors[0];
                ColourD = colors[1];
                LastPick = ColourChosen.CD;

                App.Logger?.LogInformation("Last colours {ColourC} & {ColourD}", ColourC, ColourD);
            }
            else
            {
                ColourA = colors[0];
                ColourB = colors[1];
                LastPick = ColourChosen.AB;
                App.Logger?.LogInformation("Last colours {ColourA} & {ColourB}", ColourA, ColourB);
            }

            ColourAccent = new SolidColorBrush(GetTriadColor(colors[2]));
        }
        catch (Exception ex)
        {
            App.Logger?.LogError(ex, "Error extracting dominant colours");
        }
    }

    private Color FromHex(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
            throw new ArgumentException("Hex string cannot be null or empty", nameof(hex));

        hex = hex.StartsWith("#") ? hex.Substring(1) : hex;

        if (hex.Length != 6 && hex.Length != 8)
            throw new ArgumentException("Hex string must be 6 or 8 characters long (RRGGBB or AARRGGBB)", nameof(hex));

        if (!uint.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out var value))
            throw new ArgumentException("Invalid hex color format", nameof(hex));

        if (hex.Length == 6)
        {
            // RRGGBB format - full opacity
            byte r = (byte)((value >> 16) & 0xFF);
            byte g = (byte)((value >> 8) & 0xFF);
            byte b = (byte)(value & 0xFF);
            return Color.FromArgb(255, r, g, b);
        }
        else
        {
            // AARRGGBB format
            byte a = (byte)((value >> 24) & 0xFF);
            byte r = (byte)((value >> 16) & 0xFF);
            byte g = (byte)((value >> 8) & 0xFF);
            byte b = (byte)(value & 0xFF);
            return Color.FromArgb(a, r, g, b);
        }
    }

    private Color GetTriadColor(Color baseColor)
    {
        // Convert to HSV
        double h, s, v;
        RgbToHsv(baseColor, out h, out s, out v);

        // Add 120 degrees to the hue (modulo 360)
        h = (h + 120) % 360;

        // Convert back to Color
        return HsvToRgb(h, s, v, baseColor.A);
    }

    private void RgbToHsv(Color color, out double h, out double s, out double v)
    {
        double r = color.R / 255.0;
        double g = color.G / 255.0;
        double b = color.B / 255.0;

        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        v = max;

        double delta = max - min;
        s = max == 0 ? 0 : delta / max;

        if (delta == 0)
        {
            h = 0;
        }
        else if (max == r)
        {
            h = 60 * (((g - b) / delta) % 6);
        }
        else if (max == g)
        {
            h = 60 * (((b - r) / delta) + 2);
        }
        else
        {
            h = 60 * (((r - g) / delta) + 4);
        }

        if (h < 0)
            h += 360;
    }

    private Color HsvToRgb(double h, double s, double v, byte alpha)
    {
        double c = v * s;
        double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
        double m = v - c;

        double r1, g1, b1;
        if (h < 60)
        {
            r1 = c; g1 = x; b1 = 0;
        }
        else if (h < 120)
        {
            r1 = x; g1 = c; b1 = 0;
        }
        else if (h < 180)
        {
            r1 = 0; g1 = c; b1 = x;
        }
        else if (h < 240)
        {
            r1 = 0; g1 = x; b1 = c;
        }
        else if (h < 300)
        {
            r1 = x; g1 = 0; b1 = c;
        }
        else
        {
            r1 = c; g1 = 0; b1 = x;
        }

        byte r = (byte)Math.Round((r1 + m) * 255);
        byte g = (byte)Math.Round((g1 + m) * 255);
        byte b = (byte)Math.Round((b1 + m) * 255);

        return Color.FromArgb(alpha, r, g, b);
    }
}
