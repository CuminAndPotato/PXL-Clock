#r "nuget: Pxl, 0.0.1-preview.1"
open PxlLocalDevShadow

open System
open Pxl
open Pxl.Ui

let createCanvas = CanvasProxy.createWithDefaults "localhost"


// -------------------------------------------------------------


// TODO: We have bug in here, so please be patient - we will fix it soon
// and update the repository. Thank you!
let finalScene =
    scene {
        shaderGenerative (fun p ->
            if p.x % 2.0 = 0 
            then Colors.red
            else Colors.blue
        )

        text.var4x5("abc").color(Colors.white)

        shaderEffect (fun inp ->
            if inp.pxlColor = Colors.red
            then Colors.green
            else inp.pxlColor
        )
    }


finalScene |> Simulator.start createCanvas


(*
Simulator.stop ()
*)

