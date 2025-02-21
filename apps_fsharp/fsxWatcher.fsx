#r "nuget: FSharp.Compiler.Service, 41.0.0"

open System
open System.IO
open FSharp.Compiler.Interactive.Shell

let watchPath = __SOURCE_DIRECTORY__

let fsiSession, outStream, errStream = 
    let argv = [| "fsi.exe"; "--noninteractive" |]
    let inStream = new StringReader("")
    let outStream = new StringWriter()
    let errStream = new StringWriter()
    let fsiConfig = FsiEvaluationSession.GetDefaultConfiguration()
    let session = FsiEvaluationSession.Create(fsiConfig, argv, inStream, outStream, errStream)
    session, outStream, errStream

let onChanged (e: FileSystemEventArgs) =
    let red = "\u001b[31m"
    let reset = "\u001b[0m"

    async {
        if e.FullPath = Path.Combine(__SOURCE_DIRECTORY__, __SOURCE_FILE__) then () else

        let eval command =
            let res,diag =  command |> fsiSession.EvalInteractionNonThrowing
            match res with
            | Choice1Of2 _ -> ()
            | Choice2Of2 ex ->
                printfn $"{red}Error: {ex.Message}{reset}"
                for diag in diag do
                    printfn $"{red}  {e.FullPath}:{diag.StartLine}:{diag.StartColumn} :: {diag.Message}{reset}"

        try
            printfn $"Processing event {e.ChangeType} for file {e.FullPath}..."

            let sourceDir = Path.GetDirectoryName(e.FullPath)
            let sourceFile = e.FullPath
            
            [
                $"""# silentCd @"{sourceDir}" """
                $"""# 1 @"{sourceFile}" """
                File.ReadAllText(e.FullPath)
            ]
            |> String.concat "\n"
            |> eval

            printfn "%s" (outStream.ToString())
            printfn "%s" (errStream.ToString())

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

