using Avalonia.Media;
using Avalonia.Media.Imaging;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WateryTart.Core.Utilities
{
    public class DominantColorExtractor
    {
        /// <summary>
        /// Extracts the top 3 dominant colors from an Avalonia ImageSource.
        /// </summary>
        /// <param name="imageSource">The image source to analyze</param>
        /// <param name="sampleSize">Optional: Number of colors to quantize to (default: 256)</param>
        /// <param name="ensureWhiteTextContrast">Optional: Whether to ensure colors have sufficient contrast with white text</param>
        /// <param name="minContrastRatio">Optional: Minimum contrast ratio for white text (default: 4.5)</param>
        /// <returns>A list of the top 3 dominant colors</returns>
        public static async Task<List<Color>> GetDominantColorsAsync(
            IImage imageSource, 
            int sampleSize = 256, 
            bool ensureWhiteTextContrast = false,
            double minContrastRatio = 4.5)
        {
            if (imageSource == null)
                return new List<Color>();

            return await Task.Run(() => GetDominantColors(imageSource, sampleSize, ensureWhiteTextContrast, minContrastRatio));
        }

        /// <summary>
        /// Synchronous version of GetDominantColorsAsync.
        /// </summary>
        public static List<Color> GetDominantColors(IImage imageSource, int sampleSize = 256, bool ensureWhiteTextContrast = false, double minContrastRatio = 4.5)
        {
            if (imageSource == null)
                return new List<Color>();

            if (imageSource is Bitmap bitmap)
            {
                return ExtractColorsFromBitmap(bitmap, sampleSize, ensureWhiteTextContrast, minContrastRatio);
            }

            return new List<Color>();
        }

        private static List<Color> ExtractColorsFromBitmap(Bitmap bitmap, int sampleSize, bool ensureWhiteTextContrast, double minContrastRatio)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                bitmap.Save(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                using var skBitmap = SKBitmap.Decode(memoryStream);
                return ExtractColorsFromSkBitmap(skBitmap, sampleSize, ensureWhiteTextContrast, minContrastRatio);
            }
            catch
            {
                return new List<Color>();
            }
        }

        private static List<Color> ExtractColorsFromSkBitmap(SKBitmap bitmap, int sampleSize, bool ensureWhiteTextContrast, double minContrastRatio)
        {
            var colorBuckets = new Dictionary<int, int>();
            int width = bitmap.Width;
            int height = bitmap.Height;

            // Sample every nth pixel to improve performance on large images
            int sampleStep = Math.Max(1, (int)Math.Sqrt((width * height) / sampleSize));

            for (int y = 0; y < height; y += sampleStep)
            {
                for (int x = 0; x < width; x += sampleStep)
                {
                    SKColor pixel = bitmap.GetPixel(x, y);

                    // Skip mostly transparent pixels
                    if (pixel.Alpha < 128)
                        continue;

                    // Quantize color to reduce color space
                    int quantized = QuantizeColor(pixel.Red, pixel.Green, pixel.Blue);

                    if (colorBuckets.ContainsKey(quantized))
                        colorBuckets[quantized]++;
                    else
                        colorBuckets[quantized] = 1;
                }
            }

            // Get colors with sufficient contrast for white text
            var topColors = colorBuckets
                .OrderByDescending(x => x.Value)
                .Select(x => UnquantizeColor(x.Key))
                .Where(color => HasSufficientContrastWithWhite(color, minContrastRatio))
                .Take(3)
                .ToList();

            // If we don't have enough colors with good contrast, darken the original colors
            if (topColors.Count < 3)
            {
                var fallbackColors = colorBuckets
                    .OrderByDescending(x => x.Value)
                    .Select(x => UnquantizeColor(x.Key))
                    .Take(3)
                    .Select(color => DarkenColor(color))
                    .ToList();
                
                return fallbackColors;
            }

            return topColors;
        }

        private static int QuantizeColor(byte r, byte g, byte b)
        {
            // Reduce color to 3 bits per channel for faster grouping
            int qr = (r >> 5) & 0x07;
            int qg = (g >> 5) & 0x07;
            int qb = (b >> 5) & 0x07;
            return (qr << 6) | (qg << 3) | qb;
        }

        private static Color UnquantizeColor(int quantized)
        {
            int qr = (quantized >> 6) & 0x07;
            int qg = (quantized >> 3) & 0x07;
            int qb = quantized & 0x07;

            // Scale back to full 8-bit values
            byte r = (byte)((qr << 5) | (qr << 2) | (qr >> 1));
            byte g = (byte)((qg << 5) | (qg << 2) | (qg >> 1));
            byte b = (byte)((qb << 5) | (qb << 2) | (qb >> 1));

            return new Color(255, r, g, b);
        }
        private static double GetRelativeLuminance(Color color)
        {
            // Convert to sRGB and calculate relative luminance (WCAG 2.0)
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            r = r <= 0.03928 ? r / 12.92 : Math.Pow((r + 0.055) / 1.055, 2.4);
            g = g <= 0.03928 ? g / 12.92 : Math.Pow((g + 0.055) / 1.055, 2.4);
            b = b <= 0.03928 ? b / 12.92 : Math.Pow((b + 0.055) / 1.055, 2.4);

            return 0.2126 * r + 0.7152 * g + 0.0722 * b;
        }

        private static bool HasSufficientContrastWithWhite(Color color, double minContrastRatio = 4.5)
        {
            // WCAG AA standard requires 4.5:1 for normal text, 3:1 for large text
            double colorLuminance = GetRelativeLuminance(color);
            double whiteLuminance = 1.0; // White has luminance of 1

            double contrastRatio = (whiteLuminance + 0.05) / (colorLuminance + 0.05);
            return contrastRatio >= minContrastRatio;
        }

        private static Color DarkenColor(Color color, double factor = 0.6)
        {
            // Darken the color to ensure better contrast
            return new Color(
                color.A,
                (byte)(color.R * factor),
                (byte)(color.G * factor),
                (byte)(color.B * factor)
            );
        }
    }
}