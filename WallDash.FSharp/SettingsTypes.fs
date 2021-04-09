namespace WallDash.FSharp

open FSharp.Data
open System.IO

module SettingsTypes =
    type WeatherBit = JsonProvider<"samples/weatherbit.json">
    type WeatherBitForecast = JsonProvider<"samples/weatherbit_forecast.json">
    type OpenWeather = JsonProvider<"samples/openweather.json">
    type TomorrowIo = JsonProvider<"samples/tomorrowio.json">
    type TrelloCards = JsonProvider<"samples/trellocards.json">
    type Config = JsonProvider<"samples/walldash-config.json", RootName="Config">
    type Quote = JsonProvider<"https://raw.githubusercontent.com/misterneo/Random-Quotes-Collection/master/quotes.json", RootName="Quote">
    let LoadQuotes() = JFSharp.Pipes.WebPipe.QuickGet "https://raw.githubusercontent.com/misterneo/Random-Quotes-Collection/master/quotes.json" |> Quote.Parse
    let LoadConfig() : Config.Config = File.ReadAllText(@"c:\dev\config\walldash-config.json") |> Config.Parse

    type WeatherProvider =
        | WeatherBit
        | OpenWeather
        | TomorrowIo

    let GetWeatherProvider (cfg: Config.Config) = 
        cfg.DesiredWeatherSource
        |> function
        | "OpenWeather" -> WeatherProvider.OpenWeather
        | "WeatherBit" -> WeatherProvider.WeatherBit
        | "TomorrowIo" -> WeatherProvider.TomorrowIo
        | _ -> WeatherProvider.OpenWeather

    type WeatherData =
        { Temp: string
          IconUrl: string
          LowHigh: string }
