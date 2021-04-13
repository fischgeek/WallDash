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
        let trelloHtml =
            cards
            |> Seq.map (fun c -> sprintf "<div class='card-title'>%s</div><div class='card-desc'>%s</div>" c.Name (c.Desc |> StringPipe.KeepStart 50))
            |> String.concat ""
        printfn "Done."
        trelloHtml