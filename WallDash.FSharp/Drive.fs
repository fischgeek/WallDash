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
        { Name: string
          TotalSpaceGB: decimal
          TotalSpaceTB: decimal
          DisplayTotalSpace: string
          DisplayFreeSpace: string
          FreeSpace: decimal
          UsedSpace: decimal
          PercentUsed: decimal
          Color: string }

    let getTbValue (sizeValue: int64) =
        let ttfdecimal = 1024M
        Math.Round((decimal) sizeValue
        / ttfdecimal
        / ttfdecimal
        / ttfdecimal
        / ttfdecimal,2)

    let getGbValue (sizeValue: int64) = 
        let ttf = 1024M
        Math.Round((decimal) sizeValue / ttf / ttf / ttf,2)

    let getSizeTuple (sizeValue: int64) = (getGbValue sizeValue, getTbValue sizeValue)

    let calculateDriveSpace (drive: DriveInfo) =
        try
            let name = drive.Name.Replace(@":\", "")
            let totalSizeInGB,totalSizeInTB = getSizeTuple drive.TotalSize
            let freeSpaceInGB,freeSpaceInTB = getSizeTuple drive.TotalFreeSpace
            let usedSpace = totalSizeInGB - freeSpaceInGB
            let displayTotalSize = 
                if totalSizeInTB >= 1M
                then $"{Math.Round(totalSizeInTB,1)}TB" 
                else $"{Math.Round(totalSizeInGB,0)}GB"
            let displayFree = 
                if freeSpaceInTB >= 1M
                then $"{Math.Round(freeSpaceInTB,1)}TB"
                else $"{Math.Round(freeSpaceInGB,0)}GB"

            let usedSpaceDiff = usedSpace / totalSizeInGB
            let usedPercentage = Math.Round(usedSpaceDiff * 100M, 0)
            let color =
                if usedPercentage >=< (0M, 50M) then "green"
                elif usedPercentage >=< (51M, 75M) then "orange"
                elif usedPercentage >=< (76M, 100M) then "red"
                else "blue"
            { 
                Name = name
                FreeSpace = freeSpaceInGB
                TotalSpaceGB = totalSizeInGB
                TotalSpaceTB = totalSizeInTB
                DisplayTotalSpace = displayTotalSize
                DisplayFreeSpace = displayFree
                UsedSpace = usedSpace
                PercentUsed = usedPercentage
                Color = color }
        with ex -> 
            let zerod = 0 |> Decimal
            {Name = "ERR";TotalSpaceGB = zerod;TotalSpaceTB = zerod;DisplayTotalSpace = "";DisplayFreeSpace = "";FreeSpace = zerod;UsedSpace = zerod;PercentUsed = zerod;Color = "red"}

        //printfn $"Drive {name}"
        //printfn $" TotalSpaceGB {totalSizeInGB}"
        //printfn $" TotalSpaceTB {totalSizeInTB}"
        //printfn $" UsedSpace {usedSpace}"
        //printfn $" FreeSpace {freeSpaceInGB}"
        //printfn $" Percentage {usedPercentage}"
        //printfn $" Color {color}"

        

    let private getSpecificDriveInfo (labelLetters: string []) =
        let drives = DriveInfo.GetDrives()
        drives
        |> Seq.map
            (fun d ->
                labelLetters
                |> Seq.map
                    (fun l -> 
                        if d.Name = $"{l}:\\" then
                            Some d 
                        else None)
                    |> Seq.toList)
        |> Seq.toList
        |> Seq.concat
        |> OptionPipe.UnwrapSomes
        |> Seq.map (calculateDriveSpace)
        |> Seq.toList

    //<style>#{x.Name}-drive span {{ color: {x.Color} }}</style>
    let private generateDriveHtml (drives: DriveSpecs list) =
        drives
        |> Seq.map
            (fun x -> 
                $"
                    <div id='{x.Name}-drive' data-percent='{x.PercentUsed}' data-text='{x.Name}' data-animate='false' class='{x.Color} medium circle float-end'>
                        <span class='drive-space'>{x.DisplayFreeSpace}</span>
                    </div>
                ")
        |> String.concat ""

    let GetDriveInfo () =
        printf "\tGetting Drive info..."
        cfg.Drives
        |> getSpecificDriveInfo
        |> (fun x -> 
            printfn "Done."
            x)
        |> generateDriveHtml
