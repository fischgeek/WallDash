namespace WallDash.FSharp

open System
open System.Management
open System.IO
open JFSharp
open JFSharp.Pipes
open System.Management.Instrumentation

module Drive = 
    let cfg = SettingsTypes.LoadConfig()
    type DriveSpecs = 
        {
            Name: string
            TotalSpace: int64
            FreeSpace: int64
            PercentSpace: decimal
            Color: string
        }

    let calculateDriveSpace (drive: DriveInfo) = 
        let ttf = (int64)1024
        let circleRad = 440M
        let name = drive.Name.Replace(@":\", "")
        let totalSpace = drive.TotalSize / ttf / ttf / ttf
        let totalSpaceSuffix = if totalSpace >= ttf then "TB" else "GB"
        let freeSpace = drive.TotalFreeSpace / ttf / ttf / ttf
        let freeSpaceSuffix = if freeSpace >= ttf then "TB" else "GB"
        let decimalValueOfDiff = (decimal)freeSpace / (decimal)totalSpace
        let diffPercentage = Math.Round(decimalValueOfDiff * 100M, 0)
        let usedPercentage = (int)(100M - diffPercentage)
        
        let color = 
            if usedPercentage >=< (0,50) then "green"
            elif usedPercentage >=< (51,75) then "orange"
            elif usedPercentage >=< (76,100) then "red"
            else "blue"
            
        let x = (circleRad * decimalValueOfDiff)
        let perc = Math.Round(x, 0) // Math.Round(circleRad - x, 0)
        { Name = name; FreeSpace = freeSpace; TotalSpace = totalSpace; PercentSpace = perc; Color = color; }

    let private getSpecificDriveInfo (labelLetters: string[]) = 
        DriveInfo.GetDrives()
        |> Seq.map (fun d ->
            labelLetters 
            |> Seq.map (fun l -> if d.Name = @$"{l}:\" then Some d else None)
            |> Seq.toList
        )
        |> Seq.toList
        |> Seq.concat
        |> OptionPipe.UnwrapSomes
        |> Seq.map (calculateDriveSpace)
        |> Seq.toList

    let private generateDriveHtml (drives: DriveSpecs list) = 
        drives
        |> Seq.map (fun x -> 
        $"
        <div>
            <style>
                #drive-{x.Name} {{
                    stroke-dasharray: {x.PercentSpace};
                    stroke-dashoffset: {x.PercentSpace};
                }}
            </style>
            <div class='item html'>
                <div class='drive-labels'>
                    <h2>{x.Name}</h2>
                    <span>{x.FreeSpace}GB</span>
                </div>
                <svg width='160' height='160' xmlns='http://www.w3.org/2000/svg'>
                    <g>
                        <circle id='drive-{x.Name}' class='circle_animation' r='69.85699' cy='81' cx='81' stroke-width='8' stroke='{x.Color}' fill='none' />
                    </g>
                </svg>
            </div>
        </div>")
        |> String.concat ""

    let GetDriveInfo() = 
        cfg.Drives 
        |> getSpecificDriveInfo
        |> generateDriveHtml
