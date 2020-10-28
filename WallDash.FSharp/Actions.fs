namespace WallDash.FSharp

open JFSharp.Pipes
open System
open System.Diagnostics

module Actions =

    let getMonitorDimensions = Settings.MonitorSize

    let checkForGoogleChrome = 
        ()

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