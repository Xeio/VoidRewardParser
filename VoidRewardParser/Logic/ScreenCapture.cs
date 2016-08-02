using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Windows.Globalization;
using Windows.Media.Ocr;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Configuration;

namespace VoidRewardParser.Logic
{
    public class ScreenCapture
    {

        public static async Task<string> ParseTextAsync()
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                using (var memoryRandomAccessStream = new InMemoryRandomAccessStream())
                {
                    await Task.Run(() => SaveScreenshot(memoryStream));
                    await memoryRandomAccessStream.WriteAsync(memoryStream.ToArray().AsBuffer());
                    return await RunOcr(memoryRandomAccessStream);
                }
            }
            finally
            {
                GC.Collect(0);
            }
        }

        public static void SaveScreenshot(Stream stream)
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

        private static async Task<string> RunOcr(IRandomAccessStream stream)
        {
            OcrEngine engine = null;
            if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["LanguageCode"]))
            {
                engine = OcrEngine.TryCreateFromLanguage(new Language(ConfigurationManager.AppSettings["LanguageCode"]));
            }
            if (engine == null)
            {
                engine = OcrEngine.TryCreateFromUserProfileLanguages();
            }
            var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);
            var result = await engine.RecognizeAsync(await decoder.GetSoftwareBitmapAsync());
            return result.Text;
        }
    }
}
