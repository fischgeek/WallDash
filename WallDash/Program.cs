using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Drawing.Imaging;
using WallDash.FSharp;

namespace WallDash
{
    class Program
    {
        static void Main(string[] args)
        {
            var headHtml = File.ReadAllText(@"C:\dev\WallDash\WallDash\html\head.html");
            var bodyHtml = Settings.GetBodyHtml;
            var source = $"<html>{headHtml}{bodyHtml}</html>";
            Cleanup(source);
            StartBrowser(source);
        }

        private static void Cleanup(string source)
        {
            File.Delete(@"c:\wallpaper.png");
            File.Delete(@"c:\test.html");
            File.WriteAllText(@"c:\test.html", source);
        }

        private static void StartBrowser(string source)
        {
            var th = new Thread(() => {
                var webBrowser = new WebBrowser();
                webBrowser.ScrollBarsEnabled = false;
                webBrowser.DocumentCompleted +=
                    webBrowser_DocumentCompleted;
                webBrowser.DocumentText = source;
                webBrowser.ScriptErrorsSuppressed = true;
                Application.Run();
            });
            th.SetApartmentState(ApartmentState.STA);
            th.Start();
        }

        private static void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var webBrowser = (WebBrowser)sender;
            webBrowser.Width = Settings.MonitorSize.Item1;
            webBrowser.Height = Settings.MonitorSize.Item2;
            using (Bitmap bitmap =
                new Bitmap(webBrowser.Width, webBrowser.Height)) {
                bitmap.SetResolution(96.0F, 96.0F);
                webBrowser.DrawToBitmap(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                bitmap.Save(@"c:\wallpaper.png", ImageFormat.Png);
            }
            var uri = new Uri(@"C:\wallpaper.png");
            Wallpaper.Set(uri, Wallpaper.Style.Centered);
            Environment.Exit(0);
        }
    }
}
