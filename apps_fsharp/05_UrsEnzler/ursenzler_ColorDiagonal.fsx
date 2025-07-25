﻿#r "nuget: Pxl, 0.0.11"

open System
open Pxl
open Pxl.Ui


(*

Idea and Design: Nora Enzler
Programming: Nora und Urs Enzler
Color optimizations: Nora Enzler

*)

let time (now: DateTimeOffset) =
    scene {
        let! ctx = getCtx()

        let timeText = text.var4x5($"{now:HH}:{now:mm}").color(Colors.white)
        let textWidth = timeText.measure()
        let marginLeft = (ctx.width - textWidth) / 2.0
        let marginTop = (ctx.height - timeText._data.fontSize - 1.0) / 2.0
        timeText
            .xy(marginLeft, marginTop)
            .color (Colors.white)
    }

let colors =
    [
        Color.rgb(230, 69, 69)
        Color.rgb(230, 101, 69)
        Color.rgb(230, 133, 69)
        Color.rgb(230, 165, 69)
        Color.rgb(230, 197, 69)
        Color.rgb(230, 230, 69)
        Color.rgb(197, 230, 69)
        Color.rgb(165, 230, 69)
        Color.rgb(133, 230, 69)
        Color.rgb(101, 230, 69)
        Color.rgb(69, 230, 69)
        Color.rgb(69, 230, 101)
        Color.rgb(69, 230, 133)
        Color.rgb(69, 230, 165)
        Color.rgb(69, 230, 197)
        Color.rgb(69, 230, 230)
        Color.rgb(69, 197, 230)
        Color.rgb(69, 165, 230)
        Color.rgb(69, 133, 230)
        Color.rgb(69, 101, 230)
        Color.rgb(69, 69, 230)
        Color.rgb(101, 69, 230)
        Color.rgb(133,  69, 230)
        Color.rgb(165, 69, 230)
        Color.rgb(197, 69, 230)
        Color.rgb(230, 69, 230)
        Color.rgb(230, 69, 197)
        Color.rgb(230, 69, 165)
        Color.rgb(230, 69, 133)
        Color.rgb(230, 69, 101)
    ]

let lines1 =
    [
        for i in 15..-1..0 do
            i, 0, 24, 24-i
        for i in 1..14 do
            0, i, 24-i, 24
    ]

let lines2 =
    [
        for i in 9..24 do
            i, 0, 0, i
        for i in 1..14 do
            i, 24, 24, i
    ]

let linesScene minutes (seconds: int) =
    let lMin = if seconds <= 30 then 0 else seconds - 30
    let lMax = min seconds 29
    let lines = if minutes % 2 = 0 then lines1 else lines2
    scene {
        for l in lMin..lMax do
            let color = colors[l % colors.Length]
            let x1, y1, x2, y2 = lines[l%lines.Length]
            line.p1p2(x1, y1, x2, y2).stroke(color).noAntiAlias()
    }

let diffuser =
    scene {
        rect.xywh(0, 07, 24, 9).fill(Color.argb(80, 0, 0, 0)).useAntiAlias()
        rect.xywh(0, 08, 24, 7).fill(Color.argb(80, 0, 0, 0)).useAntiAlias()
    }

[<AppV1(name = "Urs Enzler - Color Diagonal")>]
let all =
    scene {
        let! ctx = getCtx ()
        linesScene ctx.now.Minute ctx.now.Second
        diffuser
        time ctx.now
    }

all |> Simulator.start "localhost"

(*
Simulator.stop ()
*)
