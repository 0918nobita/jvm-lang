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

    let classes = Array.collect (fun index -> Array.append [| byte(7) |] <| intToU2 index) [| 1; 2; 3; 5 |]
    stream.Write(ReadOnlySpan classes)

    let nameAndType1 = Array.concat [[| byte(0x0c) |]; (intToU2 4); (intToU2 6)]
    stream.Write(ReadOnlySpan nameAndType1)

    let fieldRef = Array.concat [[| byte(9) |]; (intToU2 18); (intToU2 20)]
    stream.Write(ReadOnlySpan fieldRef)

    let nameAndType2 = Array.concat [[| byte(0x0c) |]; (intToU2 7); (intToU2 8)]
    stream.Write(ReadOnlySpan nameAndType2)

    let methodRef1 = Array.concat [[| byte(0x0a) |]; (intToU2 17); (intToU2 22)]
    stream.Write(ReadOnlySpan methodRef1)

    let nameAndType3 = Array.concat [[| byte(0x0c) |]; (intToU2 11); (intToU2 12)]
    stream.Write(ReadOnlySpan nameAndType3)

    let methodRef2 = Array.concat [[| byte(0x0a);|]; (intToU2 19); (intToU2 24)]
    stream.Write(ReadOnlySpan methodRef2)

    stream.Close ()
    0
