﻿#r "nuget: Pxl, 0.0.11"

open System
open Pxl
open Pxl.Ui



/// Converts HSV to RGB.
/// h: Hue in degrees (0-360)
/// s: Saturation (0.0-1.0)
/// v: Value (0.0-1.0)
/// Returns a tuple (R, G, B) where each value is in the range 0-255.
let hsva (h: float) (s: float) (v: float) a =
    let c = v * s
    let x = c * (1.0 - abs ((h / 60.0) % 2.0 - 1.0))
    let m = v - c

    let r', g', b' =
        if h < 60.0 then c, x, 0.0
        elif h < 120.0 then x, c, 0.0
        elif h < 180.0 then 0.0, c, x
        elif h < 240.0 then 0.0, x, c
        elif h < 300.0 then x, 0.0, c
        else c, 0.0, x

    let a = a * 255.0 |> int
    let r = (r' + m) * 255.0 |> int
    let g = (g' + m) * 255.0 |> int
    let b = (b' + m) * 255.0 |> int

    Color.argb(a, r, g, b)

let numbers =
    [
        [
            " XX "
            "X XX"
            "XX X"
            "X  X"
            " XX "
        ]
        [
            "  X "
            " XX "
            "  X "
            "  X "
            "  X "
        ]
        [
            " XX "
            "X  X"
            "  X "
            " X  "
            "XXXX"
        ]
        [
            "XXX "
            "   X"
            " XX "
            "   X"
            "XXX "
        ]
        [
            "X  X"
            "X  X"
            "XXXX"
            "   X"
            "   X"
        ]
        [
            "XXXX"
            "X   "
            "XXX "
            "   X"
            "XXX "
        ]
        [
            " XX "
            "X   "
            "XXX "
            "X  X"
            " XX "
        ]
        [
            "XXXX"
            "   X"
            "  X "
            "  X "
            "  X "
        ]
        [
            " XX "
            "X  X"
            " XX "
            "X  X"
            " XX "
        ]
        [
            " XX "
            "X  X"
            " XXX"
            "   X"
            " XX "
        ]
    ]

let time (now: DateTimeOffset) =
    scene {
        text
            .var4x5($"{now:HH}:{now:mm}")
            .color(Colors.white)
            .xy(1, 19)
            .color(hsva 222 0.6 0.8 1)
    }

type Snowflake = Falling | Lying | Ice | Empty
type World = Snowflake array

let getInitialWorld hours minutes =
    let drawDigit (digit: string list) (world: World) x y =
        for r in 0..4 do
            let line = digit[r]
            for c in 0..3 do
                if line[c] = 'X' then
                    world[(r + y) * 24 + c + x] <- Snowflake.Ice

    let world = Array.create (24*24) Snowflake.Empty
    let h1 = hours / 10
    let h2 = hours % 10
    let m1 = minutes / 10
    let m2 = minutes % 10

    drawDigit numbers[h1] world 1 19
    drawDigit numbers[h2] world 6 19
    drawDigit numbers[m1] world 13 19
    drawDigit numbers[m2] world 18 19

    world[(20*24) + 11] <- Snowflake.Ice
    world[(22*24) + 11] <- Snowflake.Ice

    world

let getNext (world: World): World=
    let nextWorld = Array.create 576 Snowflake.Empty
    let mutable lastSnowflakeCreated = -2
    for i in 0..575 do
        let above = if i >= 24 then Some world[i - 24] else None
        let below = if i + 24 < 576 then Some world[i + 24] else None
        let leftHill = if i >= 49 then Some world[i - 49] else None
        let rightHill = if i >= 47 then Some world[i - 47] else None
        let current = world[i]

        let state =
            match above, current, below, leftHill, rightHill with
            | _, Ice, _, _, _ -> Ice
            | None, Empty, _, _, _ ->
                if Random.Shared.Next(10) > 8 && i > lastSnowflakeCreated + 1 then
                    lastSnowflakeCreated <- i
                    Snowflake.Falling else Snowflake.Empty
            | None, Falling, Some Empty, _, _ -> Empty
            | None, Falling, Some Lying, _, _ -> Lying
            | None, Falling, Some Ice, _, _ -> Lying
            | _, Lying, _, _, _ -> Lying
            | Some Falling, Empty, Some Empty, _, _ -> Falling
            | _, Falling, Some Lying, _, _ -> Lying
            | _, Falling, Some Ice, _, _ -> Lying
            | Some Empty, Falling, Some Empty, _, _ -> Empty
            | Some Empty, Falling, Some Falling, _, _ -> Empty

            // falling hill from left
            | _, Empty, Some Lying, Some Lying, _ -> Falling
            | _, Empty, Some Ice, Some Lying, _ -> Falling
            | _, Empty, None, Some Lying, _ -> Falling

            // falling hill from right
            | _, Empty, Some Lying, _, Some Lying -> Falling
            | _, Empty, Some Ice, _, Some Lying -> Falling
            | _, Empty, None, _, Some Lying -> Falling

            | Some Lying, _, _, _, _ -> Lying
            | Some Falling, Empty, Some Lying, _, _ -> Falling
            | Some Falling, Empty, Some Ice, _, _ -> Falling
            | _, Falling, None, _, _ -> Lying
            | Some Falling, Empty, None, _, _ -> Falling
            | _ -> Empty
        nextWorld[i] <- state
    nextWorld

let snowflake = hsva 0 0 1 1
let empty = hsva 200 0.8 0.1 1.0
let ice = hsva 222 0.6 0.8 1.0
let getSnowColor snowHeight = hsva 220 (0.1 + float snowHeight * 0.025) 1.0 1.0

let getSnowHeight (world: World) c r =
    [0..r]
    |> List.sumBy (fun row ->
        match world[row * 24 + c] with
        | Empty
        | Falling -> 0
        | Lying
        | Ice -> 1)

let snowing =
    scene {
        let! ctx = getCtx()
        let! worldState = useState { getInitialWorld ctx.now.Hour ctx.now.Minute }

        let! minChanged = Trigger.valueChanged ctx.now.Minute
        let! frameTrigger = Trigger.valueChanged (ctx.now.Millisecond / 500)
        let world =
            if minChanged then
                let world = getInitialWorld ctx.now.Hour ctx.now.Minute
                do worldState.value <- world
                world
            else if frameTrigger then
                let world = worldState.value
                let nextWorld = getNext world
                do worldState.value <- nextWorld
                nextWorld
            else
                worldState.value

        world
        |> Array.mapi (fun i cell ->
            match cell with
            | Snowflake.Empty -> empty
            | Snowflake.Falling -> snowflake
            | Snowflake.Lying ->
                let height = getSnowHeight world (i % 24) (i / 24)
                getSnowColor height
            | Snowflake.Ice -> ice
        )
        |> pxls.set
    }

[<AppV1(name = "Urs Enzler - Let It Snow")>]
let all =
    scene {
        let! ctx = getCtx ()
        snowing
        //time ctx.now
    }


all |> Simulator.start "localhost"

(*
Simulator.stop ()
*)

