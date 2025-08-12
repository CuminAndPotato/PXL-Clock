#r "nuget: Pxl, 0.0.11"

open System
open System.Threading
open System.Threading.Tasks
open Pxl
open Pxl.Ui
open FsHttp
open FsHttp.Operators


(*

see: https://github.com/CuminAndPotato/PXL-Clock/issues/20

HINT:
    This example will most likely be changed in the future,
    when writing own PXL Apps is not in an experimental stagem,
    when the API provides concrete ways of performing IO in a
    more controlled way.

*)



/// TODO: Move to PXL API?
 
module Task =
    
    let private startTask (newTask: _ -> Task<_>) =
        let cts = new CancellationTokenSource()
        let t = newTask cts.Token
        if t.Status = TaskStatus.Created then
            do t.Start()
        {| task = t; cts = cts |}

    /// Starts immediately a new poll and restarts every `durationInS` seconds.
    /// Cancels current polls when restarting.
    /// Retains the last result (initially defaultValue) and until a new result is available.
    let poll durationInS (defaultValue: 'a) (newPollTask: _ -> Task<'a>) =
        scene {
            let! lastPollResult = useState { defaultValue }
            let! pollState = useState { startTask newPollTask }
            let! timer = Timer.interval(durationInS)

            if timer.isElapsed then
                pollState.value.cts.Cancel()
                pollState.value <- startTask newPollTask

            lastPollResult.value <-
                match pollState.value.task.Status with
                | TaskStatus.RanToCompletion -> pollState.value.task.Result
                | _ -> lastPollResult.value

            return lastPollResult.value
        }

    let runOnce defaultValue newPollTask =
        poll Double.PositiveInfinity defaultValue newPollTask


    // don't move this to PXL API
    let map f t =
        task {
            let! r = t
            return f r
        }


// ------------------------------------------------------------
// Test
// ------------------------------------------------------------

let getRandomValue _ =
    // hard to believe that no one ever complained not being able
    // to pass cancellation token when sending requests in FsHttp
    http {
        GET "https://csrng.net/csrng/csrng.php?min=1&max=100"
    }
    |> Request.sendTAsync
    |> Task.map Response.deserializeJson<list<{| random: int |}>>
    |> Task.map _.Head.random


[<AppV1(name = "Cumin And Potato - Random Number via HTTP")>]
let all =
    scene {
        bg.color(Colors.darkBlue)

        let! randomValue = Task.poll 1.0 -1 getRandomValue
        // let! randomValue = Task.runOnce -1 getRandomValue

        text
            .mono4x5($"{randomValue}")
            .xy(7, 8)
            .color(Colors.whiteSmoke)
    }

all |> Simulator.start "localhost"

(*
Simulator.stop ()
*)
