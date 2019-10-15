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
    
    let intToU4 (n : int) =
        let bytes = BitConverter.GetBytes n
        Array.Reverse bytes
        bytes

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
                ; "print"
                ; "(Ljava/lang/String;)V"
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

    let accessFlags = [| byte(0x00); byte(0x20) |]
    stream.Write(ReadOnlySpan accessFlags)

    let thisClass = intToU2 16
    stream.Write(ReadOnlySpan thisClass)

    let superClass = intToU2 17
    stream.Write(ReadOnlySpan superClass)

    let interfacesCount = intToU2 0
    stream.Write(ReadOnlySpan interfacesCount)

    let fieldsCount = intToU2 0
    stream.Write(ReadOnlySpan fieldsCount)

    let methodsCount = intToU2 2
    stream.Write(ReadOnlySpan methodsCount)

    let method1 =
        Array.concat
            [ intToU2 0  // access flags
            ; intToU2 7  // name index
            ; intToU2 8  // descriptor index
            ; intToU2 1  // attributes count
            ]
    stream.Write(ReadOnlySpan method1)

    let codeAttr1 =
        Array.concat
            [ intToU2 13  // attribute name index
            ; intToU4 17  // attribute length
            ; intToU2 1  // max stack
            ; intToU2 1  // max locals
            ; intToU4 5  // code length
            ;  [| byte(0x2a)  // aload_0
                ; byte(0xb7)  // invokespecial
                |]
            ; intToU2 23  // method ref (java/lang/Object.<init>:()V)
            ; [| byte(0xb1) |]  // return
            ; intToU2 0  // exception table length
            ; intToU2 0  // attributes count
            ]
    stream.Write(ReadOnlySpan codeAttr1)

    let method2 =
        Array.concat
            [ intToU2 9   // access flags
            ; intToU2 9   // name index
            ; intToU2 10  // descriptor index
            ; intToU2 1   // attributes count
            ]
    stream.Write(ReadOnlySpan method2)

    let codeAttr2 =
        Array.concat
            [ intToU2 13  // attribute name index
            ; intToU4 21  // attribute length
            ; intToU2 2  // max stack
            ; intToU2 1  // max locals
            ; intToU4 9  // code length
            ; [| byte(0xb2) |]  // getstatic
            ; intToU2 21  // field ref (java/lang/System.out:Ljava/io/PrintStream;)
            ; [| byte(0x12) |]  // ldc
            ; [| byte(15) |]  // string ref
            ; [| byte(0xb6) |]  // invokevirtual
            ; intToU2 25  // method ref (java/io/PrintStream.println:([Ljava/lang/String;)V)
            ; [| byte(0xb1) |]  // return
            ; intToU2 0  // exception table length
            ; intToU2 0  // attributes count
            ]
    stream.Write(ReadOnlySpan codeAttr2)

    let attributesCount = intToU2 0
    stream.Write(ReadOnlySpan attributesCount)

    stream.Close ()
    0
