namespace WallDash.FSharp

open System
open System.IO
open canopy.configuration
open canopy.classic
open OpenQA.Selenium
open OpenQA.Selenium.Chrome

module MOTD = 
    let private fetchVotd stamp = 
        let startOptions = 
            let chromeOpts = ChromeOptions ()
            chromeOpts.AddArgument "--headless"
            chromeOpts.AddArgument "start-maximized"
            let userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.56 Safari/537.36"
            chromeOpts.AddArgument $"user-agent={userAgent}"
            canopy.types.BrowserStartMode.ChromeWithOptions chromeOpts
        start startOptions
        Console.Clear()
        printfn $"[{stamp}] Fetching new data..."
        printf "\tGetting new VOTD..."
        url "https://bible.com/"
        let v = element "p.votd-verse a"
        let r = element "p.votd-ref a"
        //printfn $"{v.Text} - {r.Text}"
        let o = {| Verse = v.Text; Ref = r.Text |}
        quit()
        printfn "Done."
        o

    let private fetchWod stamp = 
        let startOptions = 
            let chromeOpts = ChromeOptions ()
            //chromeOpts.BinaryLocation <- @"c:\dev\chromedriver\chromedriver.exe"
            //chromeOpts.AddArgument "--headless"
            chromeOpts.BrowserVersion <- "95.0.4638.54"
            chromeOpts.AddArgument "start-maximized"
            let userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.56 Safari/537.36"
            chromeOpts.AddArgument $"user-agent={userAgent}"
            //chromeOpts.AddArgument "--log-level=3"
            canopy.types.BrowserStartMode.ChromeWithOptions chromeOpts
        start startOptions
        Console.Clear()
        //let _url = "https://www.merriam-webster.com/word-of-the-day/calendar"
        let _url = "https://www.dictionary.com/e/word-of-the-day/"
        printfn $"[{stamp}] Fetching new data..."
        printf $"\tGetting Word of the Day..."
        url _url
        //let v = element "h2.wod-l-hover"
        //let r = element "div.definition-block p"
        let v = element "div.otd-item-headword__word h1"
        let r = element "div.otd-item-headword__pos p:nth-child(2)"
        //printfn $"{v.Text} - {r.Text}"
        let o = {| Verse = v.Text; Ref = r.Text |}
        quit()
        printfn "Done."
        o

    let GetVerseOfTheDay stamp = 
        let votdFile = @"c:\dev\temp\walldash\votd.txt"
        if File.Exists votdFile then
            let fi = FileInfo(votdFile)
            if fi.LastWriteTime.Date = DateTime.Now.Date then
                File.ReadAllText(votdFile)
            else
                let votd = 
                    let v = fetchVotd stamp
                    $"{v.Verse}<br />{v.Ref}"
                File.WriteAllText(votdFile, votd)
                votd
        else
            let votd = 
                let v = fetchVotd stamp
                $"{v.Verse}<br />{v.Ref}"
            File.WriteAllText(votdFile, votd)
            votd

    let GetWordOfTheDay stamp = 
        let wodFile = @"c:\dev\temp\walldash\wod.txt"
        if File.Exists wodFile then
            let fi = FileInfo(wodFile)
            if fi.LastWriteTime.Date = DateTime.Now.Date then
                File.ReadAllText(wodFile)
            else
                let votd = 
                    let v = fetchWod stamp
                    $"{v.Verse}<br />{v.Ref}"
                File.WriteAllText(wodFile, votd)
                votd
        else
            let votd = 
                let v = fetchWod stamp
                $"{v.Verse}<br />{v.Ref}"
            File.WriteAllText(wodFile, votd)
            votd

    let GetRandomQuote (stamp: string) = 
        printf "\tGetting MOTD..."
        let quotes = SettingsTypes.LoadQuotes() |> Seq.toArray
        let random = Random()
        let ranNum = random.Next(0, quotes.Length)
        let quote = 
            let q = quotes.[ranNum]
            $"{q.Quote}<br />- {q.Author}"
        let quoteFile = @"c:\dev\temp\walldash\quote.txt"
        let quoteText = 
            if File.Exists quoteFile then
                let fi = FileInfo(quoteFile)
                if fi.LastWriteTime.Date = DateTime.Now.Date then
                    File.ReadAllText(quoteFile)
                else
                    File.WriteAllText(quoteFile, quote)
                    quote
            else
                File.WriteAllText(quoteFile, quote)
                quote
        printfn "Done."
        quoteText