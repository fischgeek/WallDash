namespace WallDash.FSharp

open JFSharp.Pipes
open System
open System.Diagnostics
open System.Timers
open System.IO

module Actions =
    let private walldir = @"c:\dev\temp\walldash\"
    let private providedHeadHtml = @"html\head.html"
    let private wallHtml = @"c:\dev\temp\walldash\walldash.html"
    let private wallBg = @"c:\dev\temp\walldash\wallpaper.png"
    let private tempHtml = @"c:\dev\temp\walldash\temp.html"

    let InitWallDash() =
        DirectoryPipe.Create walldir

    let HtmlToImage html (dimensions: int*int) =
        let w,h = dimensions
        let windowSize = sprintf "%i,%i" w h
        //let stamp = DateTime.Now |> DateTimePipe.ToCompressedStamp
        //let saveLocation = sprintf @"c:\dev\temp\walldash\%s_wallpaper.png" stamp
        let saveLocation = @"c:\dev\temp\walldash\wallpaper.png"
        let ChromeArgs = sprintf """--headless --screenshot=""%s"" --window-size=""%s"" %s""" saveLocation windowSize html
        //ProcessPipe.StartProcess Settings.ChromeExe ChromeArgs
        let theProc = ProcessPipe.StartProcessOption Settings.ChromeExe ChromeArgs
        let rec whileProcExists (proc: Process) =
            ProcessPipe.ExistsById proc.Id
            |> function
            | true -> 
                System.Threading.Thread.Sleep 500
                printfn "Process %i exists. Waiting." proc.Id
                whileProcExists proc
            | false -> 
                printfn "Process exited. Moving on."
                ()
        theProc
        |> function
        | Some p -> whileProcExists p
        | None -> ()
        saveLocation

    let Cleanup source = 
        File.Delete tempHtml
        File.WriteAllText(tempHtml, source)

    let DoWallDashStuff() = 
        printfn "Fetching new data..."
        let headHtml = File.ReadAllText providedHeadHtml
        let bodyHtml = Settings.GetBodyHtml()
        let source = sprintf "<html>%s%s</html>" headHtml bodyHtml
        File.WriteAllText(wallHtml, source)
        Cleanup source
        let savedImage = HtmlToImage wallHtml (1920, 1080)
        Wallpaper.Set savedImage None |> ignore
        printfn "Done"

    let StartWallDash() = 
        InitWallDash()
        DoWallDashStuff()
        let timer = new Timers.Timer(60000.)
        let timerEvent = Async.AwaitEvent (timer.Elapsed) |> Async.Ignore        
        timer.Start()
        while true do
            Async.RunSynchronously timerEvent
            printfn "%A" DateTime.Now
            DoWallDashStuff()

    let getMonitorDimensions = Settings.MonitorSize

    let checkForGoogleChrome = 
        ()

    let SetWallPaper() = Wallpaper.Set @"C:\Users\jeremiah\Dropbox\Jeremiah\Photos\Wallpaper\2ca37eb1.jpg" None