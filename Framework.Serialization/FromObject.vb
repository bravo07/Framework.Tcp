Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary
Public Class FromObject
    Public Shared Function Convert(value As Object) As Byte()
        Dim bf As New BinaryFormatter
        Using ms As New MemoryStream
            bf.Serialize(ms, value)
            ms.Position = 0
            Return ms.ToArray
        End Using
    End Function
End Class
