using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Drawing.Imaging;
using WallDash.FSharp;
using System.Timers;

namespace WallDash
{
public    class Program
    {
        private static System.Timers.Timer t2;
        private static string walldir = @"c:\dev\temp\walldash\";
        private static string providedHeadHtml = @"C:\dev\WallDash\WallDash\html\head.html";
        private static string wallHtml = @"c:\dev\temp\walldash\walldash.html";
        private static string wallBg = @"c:\dev\temp\walldash\wallpaper.png";
        private static string tempHtml = @"c:\dev\temp\walldash\temp.html";

       public static void Main(string[] args)
        {
            t2 = new System.Timers.Timer();
            t2.Interval = 15000;
            t2.Elapsed += T2_Elapsed;
            t2.AutoReset = true;
            t2.Enabled = true;
            Init();
            GetStuff();
            Console.ReadLine();
        }

        private static void Init()
        {
            Directory.CreateDirectory(walldir);
        }

        private static void T2_Elapsed(object sender, ElapsedEventArgs e)
        {
            GetStuff();
        }

        private static void GetStuff()
        {
            Console.WriteLine("Fetching new data...");
            var headHtml = File.ReadAllText(providedHeadHtml);
            var bodyHtml = Settings.GetBodyHtml();
            var source = $"<html>{headHtml}{bodyHtml}</html>";
            File.WriteAllText(wallHtml, source);
            Cleanup(source);
            var savedImage = Actions.HtmlToImage(wallHtml, 1920, 1080);
            System.Threading.Thread.Sleep(5000);
            SetWallPaper(savedImage);
            Console.WriteLine("Done.");
            //Environment.Exit(1);
        }

        private static void SetWallPaper(string fileName)
        {
            var savedFile = Wallpaper.Set(fileName, walldir, Wallpaper.Style.Centered);
            foreach (string file in Directory.GetFiles(walldir, "*.png")) {
                if (file != savedFile && file != wallBg) {
                    try {
                        File.Delete(file);
                    } catch (Exception ex) {
                        Console.Write($"Failed to delete {file}.");
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        private static void Cleanup(string source)
        {
            //File.Delete(@"c:\dev\assets\wallpaper.png");
            File.Delete(tempHtml);
            File.WriteAllText(tempHtml, source);
        }

        private static void StartBrowser(string source)
        {
            //var th = new Thread(() => {
            //    var webBrowser = new WebBrowser();
            //    webBrowser.ScrollBarsEnabled = false;
            //    webBrowser.DocumentCompleted +=
            //        webBrowser_DocumentCompleted;
            //    webBrowser.DocumentText = source;
            //    webBrowser.ScriptErrorsSuppressed = true;
            //    Application.Run();
            //});
            //th.SetApartmentState(ApartmentState.STA);
            //th.Start();
        }

        private static void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            //var webBrowser = (WebBrowser)sender;
            //webBrowser.Width = Settings.MonitorSize.Item1;
            //webBrowser.Height = Settings.MonitorSize.Item2;
            //var stamp = DateTime.Now.Ticks.ToString();
            //var fileName = $@"{walldir}\wallpaper_{stamp}.png";
            //using (Bitmap bitmap =
            //    new Bitmap(webBrowser.Width, webBrowser.Height)) {
            //    bitmap.SetResolution(96.0F, 96.0F);
            //    webBrowser.DrawToBitmap(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
            //    bitmap.Save(fileName, ImageFormat.Png);
            //}
            //var uri = new Uri(fileName);
            //Wallpaper.Set(uri, walldir, Wallpaper.Style.Centered);
            //var files = Directory.GetFiles($@"c:\dev\assets\walldash\");
            //foreach (var file in files) {
            //    if (file != fileName) {
            //        try {
            //            File.Delete(file);
            //        } catch (Exception) {
            //            Console.WriteLine("Failed to delete: " + file);
            //        }                    
            //    }
            //}
            // sort desc by age
            // skip first, delete rest (except bg)
            //Environment.Exit(1);
        }
    }
}
