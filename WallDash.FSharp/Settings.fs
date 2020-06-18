namespace WallDash.FSharp

open FSharp.Data
open JFSharpKit
open System
open TrelloConnectFSharp

module Settings =
    type WeatherBit = JsonProvider<"samples/weatherbit.json">

    type WeatherBitForecast = JsonProvider<"samples/weatherbit_forecast.json">

    type OpenWeather = JsonProvider<"samples/openweather.json">

    type TrelloCards = JsonProvider<"samples/trellocards.json">

    type WeatherProvider =
        | WeatherBit
        | OpenWeather

    type WeatherData =
        { Temp: string
          IconUrl: string
          LowHigh: string }

    let private monitorDimensions = System.Configuration.ConfigurationManager.AppSettings.["MonitorDimensions"]
    let private trelloListId = System.Configuration.ConfigurationManager.AppSettings.["TrelloListId"]
    let private weaterhBitApi = System.Configuration.ConfigurationManager.AppSettings.["WeatherBitApi"]
    let private openWeatherApi = System.Configuration.ConfigurationManager.AppSettings.["OpenWeatherApi"]
    let private lat = System.Configuration.ConfigurationManager.AppSettings.["Lat"]
    let private lon = System.Configuration.ConfigurationManager.AppSettings.["Lon"]
    let private latLong = sprintf "lat=%s&lon=%s" lat lon

    let private openWeatherBase = "https://api.openweathermap.org/data/2.5/onecall"
    let private openWeatherCall = sprintf "%s?%s&units=imperial&appid=%s" openWeatherBase latLong openWeatherApi
    let private openWeatherIconUrl iconCode = sprintf "http://openweathermap.org/img/wn/%s@2x.png" iconCode

    let private weatherBitBase = "https://api.weatherbit.io/v2.0"
    let private weatherBitForecastCall = sprintf "%s/forecast/daily?%s&units=i&key=%s" weatherBitBase latLong weaterhBitApi
    let private weatherBitCurrentCall = sprintf "%s/current?&%s&units=i&key=%s" weatherBitBase latLong weaterhBitApi
    let private weatherBitIconUrl iconCode = sprintf "https://www.weatherbit.io/static/img/icons/%s.png" iconCode

    let MonitorSize = 
        if monitorDimensions = null then failwith "Failed to get monitor dimensions from settings"
        let monSize = monitorDimensions.Split('x')
        let width = 
            match monSize.[0] with
            | Int w -> w
            | _ -> 1920
        let height = 
            match monSize.[1] with
            | Int h -> h
            | _ -> 1080
        (width, height)

    let private formatLowHigh low high =
        sprintf "Low: %s | High: %s"
            (low
             |> DecimalKit.RoundZero
             |> StringKit.ToString)
            (high
             |> DecimalKit.RoundZero
             |> StringKit.ToString)

    let private formatTemp t =
        t
        |> DecimalKit.RoundZero
        |> StringKit.ToString

    let private GetGreeting =
        let now = DateTime.Now

        let words =
            if now.Hour > 7 && now.Hour < 12 then "Good Morning!"
            elif now.Hour > 12 && now.Hour < 17 then "Good Afternoon!"
            else "Good Evening!"
        sprintf "<div class='header'>%s</div>" words

    let private GetDateInfo = sprintf "<div class='header'>%s</div><div>%s</div>" (DateTime.Now.ToString("m")) (DateTime.Now.DayOfWeek.ToString())

    let private GetWeather(source: WeatherProvider): string =
        let weatherData =
            source
            |> function
            | WeatherBit ->
                let res = WebKit.QuickGet weatherBitForecastCall
                let forecast = WeatherBitForecast.Parse(res)
                WebKit.QuickGet weatherBitCurrentCall
                |> WeatherBit.Parse
                |> (fun w ->
                {| Temp = w.Data.[0].Temp |> formatTemp
                   IconUrl = w.Data.[0].Weather.Icon |> weatherBitIconUrl
                   LowHigh = formatLowHigh forecast.Data.[0].MinTemp forecast.Data.[0].MaxTemp |})
            | OpenWeather ->
                WebKit.QuickGet openWeatherCall
                |> OpenWeather.Parse
                |> (fun w ->
                {| Temp = w.Current.Temp |> formatTemp
                   IconUrl = w.Current.Weather.[0].Icon |> openWeatherIconUrl
                   LowHigh = formatLowHigh w.Daily.[0].Temp.Min w.Daily.[0].Temp.Max |})
        sprintf
            "<div class='weather header'><span><img src='%s' /></span><span class='header'>%s&deg;</span></div><div class='weather-highlow'>%s</div>"
            weatherData.IconUrl weatherData.Temp weatherData.LowHigh

    let private CustomIcon(icon: string) =
        match icon with
        | "c01d" -> "sun.png"
        | "01d" -> "sun.png"
        | _ -> ""

    let private GetTrelloItems() =
        let mutable outString = ""
        let cards = Trello.GetCards trelloListId
        TrelloCards.Parse(cards)
        |> Seq.iter (fun c ->
            outString <- sprintf "%s<div class='card'>%s</div>" outString c.Name
            ())
        outString

    let GetBodyHtml: string =
        let leftContainer = sprintf "<div class='top-container'>%s</div>" GetGreeting
        let centerContainer = sprintf "<div class='top-container'>%s</div>" GetDateInfo
        let rightContainer = sprintf "<div class='top-container'>%s</div>" (GetWeather WeatherProvider.OpenWeather)

        let bigBox =
            sprintf "<div class='big-box'>
                        <div class='big-box-section'>
                            <div class='big-box-section-header'>Trello Items</div>
                            <div class='card-container'>%s</div>
                        </div>
                     </div>" (GetTrelloItems())
        sprintf "%s%s%s%s" leftContainer centerContainer rightContainer bigBox
