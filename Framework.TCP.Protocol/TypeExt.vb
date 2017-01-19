Imports System.IO
Imports System.IO.Compression
Imports System.Net.Sockets
Imports Framework.Serialization
Imports Framework.TCP.Protocol.Config
Public Module TypeExt
    <System.Runtime.CompilerServices.Extension>
    Public Function PayloadSet(ByRef packet As Packet, payload As Byte()) As Packet
        packet.Payload = New Byte(payload.Length - 1) {}
        Array.Copy(payload, packet.Payload, payload.Length)
        packet.Checksum = packet.Payload.Checksum(Config.PACKET_CHECKSUMTYPE)
        Return packet
    End Function
    <System.Runtime.CompilerServices.Extension>
    Public Function PayloadGet(packet As Packet) As Object
        If (packet.Serialized) Then Return FromByte.Convert(packet.Payload) Else Return packet.Payload
    End Function
    <System.Runtime.CompilerServices.Extension>
    Public Function PayloadValidate(packet As Packet) As Boolean
        Return packet.Payload IsNot Nothing AndAlso packet.Payload.Checksum(Config.PACKET_CHECKSUMTYPE).SequenceEqual(packet.Checksum)
    End Function
    <System.Runtime.CompilerServices.Extension>
    Public Sub SaveAs(src As Byte(), filename As String, Optional Overwrite As Boolean = False)
        If (src IsNot Nothing) Then
            If (File.Exists(filename) AndAlso Overwrite) Then File.Delete(filename)
            Using bw As New BinaryWriter(File.Open(filename, FileMode.OpenOrCreate))
                bw.Write(src)
            End Using
        End If
    End Sub
    <System.Runtime.CompilerServices.Extension>
    Public Function Remove(src As Byte(), sequence As Byte()) As Byte()
        If (src.Contains(sequence)) Then
            Dim dst() As Byte = New Byte((src.Length - 1) - sequence.Length) {}
            Array.Copy(src, dst, src.Length - sequence.Length)
            Return dst
        End If
        Return src
    End Function
    <System.Runtime.CompilerServices.Extension>
    Public Function Append(ByRef src As Byte(), sequence As Byte()) As Byte()
        If (Not src.Contains(sequence)) Then
            Array.Resize(Of Byte)(src, src.Length + sequence.Length)
            sequence.CopyTo(src, src.Length - sequence.Length)
            Return src
        End If
        Return src
    End Function
    <System.Runtime.CompilerServices.Extension>
    Public Function Contains(src As Byte(), sequence As Byte()) As Boolean
        If (src.Length >= sequence.Length) Then
            Dim bytes() As Byte = New Byte(sequence.Length - 1) {}
            Array.Copy(src, src.Length - sequence.Length, bytes, 0, sequence.Length)
            Return bytes.SequenceEqual(sequence)
        End If
        Return False
    End Function
	<Runtime.CompilerServices.Extension>
	Public Function HexStr(src() As Byte) As String
		Return String.Join(String.Empty, Array.ConvertAll(src, Function(b) b.ToString("X2")))
	End Function
	<Runtime.CompilerServices.Extension>
	Public Function Randomize(ByRef src() As Byte) As Byte()
		Using rng As New Security.Cryptography.RNGCryptoServiceProvider()
			rng.GetNonZeroBytes(src)
			Return src
		End Using
	End Function
    <Runtime.CompilerServices.Extension>
    Public Function Checksum(src() As Byte, Type As HashType) As Byte()
        Dim result As Byte() = Nothing
        Select Case Type
            Case HashType.SHA256
                Using Cryptograph As Security.Cryptography.SHA256 = Security.Cryptography.SHA256.Create()
                    result = Cryptograph.ComputeHash(src)
                End Using
            Case HashType.SHA384
                Using Cryptograph As Security.Cryptography.SHA384 = Security.Cryptography.SHA384.Create()
                    result = Cryptograph.ComputeHash(src)
                End Using
            Case HashType.SHA512
                Using Cryptograph As Security.Cryptography.SHA512 = Security.Cryptography.SHA512.Create()
                    result = Cryptograph.ComputeHash(src)
                End Using
        End Select
        Return result
    End Function
    <System.Runtime.CompilerServices.Extension>
    Public Function IsConnected(Handler As TcpClient, Optional ByRef Err As Exception = Nothing) As Boolean
        Return Handler.Client.IsConnected(Err)
    End Function
    <System.Runtime.CompilerServices.Extension>
    Public Function IsConnected(Handler As Socket, Optional ByRef Err As Exception = Nothing) As Boolean
        If (Handler IsNot Nothing AndAlso Handler.Connected) Then
            Try
                If (Handler.Poll(0, SelectMode.SelectRead)) Then
                    If Handler.Receive(New Byte(0) {}, SocketFlags.Peek) = 0 Then
                        Return False
                    End If
                End If
                Return True
            Catch ex As SocketException
                Err = ex
                Return False
            End Try
        Else
            Return False
        End If
    End Function
End Module

