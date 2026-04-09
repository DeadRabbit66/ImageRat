using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace ImageRat
{
    public static class SteganographyHelper
    {
        /// <summary>
        /// Прячет текст в изображение (чистый LSB)
        /// </summary>
        public static byte[] HideText(string text, Image image)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);

            byte[] marker = { 0x00, 0x00, 0x00, 0x00 }; 
            byte[] dataWithMarker = new byte[data.Length + marker.Length];
            Buffer.BlockCopy(data, 0, dataWithMarker, 0, data.Length);
            Buffer.BlockCopy(marker, 0, dataWithMarker, data.Length, marker.Length);

            Bitmap bmp = new Bitmap(image);
            HideDataInPixels(bmp, dataWithMarker);

            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Извлекает текст из изображения (чистый LSB)
        /// </summary>
        public static string ExtractText(byte[] imageBytes)
        {
            using (MemoryStream ms = new MemoryStream(imageBytes))
            using (Bitmap bmp = new Bitmap(ms))
            {
                byte[] data = ExtractDataFromPixels(bmp);

                int endIndex = -1;
                for (int i = 0; i <= data.Length - 4; i++)
                {
                    if (data[i] == 0 && data[i + 1] == 0 && data[i + 2] == 0 && data[i + 3] == 0)
                    {
                        endIndex = i;
                        break;
                    }
                }

                if (endIndex == -1)
                    return string.Empty; 

                byte[] result = new byte[endIndex];
                Buffer.BlockCopy(data, 0, result, 0, endIndex);

                return Encoding.UTF8.GetString(result);
            }
        }

        /// <summary>
        /// Проверяет, можно ли спрятать текст в картинку
        /// </summary>
        public static bool CanHide(string text, Image image, out string error)
        {
            error = string.Empty;

            int maxBytes = (image.Width * image.Height * 3) / 8; 

            byte[] data = Encoding.UTF8.GetBytes(text);
            if (data.Length + 4 > maxBytes) 
            {
                error = $"Текст слишком большой. Максимум: {maxBytes - 4} байт";
                return false;
            }

            return true;
        }


        private static void HideDataInPixels(Bitmap bmp, byte[] data)
        {
            int dataIndex = 0;
            int bitIndex = 7; 

            for (int y = 0; y < bmp.Height && dataIndex < data.Length; y++)
            {
                for (int x = 0; x < bmp.Width && dataIndex < data.Length; x++)
                {
                    Color pixel = bmp.GetPixel(x, y);

                    int r = pixel.R;
                    int g = pixel.G;
                    int b = pixel.B;

                    if (dataIndex < data.Length)
                    {
                        int bit = (data[dataIndex] >> bitIndex) & 1;
                        r = (r & 0xFE) | bit;
                        bitIndex--;
                        if (bitIndex < 0)
                        {
                            bitIndex = 7;
                            dataIndex++;
                        }
                    }

                    if (dataIndex < data.Length)
                    {
                        int bit = (data[dataIndex] >> bitIndex) & 1;
                        g = (g & 0xFE) | bit;
                        bitIndex--;
                        if (bitIndex < 0)
                        {
                            bitIndex = 7;
                            dataIndex++;
                        }
                    }
                    if (dataIndex < data.Length)
                    {
                        int bit = (data[dataIndex] >> bitIndex) & 1;
                        b = (b & 0xFE) | bit;
                        bitIndex--;
                        if (bitIndex < 0)
                        {
                            bitIndex = 7;
                            dataIndex++;
                        }
                    }

                    bmp.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }
        }

        private static byte[] ExtractDataFromPixels(Bitmap bmp)
        {
            System.Collections.Generic.List<byte> result = new System.Collections.Generic.List<byte>();
            byte currentByte = 0;
            int bitIndex = 7;

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color pixel = bmp.GetPixel(x, y);

                    int r = pixel.R;
                    int g = pixel.G;
                    int b = pixel.B;

                    int bit = r & 1;
                    currentByte = (byte)((currentByte << 1) | bit);
                    bitIndex--;
                    if (bitIndex < 0)
                    {
                        result.Add(currentByte);
                        currentByte = 0;
                        bitIndex = 7;
                    }

                    bit = g & 1;
                    currentByte = (byte)((currentByte << 1) | bit);
                    bitIndex--;
                    if (bitIndex < 0)
                    {
                        result.Add(currentByte);
                        currentByte = 0;
                        bitIndex = 7;
                    }

                    bit = b & 1;
                    currentByte = (byte)((currentByte << 1) | bit);
                    bitIndex--;
                    if (bitIndex < 0)
                    {
                        result.Add(currentByte);
                        currentByte = 0;
                        bitIndex = 7;
                    }
                }
            }

            return result.ToArray();
        }
    }
}