namespace WallDash.FSharp
open System.Windows.Forms

module Forms =
    let MakeLabel text (clickAction: unit -> unit) = 
        let l = new Label()
        l.Name <- text
        l.Text <- text
        l.Click.Add (fun _ -> clickAction())

    let MakeText text (keyPressEvent: unit -> unit) =
        let t = new TextBox()
        t.Name <- text
        t.KeyPress.Add (fun x -> keyPressEvent())

    let rec DoBackgroundStuff() = 
        System.Threading.Thread.Sleep 1000
        DoBackgroundStuff()
    
    let MakeForm() =
        let f = new Form()
        let l = new Label()
        l.Text <- "Hello"
        f.Controls.Add l
        let t = new TextBox()
        t.Text <- "Hello"
        t.Top <- l.Top + l.Height
        f.Controls.Add t

        let b = new Button()
        b.Text <- "Hello"
        b.Click.Add (fun _ -> 
            MessageBox.Show "button pushed" |> ignore
            ())
        b.Top <- t.Top + t.Height
        f.Controls.Add b
        f.Controls |> Seq.cast<Control> |> Seq.iter (fun c ->
            SharedFSharpForms.Pipes.FormsPipe.HandleKeyFull true c (Some Keys.Control) Keys.S (fun () -> (MessageBox.Show "hotkey") |> ignore)
        )


        let x = async { DoBackgroundStuff() } |> Async.StartAsTask
        Application.Run f