Imports System.IO
Imports System.IO.Compression
Public Class Deflation
    Public Shared Function Compress(src As Byte(), Level As CompressionLevel) As Byte()
        Using ms As New MemoryStream
            Using gzs As New DeflateStream(ms, Level, True)
                gzs.Write(src, 0, src.Length)
            End Using
            Return ms.ToArray
        End Using
    End Function
    Public Shared Function Decompress(src As Byte(), Optional bufferLen As Integer = &H400) As Byte()
        Using gzs As New DeflateStream(New MemoryStream(src), CompressionMode.Decompress)
            Dim length As Integer = 0, buffer As Byte() = New Byte(bufferLen) {}
            Using ms As New MemoryStream
                Do
                    length = gzs.Read(buffer, 0, bufferLen)
                    If length > 0 Then
                        ms.Write(buffer, 0, length)
                    End If
                Loop While length > 0
                Return ms.ToArray()
            End Using
        End Using
    End Function
End Class
