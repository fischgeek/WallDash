namespace WallDash.FSharp

open JFSharp.Pipes
open JFSharp.WebCalls
open System
open System.Diagnostics
open System.Timers
open System.IO
open System.Reflection
open TrelloConnect
open JFSharp
open Dropbox.Api.Sharing

module Actions =
    let cfg = SettingsTypes.LoadConfig()
    let private path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
    let private providedHeadHtml = @$"{path}\html\head.html"
    let private providedEndingBody = @$"{path}\html\ending_body.html"

    let InitWallDash() =
        DirectoryPipe.Create cfg.WorkingDirectory
        DirectoryPipe.Create @"c:\dev\temp\walldash\html\js"
        DirectoryPipe.Create @"c:\dev\temp\walldash\html\css"
        FilePipe.CopyWithOverwrite $@"{path}\html\js\jquery.min.js" @"c:\dev\temp\walldash\html\js\jquery.min.js"
        FilePipe.CopyWithOverwrite $@"{path}\html\js\percircle.js" @"c:\dev\temp\walldash\html\js\percircle.js"
        FilePipe.CopyWithOverwrite $@"{path}\html\css\percircle.css" @"c:\dev\temp\walldash\html\js\percircle.css"

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
        let headHtml = File.ReadAllText providedHeadHtml
        let endingBody = File.ReadAllText providedEndingBody
        let bodyHtml = Settings.GetBodyHtml cfg stamp

        printf "\tRendering HTML..."
        let source = $"<!DOCTYPE html><html>{headHtml}<body>{bodyHtml}{endingBody}</body></html>"
        File.WriteAllText(cfg.OutputHtmlFile, source)
        Cleanup source
        let savedImage = HtmlToImage cfg.OutputHtmlFile (1920, 1080)
        JFSharp.Utils.SecondSleep 5
        Wallpaper.Set savedImage None |> ignore
        printfn "Done."
        let stamp = DateTime.Now.ToLongTimeString()
        printfn $"[{stamp}] Done."

    let StartWallDash() = 

        

        InitWallDash()
        DoWallDashStuff()
        //let timer = new Timers.Timer(300000.)
        //let timerEvent = Async.AwaitEvent (timer.Elapsed) |> Async.Ignore
        //timer.Start()
        //let mutable shouldRun = true
        //while shouldRun do
        //    shouldRun <- DateTime.Now.Hour > 6 && DateTime.Now.Hour < 23 
        //    Async.RunSynchronously timerEvent
        //    printfn "%A" DateTime.Now
        //    DoWallDashStuff()

    let getMonitorDimensions = Settings.MonitorSize

    let checkForGoogleChrome = ()

    let SetWallPaper() = Wallpaper.Set @"C:\Users\jeremiah\Dropbox\Jeremiah\Photos\Wallpaper\2ca37eb1.jpg" None