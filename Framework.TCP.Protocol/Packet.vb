Imports Framework.Compression
Imports Framework.Serialization
<Serializable>
Public NotInheritable Class Packet
    Sub New()
        Me.Timestamp = DateTime.Now
    End Sub
    Public Shared Function Create(Type As ContentType, Optional Obj As Object = Nothing) As Byte()
        Dim p As New Packet With {.ContentType = Type, .Version = Version.Current}
        If (Obj IsNot Nothing AndAlso Obj.GetType.IsSerializable) Then
            p.PayloadSet(FromObject.Convert(Obj))
            p.Checksum = p.Payload.Checksum(Config.PACKET_CHECKSUMTYPE)
            p.Serialized = True
        ElseIf (Obj IsNot Nothing AndAlso Not Obj.GetType.IsSerializable) Then
            Throw New Exception(String.Format("Unable to create packet from '{0}'", Obj.GetType.Name))
        ElseIf (Obj Is Nothing) Then
            p.Payload = New Byte(0) {&H0}
            p.Checksum = p.Payload.Checksum(Config.PACKET_CHECKSUMTYPE)
            p.Serialized = False
        End If
        Return Deflation.Compress(FromObject.Convert(p), Config.PACKET_COMPRESSLEVEL).Append(Config.ENDOFPACKET)
    End Function
    Public Shared Function [TryCast](bytes As Byte(), ByRef result As Packet) As Boolean
        Dim obj As Object = FromByte.Convert(Deflation.Decompress(bytes.Remove(Config.ENDOFPACKET)))
        If (obj IsNot Nothing AndAlso obj.GetType.IsAssignableFrom(GetType(Packet))) Then
            Dim packet As Packet = CType(obj, Packet)
            If (packet.PayloadValidate) Then
                result = packet
                Return True
            End If
        End If
        Return False
    End Function
    Public Property Payload As Byte()
    Public Property Checksum As Byte()
    Public Property Serialized As Boolean
    Public Property Version As Version
    Public Property Timestamp As DateTime
    Public Property ContentType As ContentType
End Class