// fsharplint:disable Hints
namespace WallDash.FSharp

open JFSharp
open System
open FSharpGoogleCalendar
open Google.Apis.Calendar.v3.Data
open WallDash.FSharp.SettingsTypes
open  JFSharp.Pipes
open System.IO

module Calendar = 
    let cfg = LoadConfig()

    type DashboardEvent = {
        Title: string
        Color: string
        AllDay: bool
        StartDate: DateTime
        EndDate: DateTime
        StartDateAsString: string
        EndDateAsString: string
        Separator: string
    }
    with 
        static member IsAllDay (x: DashboardEvent) = x.AllDay
        static member HasTitle (x: DashboardEvent) = x.Title |> SP.IsNotEmpty

    let private getCalendarColor calId = 
        let bg,fg = GoogleCalendar.GetCalendarColor calId
        bg

    let private getColorFromId colorId = 
        let clrId = if isNull colorId then "10" else colorId
        let x = GoogleCalendar.GetColors clrId
        x.Background

    let GetCalendarEvents() = 
        let mappedEvents = 
            cfg.GoogleCalendarIds
            |> Seq.map (fun cal -> 
                let events = 
                    GoogleCalendar.GetEventList cal DateTime.Now (DateTime.Now.AddDays(14.))                    
                let bg,fg = GoogleCalendar.GetCalendarColor cal
                bg,events
            )
        mappedEvents

    let private formatEventListForDisplay (events: DashboardEvent list) =
        events
        |> function
        | [] -> $"<li class='no-events'>No Events</li>"
        | x ->
            x
            |> Seq.map (fun (event: DashboardEvent) -> 
                let st,sep,en = 
                    event.AllDay
                    |> function
                    | true -> "All Day", "", ""
                    | false -> event.StartDateAsString, event.Separator, event.EndDateAsString
                let t = event.Title |> StringPipe.KeepStart 35
                $"<li style='border-left:2px solid {event.Color}'>{t}<div class='event-time'>{st}{sep}{en}</div></li>"
                )
                |> String.concat ""
    
    let private getRealDate (eventDateTime: EventDateTime) = 
        if isNull eventDateTime.Date then 
            eventDateTime.DateTime.HasValue
            |> function
            | true -> eventDateTime.DateTime.Value
            | false -> failwith "Could not parse date"
        else 
            DateTime.Parse(eventDateTime.Date)

    let private getStringDate (eventDateTime: EventDateTime) = 
        if isNull eventDateTime.Date then
            eventDateTime.DateTime.HasValue
            |> function
            | true -> eventDateTime.DateTime.Value.ToString("h:mm")
            | false -> failwith "Could not parse date"
        else 
            "All Day"

    let private getStringTimeRange (startDate, endDate) = 
        $"{getStringDate startDate} {getStringDate endDate}"
    
    let private cleanAndFlatten (eventsWithColor: (string * Event list option) list) = 
        eventsWithColor
        |> List.collect (fun (clr,evtOp) -> 
            evtOp
            |> OP.NoneTo []
            |> List.map (fun evt -> 
                let allDay = if getStringDate evt.Start = "All Day" then true else false
                {
                    Title = evt.Summary
                    Color = clr
                    AllDay = allDay
                    StartDate = getRealDate evt.Start
                    EndDate = getRealDate evt.End
                    StartDateAsString = getStringDate evt.Start
                    EndDateAsString = getStringDate evt.End
                    Separator = "-"
                }
            )
        )
        
    let private cleanAndFlattenx =
        List.collect (fun (clr, evtOp) -> 
            evtOp
            |> OP.NoneTo []
            |> List.map (fun (evt: Event) -> 
                let allDay = if getStringDate evt.Start = "All Day" then true else false
                {
                    Title = evt.Summary
                    Color = clr
                    AllDay = allDay
                    StartDate = getRealDate evt.Start
                    EndDate = getRealDate evt.End
                    StartDateAsString = getStringDate evt.Start
                    EndDateAsString = getStringDate evt.End
                    Separator = "-"
                }
            )
        )
    
    let private collectEvents (dateTime: DateTime) (events: DashboardEvent list) = 
        let d = dateTime.Date
        let allDayEvents = 
            events 
            |? DashboardEvent.IsAllDay
            |? DashboardEvent.HasTitle
            |? fun ev -> ev.StartDate.Date = d
        
        let eventsToday = 
            events
            |?! DashboardEvent.IsAllDay
            |? (fun ev -> ev.StartDate.Date = d)
            |> Seq.sortBy (fun ev -> ev.StartDate.Date)

        allDayEvents |> Seq.append eventsToday |> Seq.toList

    let private shouldGetCalInfo() = 
        let now = DateTime.Now
        let calFile = cfg.CacheFiles.Calendar |> FileInfo
        let diff = now - calFile.LastWriteTime
        if diff.TotalMinutes >= 15.0 then 
            File.Delete calFile.FullName
            true 
        else false

    let GetCalendarInfo() =
        let addDay (x: int) (dt: DateTime) = float x |> dt.AddDays 
        printf "\tGetting Calendar events..."
        let calendarDiv = 
            if shouldGetCalInfo() then
                let events = GetCalendarEvents() |> Seq.toList
                let dt = DateTime.Now
                let cleaned = events |> cleanAndFlatten
                let todaysEvents = cleaned |> collectEvents dt |> formatEventListForDisplay
                let tomorrowsEvents = cleaned |> collectEvents (dt.AddDays(1.)) |> formatEventListForDisplay
                let nextEvents = cleaned |> collectEvents (dt |> addDay 2) |> formatEventListForDisplay
                let nextDay = dt.AddDays(2.).ToString("dddd")
                let calHtml = 
                    $"<div class='calendar'>
                        <h1>Today</h1><ul class='calendar-list'>{todaysEvents}</ul>
                        <h1>Tomorrow</h1><ul class='calendar-list'>{tomorrowsEvents}</ul>
                        <h1>{nextDay}</h1><ul class='calendar-list'>{nextEvents}</ul>
                    </div>"
                File.WriteAllText(cfg.CacheFiles.Calendar, calHtml)
                calHtml
            else File.ReadAllText(cfg.CacheFiles.Calendar)
        printfn "Done."
        calendarDiv
