Imports System.Net.Sockets
Imports Framework.TCP.Protocol
Imports System.Net

Public Class Handler
    Inherits ClientBase
    Public Delegate Sub ClientDroppedCallback(Handler As Handler, Message As String, DropConnection As Boolean)
    Sub New(Socket As TcpClient, ClientDroppedCallback As ClientDroppedCallback)
        Me.Active					= True
        Me.Accepted					= False
        Me.Socket					= Socket
        Me.ClientDropped			= ClientDroppedCallback
		Me.SendQueue				= New Queue(Of Byte())
        Me.EndPoint					= Socket.Client.RemoteEndPoint
        Me.m_firstseen				= DateTime.Now
        Me.m_lastresponse			= DateTime.Now
    End Sub
#Region "Routines"
    Public Sub Shutdown()
        Me.SendQueue.Clear()
        If (Me.Socket.IsConnected) Then
            Me.Socket.Client.Shutdown(SocketShutdown.Both)
        End If
        Me.Socket.Close()
        Me.Active = False
    End Sub
    Protected Friend Function BeginSend(bytes() As Byte, SendCallback As AsyncCallback) As Handler
	Try
	    If (Me.Socket IsNot Nothing AndAlso Me.Socket.IsConnected) Then
            Me.Socket.Client.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, SendCallback, Me)
        End If
	Catch ex As SocketException
		Me.Active = False
        Me.ClientDropped.Invoke(Me, ex.Message, True)
	End Try
		Return Me
    End Function
    Protected Friend Function BeginReceive(ReceiveCallback As AsyncCallback) As Handler
	Try
		If (Me.Socket IsNot Nothing AndAlso Me.Socket.IsConnected) Then
			Me.Socket.Client.BeginReceive(Me.WriteBuffer, 0, Me.WriteBuffer.Length, SocketFlags.None, ReceiveCallback, Me)
        End If
	Catch ex As SocketException
		Me.Active = False
        Me.ClientDropped.Invoke(Me, ex.Message, True)
	End Try
	Return Me
    End Function
    Protected Friend Sub Cycle(SendCallback As AsyncCallback)
        If (Me.Socket IsNot Nothing) Then
            If (Not Me.Socket.IsConnected) Then
                Me.Active = False
                Me.ClientDropped.Invoke(Me, Config.ERROR_CLIENTLOST, True)
            ElseIf ((DateTime.Now - Me.LastResponse).Seconds >= Config.RESPONSE_MAXTIME) Then
                Me.Active = False
                Me.ClientDropped.Invoke(Me, Config.ERROR_CLIENTTIMEOUT, True)
            ElseIf (Me.Socket.IsConnected AndAlso Me.SendQueue.Any) Then
                Me.BeginSend(Me.SendQueue.Dequeue, SendCallback)
            End If
        End If
    End Sub
    Protected Friend Sub Synchronize(PacketTimeStamp As DateTime)
        Me.m_lastresponse = PacketTimeStamp
    End Sub
#End Region
#Region "Properties"
    Private m_lastresponse As DateTime
    Public ReadOnly Property LastResponse As DateTime
        Get
            Return Me.m_lastresponse
        End Get
    End Property
    Private m_firstseen As DateTime
    Public ReadOnly Property FirstSeen As DateTime
        Get
            Return Me.m_firstseen
        End Get
    End Property
    Private m_sendqueue As Queue(Of Byte())
    Public Property SendQueue As Queue(Of Byte())
        Get
            Return Me.m_sendqueue
        End Get
        Set(value As Queue(Of Byte()))
            Me.m_sendqueue = value
        End Set
    End Property
    Private m_active As Boolean
    Public Property Active As Boolean
        Get
            Return Me.m_active
        End Get
        Set(value As Boolean)
            Me.m_active = value
        End Set
    End Property
    Private m_endpoint As EndPoint
    Public Property EndPoint As EndPoint
        Get
            Return Me.m_endpoint
        End Get
        Set(value As EndPoint)
            Me.m_endpoint = value
        End Set
    End Property
    Private m_socket As TcpClient
    Public Property Socket As TcpClient
        Get
            Return Me.m_socket
        End Get
        Set(value As TcpClient)
            Me.m_socket = value
        End Set
    End Property
    Private m_cldropped As ClientDroppedCallback
    Protected Friend Property ClientDropped As ClientDroppedCallback
        Get
            Return Me.m_cldropped
        End Get
        Set(value As ClientDroppedCallback)
            Me.m_cldropped = value
        End Set
    End Property
    Private m_accepted As Boolean
    Protected Friend Property Accepted As Boolean
        Get
            Return Me.m_accepted
        End Get
        Set(value As Boolean)
            Me.m_accepted = value
        End Set
    End Property
#End Region
End Class