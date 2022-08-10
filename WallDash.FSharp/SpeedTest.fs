namespace WallDash

open JFSharp.Pipes
open System
open System.IO
open OpenQA.Selenium.DevTools
open System.Text.RegularExpressions
open JFSharp

module SpeedTest = 
    let TestSpeed () =
        printf "\tRunning speed test..."
        let exePath = Path.Combine(Environment.CurrentDirectory, "extras", "speedtest.exe")
        let x = ProcessPipe.StartAndCapture exePath ""
        //let x = File.ReadAllLines("c:/dev/temp/speedtest.txt")
        let y = StringPipe.JoinLines x
        let getMatch updown =
            let matches = Regex.Match(y, $"({updown}:)\s+(\d+)", RegexOptions.IgnoreCase)
            if matches.Success then matches.Groups.[2].ToString() else "0"
        let upSpeed = getMatch "download"
        let downSpeed = getMatch "upload"
        printfn "Done."
        $"<span><span class='g-icon material-symbols-outlined'>arrow_circle_up</span>{upSpeed} | <span class='g-icon material-symbols-outlined'>arrow_circle_down</span>{downSpeed}</span>"

