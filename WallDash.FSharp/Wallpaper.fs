namespace WallDash.FSharp

open Microsoft.Win32
open System.Runtime.InteropServices

module Wallpaper = 
    let SPI_SETDESKWALLPAPER = 20
    let SPIF_UPDATEINIFILE = 0x01
    let SPIF_SENDWININICHANGE = 0x02

    [<DllImport("user32.dll", CharSet = CharSet.Auto)>]
    extern void SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni)

    type Style = 
        | Tiled
        | Centered
        | Stretched
        | Fill 
        | Fit
        | Span

    let Set wallpaperPath (style: Style option) =
        let key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true)
        
        let setStyle =
            style
            |> function
            | Some s -> s
            | None -> Style.Fill
        
        setStyle
        |> function
        | Tiled ->
            key.SetValue(@"WallpaperStyle", "0")
            key.SetValue(@"TileWallpaper", "1")
        | Centered ->
            key.SetValue(@"WallpaperStyle", "0")
            key.SetValue(@"TileWallpaper", "0")
        | Stretched ->
            key.SetValue(@"WallpaperStyle", "2")
            key.SetValue(@"TileWallpaper", "0")
        | Fill -> 
            key.SetValue(@"WallpaperStyle", "10")
            key.SetValue(@"TileWallpaper", "0")
        | Fit ->
            key.SetValue(@"WallpaperStyle", "6")
            key.SetValue(@"TileWallpaper", "0")
        | Span ->
            key.SetValue(@"WallpaperStyle", "22")
            key.SetValue(@"TileWallpaper", "0")

        SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, wallpaperPath, SPIF_UPDATEINIFILE)
        wallpaperPath
    ()

