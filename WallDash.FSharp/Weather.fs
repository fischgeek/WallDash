namespace WallDash.FSharp

open System
open JFSharp.Pipes
open WallDash.FSharp.SettingsTypes

module Weather = 
    let cfg = LoadConfig()
    let private weaterhBitApi = cfg.WeatherBitApiKey
    let private openWeatherApi = cfg.OpenWeatherApiKey
    let private lat = cfg.Lat
    let private lon = cfg.Lon
    let private latLong = sprintf "lat=%s&lon=%s" lat lon
    let private openWeatherBase = "https://api.openweathermap.org/data/2.5/onecall"
    let private openWeatherCall = sprintf "%s?%s&units=imperial&appid=%s" openWeatherBase latLong openWeatherApi
    let private openWeatherIconUrl iconCode = sprintf "http://openweathermap.org/img/wn/%s@2x.png" iconCode

    let private weatherBitBase = "https://api.weatherbit.io/v2.0"
    let private weatherBitForecastCall = sprintf "%s/forecast/daily?%s&units=i&key=%s" weatherBitBase latLong weaterhBitApi
    let private weatherBitCurrentCall = sprintf "%s/current?&%s&units=i&key=%s" weatherBitBase latLong weaterhBitApi
    let private weatherBitIconUrl iconCode = sprintf "https://www.weatherbit.io/static/img/icons/%s.png" iconCode
    
    let private tomorrowioCall = $"https://api.tomorrow.io/v4/timelines?location={lat},{lon}&timesteps=current&fields=temperature,weatherCode&units=imperial&timezone={cfg.TomorrowIoTimeZone}&apikey={cfg.TomorrowIoApiKey}"
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
        let diff = now - lastWeatherCheckTime
        //printfn "It has been %f seconds since the last weather check." diff.TotalSeconds
        lastWeatherCheckTime <- now
        diff.TotalSeconds >= 300.0

    let private getWeather(source: WeatherProvider): string =
        let weatherHtml =
            if shouldGetWeather() then
                printf "\tGetting Weather data from %s..." (source.ToString())
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

                let outString = 
                    sprintf
                        "<div class='weather header'><span><img src='%s' /></span><span class='header'>%s&deg;</span></div><div class='weather-highlow'>%s</div>"
                        weatherData.IconUrl weatherData.Temp weatherData.LowHigh
                weatherHtml <- outString
                weatherHtml
            else 
                //printfn "\tSkipping weather check."
                weatherHtml
        printfn "Done."
        weatherHtml

    let private customIcon(icon: string) =
        match icon with
        | "c01d" -> "sun.png"
        | "01d" -> "sun.png"
        | _ -> ""

    let GetWeather() = getWeather(GetWeatherProvider cfg)

