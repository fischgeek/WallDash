namespace WallDash.FSharp

open JFSharp.Pipes
open System
open System.Diagnostics
open System.Timers
open System.IO
open System.Reflection

module Actions =
    let cfg = SettingsTypes.LoadConfig()
    let private path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
    let private providedHeadHtml = @$"{path}\html\head.html"

    let InitWallDash() =
        DirectoryPipe.Create cfg.WorkingDirectory

    let HtmlToImage html (dimensions: int*int) =
        let w,h = dimensions
        let windowSize = sprintf "%i,%i" w h
        let chromeArgs = sprintf """--headless --screenshot=""%s"" --window-size=""%s"" %s""" cfg.WallpaperFile windowSize html
        let theProc = ProcessPipe.StartProcessOption Settings.ChromeExe chromeArgs  
        let rec whileProcExists (proc: Process) =
            ProcessPipe.ExistsById proc.Id
            |> function
            | true -> 
                System.Threading.Thread.Sleep 500
                //printfn "Process %i exists. Waiting." proc.Id
                whileProcExists proc
            | false -> 
                //printfn "Process exited. Moving on."
                ()
        theProc
        |> function
        | Some p -> whileProcExists p
        | None -> ()
        cfg.WallpaperFile

    let Cleanup source = 
        File.Delete cfg.TempHtmlFile
        File.WriteAllText(cfg.TempHtmlFile, source)

    let DoWallDashStuff() = 
        let stamp = DateTime.Now.ToLongTimeString()
        printfn $"[{stamp}] Fetching new data..."
        let motd = MOTD.GetVerseOfTheDay stamp // getQuote()
        let headHtml = File.ReadAllText providedHeadHtml
        let bodyHtml = Settings.GetBodyHtml motd
        printf "\tRendering HTML..."
        let source = sprintf "<html>%s%s</html>" headHtml bodyHtml
        File.WriteAllText(cfg.OutputHtmlFile, source)
        Cleanup source
        let savedImage = HtmlToImage cfg.OutputHtmlFile (1920, 1080)
        Wallpaper.Set savedImage None |> ignore
        printfn "Done."
        let stamp = DateTime.Now.ToLongTimeString()
        printfn $"[{stamp}] Done."

    let StartWallDash() = 
        InitWallDash()
        DoWallDashStuff()
        let timer = new Timers.Timer(300000.)
        let timerEvent = Async.AwaitEvent (timer.Elapsed) |> Async.Ignore
        timer.Start()
        while true do
            Async.RunSynchronously timerEvent
            printfn "%A" DateTime.Now
            DoWallDashStuff()

    let getMonitorDimensions = Settings.MonitorSize

    let checkForGoogleChrome = ()

    let SetWallPaper() = Wallpaper.Set @"C:\Users\jeremiah\Dropbox\Jeremiah\Photos\Wallpaper\2ca37eb1.jpg" None