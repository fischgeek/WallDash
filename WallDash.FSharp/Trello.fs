namespace WallDash.FSharp

open JFSharp.Pipes
open TrelloConnect
open WallDash.FSharp.SettingsTypes

module Trello = 
    let GetTrelloItems() =
        let cfg = LoadConfig()
        let t = new TrelloWorker(cfg.Trello.ApiKey, cfg.Trello.ApiToken)
        printf "\tGetting Trello items..."
        let cards = t.GetCards cfg.Trello.DefaultListId
        let out x = printfn $"{x}"
        let style (x: string) = 
           let y = 
            match x.ToLower() with
            | "red" -> "red"
            | "blue" -> "#0000b7"
            | "green" -> "green"
            | "yellow" -> "yellow"
            | "orange" -> "orange"
            | "pink" -> "#ff5674"
            | "purple" -> "purple"
            | _ -> "#00a1ff"

           $"border-left: 2px solid {y}"
        let trelloHtml1 =
            cards
            |> Seq.filter (fun c -> not c.Labels.IsEmpty)
            |> Seq.filter (fun c -> c.Labels.Head.Name.ToLower() = "wcri")
            |> Seq.map (fun c -> 
                let desc = c.Desc |> StringPipe.KeepStart 50
                let primaryLabel = c.Labels.Head
                let clr = style primaryLabel.Color
                $"<div class='card-title {primaryLabel.Name}' style='{clr}'>{c.Name}</div><div class='card-desc'>{desc}</div>"
            )
            |> String.concat ""
        let trelloHtml2 =
            cards
            |> Seq.filter (fun c -> not c.Labels.IsEmpty)
            |> Seq.filter (fun c -> c.Labels.Head.Name.ToLower() = "kero")
            |> Seq.map (fun c -> 
                let desc = c.Desc |> StringPipe.KeepStart 50
                let primaryLabel = c.Labels.Head
                let clr = style primaryLabel.Color
                $"<div class='card-title {primaryLabel.Name}' style='{clr}'>{c.Name}</div><div class='card-desc'>{desc}</div>"
            )
            |> String.concat ""
        let trelloHtml3 =
            cards
            |> Seq.filter (fun c -> not c.Labels.IsEmpty)
            |> Seq.filter (fun c -> c.Labels.Head.Name.ToLower() = "devtech")
            |> Seq.map (fun c -> 
                let desc = c.Desc |> StringPipe.KeepStart 50
                let primaryLabel = c.Labels.Head
                let clr = style primaryLabel.Color
                $"<div class='card-title {primaryLabel.Name}' style='{clr}'>{c.Name}</div><div class='card-desc'>{desc}</div>"
            )
            |> String.concat ""
        printfn "Done."
        trelloHtml1,trelloHtml2,trelloHtml3