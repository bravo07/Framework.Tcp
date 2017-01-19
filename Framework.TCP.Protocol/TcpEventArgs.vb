Public MustInherit Class TcpEventArgs
    Inherits EventArgs
    Public Property Packet As Packet
    Public Property Exception As Exception
    Public MustOverride Function IsFromServer() As Boolean
    Public MustOverride Function IsFromClient() As Boolean
	Public Function HasPacket() As Boolean
		Return Me.Packet isnot Nothing
	End Function
	Public Function HasAttachment() As Boolean
		Return Me.HasPacket AndAlso TypeOf Me.Packet.PayloadGet is Attachment
	End Function
	Public Function GetAttachement() As Attachment
		Return TryCast(Me.Packet.PayloadGet, Attachment)
	End Function
End Class
