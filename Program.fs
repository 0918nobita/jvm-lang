open System
open System.IO
open System.Text

[<EntryPoint>]
let main args =
    if Array.isEmpty args
        then
            printfn "ソースファイルを指定してください"
            exit 1

    let src = File.ReadAllText args.[0]

    let className = Path.GetFileNameWithoutExtension args.[0]

    let stream = new FileStream (className + ".class", FileMode.Create, FileAccess.Write)

    let magic = ReadOnlySpan [| byte(0xca); byte(0xfe); byte(0xba); byte(0xbe) |]
    stream.Write(magic)

    let version = ReadOnlySpan [| byte(0); byte(0); byte(0); byte(56) |]
    stream.Write(version)

    let constantPoolCount = ReadOnlySpan [| byte(0x00); byte(0x1a) |]
    stream.Write(constantPoolCount)

    let intToU2 (n : int) =
        let bytes = BitConverter.GetBytes n
        Array.Reverse bytes
        bytes.[2..3]

    let utf8contents =
        Array.map<String, byte []>
            (fun s ->
                Array.append
                    ((byte(1) :: (intToU2(String.length s) |> Array.toList))
                    |> List.toArray)
                    (Encoding.UTF8.GetBytes s))
                [| className
                ; "java/lang/Object"
                ; "java/lang/System"
                ; "out"
                ; "java/io/PrintStream"
                ; "Ljava/io/PrintStream;"
                ; "<init>"
                ; "()V"
                ; "main"
                ; "([Ljava/lang/String;)V"
                ; "println"
                ; "(I)V"
                ; "Code"
                ; src
                |]
        |> Array.concat
    stream.Write(ReadOnlySpan utf8contents)

    let stringEntity = byte(8) :: ((intToU2 14) |> Array.toList) |> List.toArray
    stream.Write(ReadOnlySpan stringEntity)

    stream.Close ()
    0
