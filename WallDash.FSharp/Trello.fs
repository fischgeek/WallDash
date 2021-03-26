namespace WallDash.FSharp

open JFSharp.Pipes
open TrelloConnectFSharp
open WallDash.FSharp.SettingsTypes

module Trello = 
    let GetTrelloItems() =
        let cfg = LoadConfig()
        printf "\tGetting Trello items..."
        let cards = Trello.GetCards cfg.TrelloListId
        let trelloHtml =
            TrelloCards.Parse(cards)
            |> Seq.map (fun c -> sprintf "<div class='card-title'>%s</div><div class='card-desc'>%s</div>" c.Name (c.Desc |> StringPipe.KeepStart 50))
            |> String.concat ""
        printfn "Done."
        trelloHtml