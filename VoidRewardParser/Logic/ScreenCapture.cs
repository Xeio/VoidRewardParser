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
using Tesseract;
using System.Linq;

namespace VoidRewardParser.Logic
{
    public static class ScreenCapture
    {
        public static async Task<string> ParseTextAsync()
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await Task.Run(() => SaveScreenshot(memoryStream));
                    if (Utilities.IsWindows10OrGreater())
                    {
                        return await RunOcr(memoryStream);
                    }
                    else
                    {
                        return await Task.Run(() => RunTesseractOcr(memoryStream));
                    }
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
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Tiff);
            }
        }

        private static async Task<string> RunOcr(MemoryStream memoryStream)
        {
            using (var memoryRandomAccessStream = new InMemoryRandomAccessStream())
            {
                await memoryRandomAccessStream.WriteAsync(memoryStream.ToArray().AsBuffer());
                OcrEngine engine = null;
                if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["LanguageCode"]))
                {
                    engine = OcrEngine.TryCreateFromLanguage(new Language(ConfigurationManager.AppSettings["LanguageCode"]));
                }
                if (engine == null)
                {
                    engine = OcrEngine.TryCreateFromUserProfileLanguages();
                }
                var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(memoryRandomAccessStream);
                var result = await engine.RecognizeAsync(await decoder.GetSoftwareBitmapAsync());
                return result.Text;
            }
        }

        private static string RunTesseractOcr(MemoryStream memoryStream)
        {
            var ENGLISH_LANGUAGE = @"eng";
            using (var ocrEngine = new TesseractEngine(@".\tessdata", ENGLISH_LANGUAGE))
            {
                ocrEngine.SetVariable("load_system_dawg", false);
                ocrEngine.SetVariable("load_freq_dawg", false);
                using (var imageWithText = Pix.LoadTiffFromMemory(memoryStream.ToArray()))
                {
                    using (var page = ocrEngine.Process(imageWithText))
                    {
                        return page.GetText();
                    }
                }
            }
        }

        private static void EnsureLanguageFilesExist(string languageCode)
        {
            if(!Directory.GetFiles(@".\tessdata").Any(file => file.StartsWith(languageCode)))
            {
                Uri uri = new Uri(@"https://api.github.com/repos/tesseract-ocr/tessdata/contents");
            }
        }
    }
}
