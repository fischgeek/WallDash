namespace WallDash.FSharp

open FSharp.Data
open JFSharp
open System
open TrelloConnectFSharp
open JFSharp.Pipes

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

    let ChromeExe = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"
    let a = System.Configuration.ConfigurationManager.AppSettings
    let private monitorDimensions = a.["MonitorDimensions"]
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

    let mutable lastWeatherCheckTime = DateTime.Now.AddHours -1.0
    let mutable weatherHtml = ""

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
             |> DecimalPipe.RoundZero
             |> StringPipe.ToString)
            (high
             |> DecimalPipe.RoundZero
             |> StringPipe.ToString)

    let private formatTemp = DecimalPipe.RoundZero >> StringPipe.ToString

    let private GetGreeting =
        let now = DateTime.Now

        let words =
            if now.Hour > 7 && now.Hour < 12 then "Good Morning!"
            elif now.Hour > 12 && now.Hour < 17 then "Good Afternoon!"
            else "Good Evening!"
        sprintf "<div class='header'>%s</div>" words

    let private GetDateInfo = sprintf "<div class='header'>%s</div><div>%s</div>" (DateTime.Now.ToString("m")) (DateTime.Now.DayOfWeek.ToString())

    let private ShouldGetWeather() = 
        let now = DateTime.Now
        let diff = now - lastWeatherCheckTime
        printfn "It has been %f seconds since the last weather check." diff.TotalSeconds
        lastWeatherCheckTime <- now
        if diff.TotalSeconds >= 300.0 then true
        else false

    let private GetWeather(source: WeatherProvider): string =
        if ShouldGetWeather() then
            printfn "Getting Weather data from %s" (source.ToString())
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
            let outString = 
                sprintf
                    "<div class='weather header'><span><img src='%s' /></span><span class='header'>%s&deg;</span></div><div class='weather-highlow'>%s</div>"
                    weatherData.IconUrl weatherData.Temp weatherData.LowHigh
            weatherHtml <- outString
            weatherHtml
        else 
            printfn "Skipping weather check."
            weatherHtml

    let private CustomIcon(icon: string) =
        match icon with
        | "c01d" -> "sun.png"
        | "01d" -> "sun.png"
        | _ -> ""

    let private GetTrelloItems() =
        printfn "Getting Trello items..."
        let cards = Trello.GetCards trelloListId
        TrelloCards.Parse(cards)
        |> Seq.map (fun c -> sprintf "<div class='card-title'>%s</div><div class='card-desc'>%s</div>" c.Name (c.Desc |> StringPipe.KeepStart 50))
        |> (fun x -> x)
        |> String.concat ""

    let GetBodyHtml() : string =
        let leftContainer = sprintf "<div class='top-container'>%s</div>" GetGreeting
        let centerContainer = sprintf "<div class='top-container'>%s</div>" GetDateInfo
        let rightContainer = sprintf "<div class='top-container'>%s</div>" (GetWeather WeatherProvider.OpenWeather)

        let bigBox =
            sprintf "<div class='big-box'>
                        <div class='big-box-section'>
                            <div class='big-box-section-header'>Priority Items</div>
                            <div class='card-container'>%s</div>
                        </div>
                     </div><div style='float:right'>%s</div>" (GetTrelloItems()) (DateTimePipe.StampString())
        sprintf "%s%s%s%s" leftContainer centerContainer rightContainer bigBox