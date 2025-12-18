using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FluentFlyoutWPF.Classes
{
    public static class ImageHelper
    {
        public static Color GetDominantColor(BitmapSource bitmapSource)
        {
            bool isLight;
            var resized = new TransformedBitmap(bitmapSource,
                new ScaleTransform(100.0 / bitmapSource.PixelWidth,
                                   100.0 / bitmapSource.PixelHeight));
            var converted = new FormatConvertedBitmap(resized, PixelFormats.Bgra32, null, 0);

            int w = converted.PixelWidth;
            int h = converted.PixelHeight;
            int stride = w * 4;
            byte[] pixels = new byte[h * stride];
            converted.CopyPixels(pixels, stride, 0);

            // 2. 先拿“第一主题色”（你原来的逻辑）
            Color first = GetAverageColor(pixels, out int validCount);
            if (validCount == 0)   // 全是透明
            {
                isLight = true;
                return Colors.Gray;
            }

            // 3. 判断是否需要退而求“强调色”
            if (IsNearWhiteOrBlack(first))
            {
                Color accent = GetAccentColor(pixels);
                isLight = IsLightColor(accent);
                return accent;
            }

            isLight = IsLightColor(first);
            return first;
        }

        #region 内部工具
        // 把有效像素平均一下，得到“第一主题色”
        private static Color GetAverageColor(byte[] pixels, out int validCount)
        {
            long r = 0, g = 0, b = 0;
            validCount = 0;
            for (int i = 0; i < pixels.Length; i += 4)
            {
                byte a = pixels[i + 3];
                if (a < 10) continue;

                b += pixels[i];
                g += pixels[i + 1];
                r += pixels[i + 2];
                validCount++;
            }

            if (validCount == 0) return Colors.Gray;
            return Color.FromRgb((byte)(r / validCount),
                                 (byte)(g / validCount),
                                 (byte)(b / validCount));
        }

        // 判断颜色是否接近纯白或纯黑
        private static bool IsNearWhiteOrBlack(Color c)
        {
            byte brightness = (byte)(0.299 * c.R + 0.587 * c.G + 0.114 * c.B);
            return brightness > 210 || brightness < 45;
        }

        // 判断颜色是亮还是暗
        private static bool IsLightColor(Color c)
        {
            byte brightness = (byte)(0.299 * c.R + 0.587 * c.G + 0.114 * c.B);
            return brightness > 128;
        }

        // 统计出现次数最多的“非背景色”作为强调色
        private static Color GetAccentColor(byte[] pixels)
        {
            var freq = new Dictionary<Color, int>();
            for (int i = 0; i < pixels.Length; i += 4)
            {
                byte a = pixels[i + 3];
                if (a < 10) continue;

                byte b = pixels[i];
                byte g = pixels[i + 1];
                byte r = pixels[i + 2];
                var c = Color.FromRgb(r, g, b);

                // 把非常接近黑白/透明的颜色直接过滤掉
                if (IsNearWhiteOrBlack(c)) continue;

                if (freq.ContainsKey(c)) freq[c]++;
                else freq[c] = 1;
            }

            if (freq.Count == 0)   // 实在没有，就返回灰色
                return Colors.Gray;

            return freq.OrderByDescending(kv => kv.Value).First().Key;
        }
        #endregion
    }
}