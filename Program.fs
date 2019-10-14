open System
open System.IO

[<EntryPoint>]
let main args =
    if Array.isEmpty args
        then
            printfn "ソースファイルを指定してください"
            exit 1
    printfn "SourceFile: %s" args.[0]

    let stream = new FileStream ("test.class", FileMode.Create, FileAccess.Write)

    let magic = ReadOnlySpan [| byte(0xCA); byte(0xFE); byte(0xBA); byte(0xBE) |]
    stream.Write(magic)

    let version = ReadOnlySpan [| byte(0); byte(0); byte(0); byte(56) |]
    stream.Write(version)

    stream.Close ()
    0
