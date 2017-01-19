Imports System.Net
Imports Framework.TCP.Protocol
Public NotInheritable Class TcpServerEvent
    Inherits Framework.TCP.Protocol.TcpEventArgs
    Sub New(Listener As Listener)
        Me.Listener = Listener
    End Sub
    Sub New(Listener As Listener, Client As Handler)
        Me.Listener = Listener
        Me.Client = Client
    End Sub
    Sub New(Listener As Listener, Ex As Exception)
        Me.Listener = Listener
        Me.Exception = Ex
    End Sub
    Sub New(Sender As Listener, Client As Handler, Packet As Packet)
        Me.Client = Client
        Me.Listener = Sender
        Me.Packet = Packet
    End Sub
    Public Overrides Function IsFromClient() As Boolean
        Return False
    End Function
    Public Overrides Function IsFromServer() As Boolean
        Return True
    End Function
	Public Property Client As Handler
	Public Property Listener As Listener
End Class
