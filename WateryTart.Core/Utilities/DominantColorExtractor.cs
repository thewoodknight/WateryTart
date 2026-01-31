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
        /// <returns>A list of the top 3 dominant colors</returns>
        public static async Task<List<Color>> GetDominantColorsAsync(IImage imageSource, int sampleSize = 256)
        {
            if (imageSource == null)
                return new List<Color>();

            return await Task.Run(() => GetDominantColors(imageSource, sampleSize));
        }

        /// <summary>
        /// Synchronous version of GetDominantColorsAsync.
        /// </summary>
        public static List<Color> GetDominantColors(IImage imageSource, int sampleSize = 256)
        {
            if (imageSource == null)
                return new List<Color>();

            if (imageSource is Bitmap bitmap)
            {
                return ExtractColorsFromBitmap(bitmap, sampleSize);
            }

            return new List<Color>();
        }

        private static List<Color> ExtractColorsFromBitmap(Bitmap bitmap, int sampleSize)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                bitmap.Save(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                using var skBitmap = SKBitmap.Decode(memoryStream);
                return ExtractColorsFromSkBitmap(skBitmap, sampleSize);
            }
            catch
            {
                return new List<Color>();
            }
        }

        private static List<Color> ExtractColorsFromSkBitmap(SKBitmap bitmap, int sampleSize)
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

            // Get top 3 colors
            var topColors = colorBuckets
                .OrderByDescending(x => x.Value)
                .Take(3)
                .Select(x => UnquantizeColor(x.Key))
                .ToList();

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
    }
}