namespace WallDash.FSharp

open System
open JFSharp.Pipes
open WallDash.FSharp.SettingsTypes
open System.IO

module Weather = 
    let cfg = LoadConfig()
    let private weatherBitKey = cfg.WeatherBit.ApiKey
    let private openWeatherApi = cfg.OpenWeather.ApiKey
    let private lat = cfg.Lat
    let private lon = cfg.Lon
    let private latLong = sprintf "lat=%s&lon=%s" lat lon
    let private openWeatherBase = "https://api.openweathermap.org/data/2.5/onecall"
    let private openWeatherCall = sprintf "%s?%s&units=imperial&appid=%s" openWeatherBase latLong openWeatherApi
    let private openWeatherIconUrl iconCode = sprintf "http://openweathermap.org/img/wn/%s@2x.png" iconCode

    let private weatherBitBase = "https://api.weatherbit.io/v2.0"
    let private weatherBitForecastCall = sprintf "%s/forecast/daily?%s&units=i&key=%s" weatherBitBase latLong weatherBitKey
    let private weatherBitCurrentCall = sprintf "%s/current?&%s&units=i&key=%s" weatherBitBase latLong weatherBitKey
    let private weatherBitIconUrl iconCode = sprintf "https://www.weatherbit.io/static/img/icons/%s.png" iconCode
    
    let private tomorrowioCall = $"https://api.tomorrow.io/v4/timelines?location={lat},{lon}&timesteps=current&fields=temperature,weatherCode&units=imperial&timezone={cfg.TomorrowIo.TimeZone}&apikey={cfg.TomorrowIo.ApiKey}"
    let private tomorrowIoIconUrl = "https://raw.githubusercontent.com/Tomorrow-IO-API/tomorrow-weather-codes/d53e9d2ffc4d7e182f91d8fda333b189aaea7a13/color"
    let private tomorrowIoIcon code = 
        match code with
        | 4201 -> $"{tomorrowIoIconUrl}/rain_heavy.svg"
        | 4001 -> $"{tomorrowIoIconUrl}/rain.svg"
        | 4200 -> $"{tomorrowIoIconUrl}/rain_light.svg"
        | 6201 -> $"{tomorrowIoIconUrl}/freezing_rain_heavy.svg"
        | 6001 -> $"{tomorrowIoIconUrl}/freezing_rain.svg"
        | 6200 -> $"{tomorrowIoIconUrl}/freezing_rain_light.svg"
        | 6000 -> $"{tomorrowIoIconUrl}/freezing_drizzle.svg"
        | 4000 -> $"{tomorrowIoIconUrl}/drizzle.svg"
        | 7101 -> $"{tomorrowIoIconUrl}/ice_pellets_heavy.svg"
        | 7000 -> $"{tomorrowIoIconUrl}/ice_pellets.svg"
        | 7102 -> $"{tomorrowIoIconUrl}/ice_pellets_light.svg"
        | 5101 -> $"{tomorrowIoIconUrl}/snow_heavy.svg"
        | 5000 -> $"{tomorrowIoIconUrl}/snow.svg"
        | 5100 -> $"{tomorrowIoIconUrl}/snow_light.svg"
        | 5001 -> $"{tomorrowIoIconUrl}/flurries.svg"
        | 8000 -> $"{tomorrowIoIconUrl}/tstorm.svg"
        | 2100 -> $"{tomorrowIoIconUrl}/fog_light.svg"
        | 2000 -> $"{tomorrowIoIconUrl}/fog.svg"
        | 1001 -> $"{tomorrowIoIconUrl}/cloudy.svg"
        | 1102 -> $"{tomorrowIoIconUrl}/mostly_cloudy.svg"
        | 1101 -> $"{tomorrowIoIconUrl}/partly_cloudy_day.svg"
        | 1100 -> $"{tomorrowIoIconUrl}/mostly_clear_day.svg"
        | 1000 -> $"{tomorrowIoIconUrl}/clear_day.svg"
        | _ -> $"{tomorrowIoIconUrl}/clear_day"

    let private weatherApiCall = $"https://api.weatherapi.com/v1/forecast.json?key={cfg.WeatherApi.ApiKey}&q={cfg.WeatherApi.Zip}&days=1&aqi=no&alerts=no"

    let mutable private lastWeatherCheckTime = DateTime.Now.AddHours -1.0
    let mutable private weatherHtml = ""
    let private formatLowHigh low high =
        sprintf "Low: %s | High: %s"
            (low
             |> DecimalPipe.RoundZero
             |> StringPipe.ToString)
            (high
             |> DecimalPipe.RoundZero
             |> StringPipe.ToString)

    let private formatTemp = DecimalPipe.RoundZero >> StringPipe.ToString

    let private shouldGetWeather() = 
        let now = DateTime.Now
        let weatherFile = cfg.CacheFiles.Weather |> FileInfo
        let diff = now - weatherFile.LastWriteTime
        if diff.TotalMinutes >= 15.0 then 
            File.Delete weatherFile.FullName
            true 
        else false

    let private getWeather(source: WeatherProvider): string =
        let weatherHtml =
            if shouldGetWeather() then
                let weatherData =
                    source
                    |> function
                    | WeatherBit ->
                        let res = WebPipe.QuickGet weatherBitForecastCall
                        let forecast = WeatherBitForecast.Parse(res)
                        WebPipe.QuickGet weatherBitCurrentCall
                        |> WeatherBit.Parse
                        |> (fun w ->
                        {| Temp = w.Data.[0].Temp |> formatTemp
                           IconUrl = w.Data.[0].Weather.Icon |> weatherBitIconUrl
                           LowHigh = formatLowHigh forecast.Data.[0].MinTemp forecast.Data.[0].MaxTemp |})
                    | OpenWeather ->
                        WebPipe.QuickGet openWeatherCall
                        |> OpenWeather.Parse
                        |> (fun w ->
                        {| Temp = w.Current.Temp |> formatTemp
                           IconUrl = w.Current.Weather.[0].Icon |> openWeatherIconUrl
                           LowHigh = formatLowHigh w.Daily.[0].Temp.Min w.Daily.[0].Temp.Max |})
                    | TomorrowIo ->
                        WebPipe.QuickGet tomorrowioCall
                        |> TomorrowIo.Parse
                        |> (fun w ->
                            let x = w.Data.Timelines.[0].Intervals.[0].Values
                            {| Temp = (x.Temperature |> DecimalPipe.RoundZero).ToString()
                               IconUrl = x.WeatherCode |> tomorrowIoIcon
                               LowHigh = formatLowHigh x.Temperature x.Temperature
                            |}
                        )
                    | WeatherApi -> 
                        let largeIcon (url: string) =  url.Replace("64x64", "128x128")
                        WebPipe.QuickGet weatherApiCall
                        |> (fun x -> x)
                        |> WeatherApi.Parse
                        |> (fun w ->
                            {| Temp = (w.Current.TempF |> DecimalPipe.RoundZero).ToString()
                               IconUrl = $"https:{w.Current.Condition.Icon}" |> largeIcon
                               LowHigh = formatLowHigh w.Forecast.Forecastday.[0].Day.MintempF w.Forecast.Forecastday.[0].Day.MaxtempF
                            |}
                        )
                let outString = 
                    sprintf
                        "<div class='weather header'><span><img src='%s' /></span><span class='header'>%s&deg;</span></div><div class='weather-highlow'>%s</div>"
                        weatherData.IconUrl weatherData.Temp weatherData.LowHigh
                File.WriteAllText(cfg.CacheFiles.Weather, outString)
                outString
            else 
                File.ReadAllText(cfg.CacheFiles.Weather)
        weatherHtml

    let private customIcon(icon: string) =
        match icon with
        | "c01d" -> "sun.png"
        | "01d" -> "sun.png"
        | _ -> ""

    let GetWeather() = 
        printf "\tGetting Weather data from %s..." (cfg.Weather.Source)
        let w = getWeather(GetWeatherProvider cfg)
        printfn "Done."
        w


