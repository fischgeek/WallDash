namespace WallDash.FSharp

open FSharp.Data
open System.IO

module SettingsTypes =
    type WeatherBit = JsonProvider<"https://raw.githubusercontent.com/fischgeek/FSharpDataProviderSampleFiles/master/json/weatherbit/weatherbit.json">
    type WeatherBitForecast = JsonProvider<"https://raw.githubusercontent.com/fischgeek/FSharpDataProviderSampleFiles/master/json/weatherbit/weatherbit_forecast.json">
    type OpenWeather = JsonProvider<"https://raw.githubusercontent.com/fischgeek/FSharpDataProviderSampleFiles/master/json/openweather/openweather.json">
    type TomorrowIo = JsonProvider<"https://raw.githubusercontent.com/fischgeek/FSharpDataProviderSampleFiles/master/json/tomorrow.io/tomorrowio.json">
    type Config = JsonProvider<"https://raw.githubusercontent.com/fischgeek/FSharpDataProviderSampleFiles/master/json/walldash/walldash-config.json", RootName="Config">
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
