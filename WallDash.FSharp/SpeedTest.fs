namespace WallDash

open JFSharp.Pipes
open JFSharp.ConsoleUtils
open System
open System.IO
open OpenQA.Selenium.DevTools
open System.Text.RegularExpressions
open JFSharp
open WallDash.FSharp.SettingsTypes

module SpeedTest = 
    let cfg = LoadConfig()

    let private shouldGetSpeed() = 
        //let now = DateTime.Now
        //let speedTestFile = cfg.CacheFiles.SpeedTest |> FileInfo
        //let diff = now - speedTestFile.LastWriteTime
        //if diff.TotalMinutes >= 30. then 
        //    File.Delete speedTestFile.FullName
        //    true 
        //else false
        true

    let TestSpeed () =
        printf "\tRunning speed test..."
        let getMatch updown y =
            let matches = Regex.Match(y, $"({updown}:)\s+(\d+)", RegexOptions.IgnoreCase)
            if matches.Success then matches.Groups.[2].ToString() else "0"
        let upSpeed,downSpeed =
            if shouldGetSpeed() then
                let exePath = Path.Combine(Environment.CurrentDirectory, "extras", "speedtest.exe") 
                ProcessPipe.StartAndCapture exePath ""
                |> StringPipe.JoinLines
                |> fun x -> (getMatch "upload" x, getMatch "download" x)
            else "0","0"

        printfn "Done."
        $"<span><span class='g-icon material-symbols-outlined'>arrow_circle_down</span>{upSpeed} | <span class='g-icon material-symbols-outlined'>arrow_circle_up</span>{downSpeed}</span>"

