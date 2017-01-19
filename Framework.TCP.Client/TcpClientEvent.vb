Imports System.Net
Imports Framework.TCP.Protocol
Public NotInheritable Class TcpClientEvent
    Inherits Framework.TCP.Protocol.TcpEventArgs
    Sub New(Client As Handler)
        Me.Client = Client
    End Sub
    Sub New(Client As Handler, Ex As Exception)
        Me.Client = Client
        Me.Exception = Ex
    End Sub
    Sub New(Client As Handler, Packet As Packet)
        Me.Client = Client
        Me.Packet = Packet
    End Sub
	Public Overrides Function IsFromClient() As Boolean
		Return True
    End Function
    Public Overrides Function IsFromServer() As Boolean
        Return False
    End Function
	Public Property Client As handler
End Class
