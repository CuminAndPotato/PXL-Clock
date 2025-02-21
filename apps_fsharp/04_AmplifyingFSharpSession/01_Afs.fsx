#r "nuget: Pxl, 0.0.3"

open System
open Pxl
open Pxl.Ui



// a list comprehension yields elements and returns a list of elements
[
    1
    2
    3
    for x in 4 .. 10 do
        x
]


// scene comprehension yields pxls (or other elements) and triggers side effects (paint)
scene {
    // bg(Colors.white)

    pxl.xy(0.0, 0.0).stroke(Colors.yellow)
    pxl.xy(23.0, 0).stroke(Colors.red)
    pxl.xy(23.0, 23.0).stroke(Colors.green)
    pxl.xy(0, 23.0).stroke(Colors.blue)

    rect.xywh(6, 6, 12, 12).fill(Colors.red)

    for x in 0 .. 23 do
        pxl.xy(x, 3).stroke(Colors.blueViolet)
        pxl.xy(x, 4).stroke(Colors.darkKhaki)
}
|> Simulator.start "localhost"

