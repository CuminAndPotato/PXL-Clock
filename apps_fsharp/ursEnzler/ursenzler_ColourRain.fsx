﻿#r "nuget: Pxl, 0.0.11"

open System
open Pxl
open Pxl.Ui



(*

Idea and Design: Nico und Urs Enzler
Programming: Nico und Urs Enzler
Color optimizations: Nico Enzler

*)

/// Converts HSV to RGB.
/// h: Hue in degrees (0-360)
/// s: Saturation (0.0-1.0)
/// v: Value (0.0-1.0)
let hsv (h: float) (s: float) (v: float) =
    Color.hsv(h, s, v).opacity(0.7)

let time hour minute second =
    scene {
        text.var10x10($"%d{hour}").xy(0,1).color(hsv 220.0 0.5 1)
        text.var10x10($"%02d{minute}").xy(2,10).color(hsv 20.0 0.5 1)
        text.var4x5($"%02d{second}").xy(15, 19).color(hsv 100 0.5 1)
    }

let offsets =
    [
        10; 4; 17; 7; 12; 1; 13; 19; 9; 14; 1; 7; 18; 9; 5; 17; 8; 4; 9; 19; 2; 6; 13; 17
    ]

let rain step =
    scene {
        for i in 0..23 do
            let offset = offsets[i]
            line.p1p2(i, (step + offset) % 24, i, 3 + (step + offset) % 24)
                .stroke(hsv (0.0 + (float (i * 15))) 0.8 1.0 |> _.opacity(0.6))
                .noAntiAlias()
    }

[<AppV1(name = "Urs Enzler - Colour Rain")>]
let all =
    scene {
        let! ctx = getCtx ()

        rain (ctx.now.Second % 24)
        time ctx.now.Hour ctx.now.Minute ctx.now.Second
    }

all |> Simulator.start "localhost"

(*
Simulator.stop ()
*)
