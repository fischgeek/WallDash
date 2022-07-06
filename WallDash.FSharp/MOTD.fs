namespace WallDash.FSharp

open System
open System.IO
open canopy.configuration
open canopy.classic
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open JFSharp
open System.Text.RegularExpressions
open HtmlAgilityPack
open JFSharp.Pipes

module MOTD = 
    open SettingsTypes

    let private getHtmlDoc (url: string) = (new HtmlWeb()).Load(url)

    let private fetchVotd stamp = 
        printf "\tGetting new Verse of the Day..."
        let doc = getHtmlDoc "https://bible.com/"
        let verse = 
            doc.DocumentNode.SelectSingleNode("//p[@class='votd-verse']").InnerText 
            |> StringPipe.Trim 
            |> StringPipe.RegexReplace "\s{2,}" " "
        let verseRef = doc.DocumentNode.SelectSingleNode("//p[@class='votd-ref']").InnerText |> StringPipe.Trim
        printfn "Done."
        {| Verse = verse; Ref = verseRef |}

    let private fetchWod (cfg: Config.Config) stamp =
        printf $"\tGetting Word of the Day..."
        let doc = getHtmlDoc "https://www.merriam-webster.com/word-of-the-day/calendar"
        let word = doc.DocumentNode.SelectSingleNode("//h2[@class='wod-l-hover']").InnerText
        let def = doc.DocumentNode.SelectSingleNode("//div[@class='definition-block']/p").InnerText
        printfn "Done."
        {| Verse = word; Ref = def |}

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

    let GetWordOfTheDay cfg stamp = 
        let wodFile = @"c:\dev\temp\walldash\wod.txt"
        if File.Exists wodFile then
            let fi = FileInfo(wodFile)
            if fi.LastWriteTime.Date = DateTime.Now.Date then
                File.ReadAllText(wodFile)
            else
                let votd = 
                    let v = fetchWod cfg stamp
                    $"{v.Verse}<br />{v.Ref}"
                File.WriteAllText(wodFile, votd)
                votd
        else
            let votd = 
                let v = fetchWod cfg stamp
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