namespace WallDash.FSharp

open System
open System.Management
open System.IO
open JFSharp

module Drive = 
    //let mappedDrives () = 
    //    let searcher =  new ManagementObjectSearcher(
    //        "root\\CIMV2",
    //        "SELECT * FROM Win32_MappedLogicalDisk")
    //    ""

    let GetDriveInfo() =
        let ttf = (int64)1024
        let circleRad = 440M

        let allDrives = DriveInfo.GetDrives()
        let cdrive = 
            DriveInfo.GetDrives()
            |> Seq.find (fun d -> d.Name = @"C:\")

        let name = cdrive.Name.Replace(@":\", "")
        let totalSpace = cdrive.TotalSize / ttf / ttf / ttf
        let freeSpace = cdrive.TotalFreeSpace / ttf / ttf / ttf
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
        name,freeSpace,totalSpace,perc,color