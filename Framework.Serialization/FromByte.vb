Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary
Public Class FromByte
    Public Shared Function Convert(src() As Byte) As Object
        Dim bf As New BinaryFormatter
        Using ms As New MemoryStream
            ms.Write(src, 0, src.Length)
            ms.Position = 0
            Return bf.Deserialize(ms)
        End Using
    End Function
    Public Shared Function Convert(Of T)(src() As Byte) As T
        Dim bf As New BinaryFormatter
        Using ms As New MemoryStream
            ms.Write(src, 0, src.Length)
            ms.Position = 0
            Return CType(bf.Deserialize(ms), T)
        End Using
    End Function
End Class
