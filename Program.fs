open System.IO

[<EntryPoint>]
let main _ =
    let stream = new FileStream ("test.class", FileMode.Create, FileAccess.Write)
    stream.WriteByte(byte(10))
    stream.Close ()
    0
