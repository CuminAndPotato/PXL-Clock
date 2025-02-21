#r "nuget: FSharp.Compiler.Service, 41.0.0"

open System
open System.IO
open FSharp.Compiler.Interactive.Shell

let watchPath = __SOURCE_DIRECTORY__

let fsiSession, outStream = 
    let argv = [| "fsi.exe"; "--noninteractive" |]
    let inStream = new StringReader("")
    let outStream = new StringWriter()
    let errStream = new StringWriter()
    let fsiConfig = FsiEvaluationSession.GetDefaultConfiguration()
    let session = FsiEvaluationSession.Create(fsiConfig, argv, inStream, outStream, errStream)
    session, outStream

let onChanged (e: FileSystemEventArgs) =
    async {
        if e.FullPath = Path.Combine(__SOURCE_DIRECTORY__, __SOURCE_FILE__) then () else

        let red = "\u001b[31m"
        let reset = "\u001b[0m"

        try
            printfn $"Processing event {e.ChangeType} for file {e.FullPath}..."
            let res,diag = 
                File.ReadAllText(e.FullPath)
                |> fsiSession.EvalInteractionNonThrowing
            match res with
            | Choice1Of2 _ -> ()
            | Choice2Of2 ex ->
                printfn $"{red}Error"
                for diag in diag do
                    printfn $"{red}  {e.FullPath}:{diag.StartLine}:{diag.StartColumn} :: {diag.Message}{reset}"

            printfn "%s" (outStream.ToString())
            outStream.GetStringBuilder().Clear() |> ignore
        with ex ->
            printfn $"{red}Error processing file {e.FullPath}: {ex.Message}{reset}"
    }
    |> Async.Start

let watcher =
    let w = new FileSystemWatcher(watchPath, "*.fsx")
    w.IncludeSubdirectories <- true
    w.NotifyFilter <- NotifyFilters.FileName ||| NotifyFilters.LastWrite
    w.Changed.Add onChanged
    w.Created.Add onChanged
    w.Renamed.Add (fun e -> onChanged (e :> FileSystemEventArgs))
    w.EnableRaisingEvents <- true
    w

printfn $"Watching {watchPath} for changes (press Enter to exit)..."
Console.ReadLine() |> ignore

watcher.Dispose()
