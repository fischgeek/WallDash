namespace WallDash.FSharp

open JFSharp
open System
open System.IO
open TrelloConnectFSharp
open JFSharp.Pipes
open WallDash.FSharp.SettingsTypes

module Settings =
    let cfg = LoadConfig()
    let GoogleCalendarCredentials = @"c:\dev\config\google-calendar-credentials.json"
    let ChromeExe = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"
    let a = System.Configuration.ConfigurationManager.AppSettings

    let private calendar2 = a.["Calendar2"]
    let private calendar3 = a.["Calendar3"]

    let private monitorDimensions = a.["MonitorDimensions"]
    let private trelloListId = System.Configuration.ConfigurationManager.AppSettings.["TrelloListId"]

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

    let private getQuote () = 
        printf "\tGetting MOTD..."
        let quotes = SettingsTypes.LoadQuotes() |> Seq.toArray
        let random = Random()
        let ranNum = random.Next(0, quotes.Length)
        let quote = 
            let q = quotes.[ranNum]
            $"{q.Quote}" //<br />- {q.Author}"
        let quoteFile = @"c:\dev\temp\walldash\quote.txt"
        let quoteText = 
            if File.Exists quoteFile then
                let fi = FileInfo(quoteFile)
                if fi.CreationTime.Date = DateTime.Now.Date then
                    File.ReadAllText(quoteFile)
                else
                    File.WriteAllText(quoteFile, quote)
                    quote
            else
                File.WriteAllText(quoteFile, quote)
                quote
        printfn "Done."
        quoteText
            
    let private formatGreeting motd =
        //let quote = MOTD.GetVerseOfTheDay()   // getQuote()
        let now = DateTime.Now
        let words =
            if now.Hour > 7 && now.Hour < 12 then "Good Morning!"
            elif now.Hour > 12 && now.Hour < 17 then "Good Afternoon!"
            else "Good Evening!"
        $"<div id='motd' class='header'>{words}</div><div class='quote'>{motd}</div>"

    let private getDateInfo() = sprintf "<div class='header'>%s</div><div>%s</div>" (DateTime.Now.DayOfWeek.ToString()) (DateTime.Now.ToString("MMMM d, yyyy"))

    let GetBodyHtml motd : string =
        let greeting = formatGreeting motd
        let date = getDateInfo()
        let weather = Weather.GetWeather()
        let trello = Trello.GetTrelloItems()
        let cal = Calendar.GetCalendarInfo()
        let driveInfo = Drive.GetDriveInfo()
        let html = 
            $"
            <div class='container-fluid'>
                <div class='row' style='text-align: center; margin-bottom: 10px;'>
                    <div class='col-sm'>{greeting}</div>
                    <div class='col-sm'>{date}</div>
                    <div id='weather-container' class='col-sm'>{weather}</div>
                </div>
                <div class='row center-box' style='margin-bottom: 10px'>
                    <div class='col-sm'>
                        <div class='big-box-section-header'>High Priority Items</div>
                        <div class='card-container'>{trello}</div>
                    </div>
                    <div class='col-sm'>
                        <div class='big-box-section-header'></div>
                    </div>
                    <div class='col-sm'>
                        <div class='big-box-section-header'></div>
                    </div>
                </div>
                <div class='row'>
                    <div class='col-sm'>
                        {cal}
                    </div>
                    <div class='col-md-auto' style='width:150px'>
                        {driveInfo}
                    </div>
                </div>
                <div id='timestamp'>{DateTimePipe.StampString()}</div>
            </div>
            "
        html