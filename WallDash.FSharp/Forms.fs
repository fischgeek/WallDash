namespace WallDash.FSharp
open System.Windows.Forms
open System.Drawing

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

    type DefaultProps = 
        { Font: Font
          Padding: Padding
          Margin: Padding
          AutoSize: bool }
        static member Empty = { 
            Font = new Font("Verdana", 12.0f) 
            Padding = new Padding(5,5,5,5)
            Margin = new Padding(5,5,5,5)
            AutoSize = true }

    type heading = 
        { Font: Font
          AutoSize: bool }
        static member Empty = 
            { Font = new Font("Verdana", 15.0f) 
              AutoSize = true }

    let head = heading.Empty
    let newLabel (text: string) size = 
        let lbl = new Label()
        lbl.Name <- text.Replace(" ", "")
        lbl.Text <- text
        lbl.AutoSize <- true
        lbl.Font <- new Font("Verdana", size)
        lbl
    let newTextbox (name: string) = 
        let txt = new TextBox()
        txt.Name <- name.Replace(" ", "")
        txt.AutoSize <- true
        txt.Font <- new Font("Verdana", 12.0f)
        txt
    
    let H1 (text: string) (flp: FlowLayoutPanel) = 
        let h1 = newLabel text 16.0f
        flp.Controls.Add h1
    let H2 (text: string) (flp: FlowLayoutPanel) = 
        let h2 = newLabel text 15.0f
        flp.Controls.Add h2
    let H3 (text: string) (flp: FlowLayoutPanel) = 
        let h3 = newLabel text 14.0f
        flp.Controls.Add h3
    
    
    let saves = new System.Collections.Generic.List<unit -> unit>()
    
    let saveToRegistry k v = ()
    let saveToWebService url k v = ()
    
    let KeyValPair key value = 
        let keyLabel = newLabel key 12.0f
        let newPadding = new Padding(5,7,5,5)
        keyLabel.Padding <- newPadding
        let valueTxt = newTextbox key
        saves.Add (fun () -> saveToRegistry key valueTxt.Text)
        
        let miniFlowLayout = new FlowLayoutPanel()
        miniFlowLayout.AutoSize <- true
        miniFlowLayout.FlowDirection <- FlowDirection.LeftToRight
        miniFlowLayout.WrapContents <- false
        miniFlowLayout.Controls.Add(keyLabel)
        miniFlowLayout.Controls.Add(valueTxt)
        miniFlowLayout

    saves |> Seq.iter (fun x -> x())

    let mainContainer() = 
        let container = new FlowLayoutPanel()
        container.FlowDirection <- FlowDirection.TopDown
        container.AutoSize <- true
        container.Dock <- DockStyle.Fill
        container.BorderStyle <- BorderStyle.FixedSingle
        container.WrapContents <- false
        container

    //type Container = 
    //    | Form of Form
    //    | FlowLayoutPanel of FlowLayoutPanel

    //let rec FindControlByName (ctlName: string) (con: Container) =
    //    let controls = 
    //        con 
    //        |> function
    //        | Form f -> f.Controls
    //        | FlowLayoutPanel f -> f.Controls

        //form.Controls
        //|> Seq.iter (fun c ->
        //    c.Controls
        //    |> function
        //    | [] ->
        //)

    let MakeForm() =
        let props = DefaultProps.Empty
        let f = new Form()
        f.BackColor <- Color.White

        let container = mainContainer()
        container |> H1 "Trello Settings"
        container.Controls.Add(KeyValPair "API Key" "trello_api_key")
        container.Controls.Add(KeyValPair "API Token" "trello_api_token")

        let b = new Button()
        b.Text <- "Save"
        b.Click.Add (fun _ -> 
            
            MessageBox.Show "Button pressed" |> ignore
            ())
        //b.Padding <- props.Padding
        b.Margin <- props.Margin
        container.Controls.Add b
        f.Controls.Add(container)
        
        
        //container.Controls |> Seq.cast<Control> |> Seq.iter (fun c ->
        //    SharedFSharpForms.Pipes.FormsPipe.HandleKeyFull true c (Some Keys.Control) Keys.S (fun () -> (MessageBox.Show "hotkey") |> ignore)
        //)
        //f.Controls |> Seq.cast<Control> |> Seq.iter (fun c ->
        //    SharedFSharpForms.Pipes.FormsPipe.HandleKeyFull true c (Some Keys.Control) Keys.S (fun () -> (MessageBox.Show "hotkey") |> ignore)
        //)
        
        

        let ni = new NotifyIcon()
        ni.Visible <- true
        let icon = new System.Drawing.Icon @"C:\Users\jeremiah\Dropbox\Jeremiah\Development\Assets\Icons\alphabet\Multipurpose_Alphabet_Icons_by_HYDRATTZ\Aikawns\W\violet.ico"
        ni.Icon <- icon
        ni.DoubleClick.Add (fun x -> 
            f.Show()
            f.WindowState <- FormWindowState.Normal
        )
        
        let x = async { DoBackgroundStuff() } |> Async.StartAsTask
        Application.Run f