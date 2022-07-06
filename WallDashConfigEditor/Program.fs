namespace WallDashConfigEditor

open JFSharp
open JFSharp.ConsoleUtils
open WallDash

module WallDashConfigEditor = 
    [<EntryPoint>]
    let main argv =
        argv 
        |> function
        | [||] -> printfn "No options passed in."
        | [|x|] -> if x = "1" then Menu.PrintMainMenu()
        | _ -> printfn "Incorrect options passed in."
        wait()
        0
