// fsharplint:disable Hints
namespace WallDash.FSharp

open System
open FSharpGoogleCalendar
open Google.Apis.Calendar.v3.Data
open WallDash.FSharp.SettingsTypes
open  JFSharp.Pipes

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
        |> Seq.map (fun (clr,evtOp) -> 
            evtOp
            |> function
            | Some eList -> 
                eList
                |> Seq.map (fun evt -> 
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
            | None -> failwith "Event list was empty"
        )
        |> Seq.concat
        |> Seq.toList

    let private collectEvents (dateTime: DateTime) (events: DashboardEvent list) = 
        let today = dateTime.Date
        let allDayEvents = 
            events 
            |> Seq.filter (fun ev -> ev.AllDay)
            |> Seq.filter (fun ev -> ev.StartDate.Date = today)
            |> Seq.toList
        
        let eventsToday = 
            events
            |> Seq.filter (fun ev -> not ev.AllDay)
            |> Seq.filter (fun ev -> ev.StartDate.Date = today)
            |> Seq.sortBy (fun ev -> ev.StartDate.Date)
            |> Seq.toList

        let allEvents = allDayEvents @ eventsToday
        allEvents

    let GetCalendarInfo() =
        printf "\tGetting Calendar events..."
        let events = GetCalendarEvents() |> Seq.toList
        let dt = DateTime.Now
        let cleaned = events |> cleanAndFlatten
        let todaysEvents = cleaned |> collectEvents dt |> formatEventListForDisplay
        let tomorrowsEvents = cleaned |> collectEvents (dt.AddDays(1.)) |> formatEventListForDisplay
        let nextEvents = cleaned |> collectEvents (dt.AddDays(2.)) |> formatEventListForDisplay
        let nextDay = dt.AddDays(2.).ToString("dddd")
        let calendarDiv = 
             $"<div class='calendar'>
                <h1>Today</h1><ul class='calendar-list'>{todaysEvents}</ul>
                <h1>Tomorrow</h1><ul class='calendar-list'>{tomorrowsEvents}</ul>
                <h1>{nextDay}</h1><ul class='calendar-list'>{nextEvents}</ul>
               </div>"
        printfn "Done."
        calendarDiv
