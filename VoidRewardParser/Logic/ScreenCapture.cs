using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Windows.Globalization;
using Windows.Media.Ocr;
using Windows.Storage;

namespace VoidRewardParser.Logic
{
    public class ScreenCapture
    {

        public static async Task<string> ParseTextAsync()
        {

            string fileName = string.Empty;
            try
            {
                fileName = await Task.Run(() => Path.GetTempFileName());
                await Task.Run(() => SaveScreenshot(fileName));
                return await RunOcr(fileName);
            }
            finally
            {
                if(!string.IsNullOrWhiteSpace(fileName) && File.Exists(fileName))
                {
                    await Task.Run(() => File.Delete(fileName));
                }
            }
        }

        public static void SaveScreenshot(string stream)
        {
            int width = (int)SystemParameters.FullPrimaryScreenWidth;
            int height = (int)SystemParameters.FullPrimaryScreenHeight;
            using (var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
                graphics.Save();
                bitmap.Save(stream, ImageFormat.Png);
            }
        }

        private static async Task<string> RunOcr(string file)
        {
            OcrEngine engine = OcrEngine.TryCreateFromUserProfileLanguages();
            Uri uri = new Uri(file);
            var storageFile = await StorageFile.GetFileFromPathAsync(file);
            var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(await storageFile.OpenAsync(FileAccessMode.Read));
            var result = await engine.RecognizeAsync(await decoder.GetSoftwareBitmapAsync());
            return result.Text;
        }
    }
}
