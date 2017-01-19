Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports Framework
Imports Framework.TCP.Protocol
Public Class Handler
    Inherits ClientBase
    Public Event Event_Status(EventType As TcpEvent, TcpEventArgs As TcpEventArgs)
    Sub New()
        Me.Active				= False
        Me.Accepted				= False
        Me.SendQueue			= New Queue(Of Byte())
        Me.GracefulShutdown		= New ManualResetEvent(False)
    End Sub
#Region "Routines"
    Public Sub Connect(ipStr As String, Port As Integer)
        Threads.BackgroundWorker.Run((Sub() Me.Initializer(ipStr, Port, Config.CLIENT_CONTIMEOUT)))
    End Sub
    Public Sub Send(Type As ContentType, Optional Obj As Object = Nothing)
        Me.EnqueueSend(Packet.Create(Type, Obj))
    End Sub
    Public Sub SendFile(Filename As String, Optional AttachmentID As Object = Nothing)
        Me.EnqueueSend(Packet.Create(ContentType.File, New Attachment(Filename, AttachmentID)))
    End Sub
    Public Sub Shutdown()
        Me.Active = False
    End Sub
    Private Sub Initializer(ipStr As String, Port As Integer, Timeout As Integer)
        Try
            If (Me.Active) Then
                Me.Shutdown()
                Me.GracefulShutdown.WaitOne()
            End If
			Me.EndPoint		= New IPEndPoint(IPAddress.Parse(ipStr), Port)
            Me.Socket		= New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            Me.Socket.SetIPProtectionLevel(Config.NAT_TRAVERSAL)
            Me.Socket.SetSocketOption(Config.SOCKET_LEVEL, Config.SOCKET_OPT, Config.SOCKET_VALUE)
            RaiseEvent Event_Status(TcpEvent.CONNECTING, New TcpClientEvent(Me))

            Dim result As IAsyncResult = Me.Socket.BeginConnect(Me.EndPoint, Nothing, Nothing)
            If Not result.AsyncWaitHandle.WaitOne(Timeout, True) Or (Not Me.Socket.IsConnected) Then
                Me.Socket.Close()
                Me.Socket.Dispose()
                Throw New Exception(Config.ERROR_REQTIMEOUT)
            Else
                Me.Active = True
                Threads.BackgroundWorker.Run(Sub() Me.Worker())
                Me.StartReceiving(New AsyncCallback(AddressOf Me.ReceiveCallback))
                Me.Handshake()
                RaiseEvent Event_Status(TcpEvent.CONNECTED, New TcpClientEvent(Me))
            End If
        Catch ex As Exception
            RaiseEvent Event_Status(TcpEvent.ERR, New TcpClientEvent(Me, ex))
        End Try
    End Sub
    Private Sub Worker()
        Try
            Do
                Threads.Delayed.Run(New Threads.Delayed.TaskDelegate(AddressOf Me.Cycle), Config.CYCLE_TIME).WaitOne()
            Loop While Me.Active
            RaiseEvent Event_Status(TcpEvent.DISCONNECTED, New TcpClientEvent(Me))
        Catch ex As Exception
            RaiseEvent Event_Status(TcpEvent.ERR, New TcpClientEvent(Me, ex))
        Finally
            If (Me.Socket.IsConnected) Then
                Me.Socket.Shutdown(SocketShutdown.Both)
            End If
            With Me
                .SendQueue.Clear()
                .Socket.Close()
                .Socket.Dispose()
                .GracefulShutdown.Set()
            End With
            RaiseEvent Event_Status(TcpEvent.INACTIVE, New TcpClientEvent(Me))
        End Try
    End Sub
    Private Sub ReceiveCallback(ar As IAsyncResult)
        Dim handler As Handler = CType(ar.AsyncState, Handler)
        If (handler IsNot Nothing AndAlso handler.Socket.IsConnected) Then
            handler.CopyReceivedData(handler.Socket, ar)
            If (Me.ReadBuffer.Contains(Config.ENDOFPACKET)) Then
                Dim recvd As Packet = Nothing
                If (Packet.TryCast(handler.ReadBuffer, recvd)) Then
                    Threads.BackgroundWorker.Run(Sub() Me.ProcessPacket(recvd))
                    handler.Reallocate(Buffer.Both)
                    handler.StartReceiving(New AsyncCallback(AddressOf Me.ReceiveCallback))
                Else
                    handler.Reallocate(Buffer.Both)
                    RaiseEvent Event_Status(TcpEvent.ERR, New TcpClientEvent(Me, New Exception(Config.ERROR_BADPACKET)))
                End If
            Else
                If (handler.ReadBuffer.Length <= Config.PACKET_MAXSIZE) Then
                    handler.Reallocate(Buffer.Writer)
                    Me.StartReceiving(New AsyncCallback(AddressOf Me.ReceiveCallback))
                Else
                    handler.Shutdown()
                    RaiseEvent Event_Status(TcpEvent.ERR, New TcpClientEvent(Me, New Exception(Config.ERROR_MAXREICEIVE)))
                End If
            End If
        End If
    End Sub
    Private Sub SendCallback(ar As IAsyncResult)
        Dim handler As Handler = CType(ar.AsyncState, Handler)
        If (handler.Socket.IsConnected) Then handler.Socket.EndSend(ar)
    End Sub
    Private Sub Handshake()
        If (Me.Socket.IsConnected) Then Me.EnqueueSend(Packet.Create(ContentType.Handshake))
    End Sub
    Private Sub Cycle(ByRef Finished As ManualResetEvent)
		Try
			If (Not Me.Socket.IsConnected) Then
				Me.Shutdown()
			Else
				If (Me.Accepted) Then
					Me.EnqueueSend(Packet.Create(ContentType.Synchronize))
				End If
				If (Me.SendQueue.Any) Then
					SyncLock Me.SendQueue
						Me.StartSending(Me.SendQueue.Dequeue, New AsyncCallback(AddressOf Me.SendCallback))
					End SyncLock
				End If
			End If
		Finally
			Finished.Set()	
		End Try
    End Sub
    Private Sub EnqueueSend(Data() As Byte)
        If (Me.Socket.IsConnected) Then Me.SendQueue.Enqueue(Data)
    End Sub
    Private Function StartSending(bytes() As Byte, SendCallback As AsyncCallback) As Handler
        If (Me.Socket.IsConnected) Then Me.Socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, SendCallback, Me)
        Return Me
    End Function
    Private Function StartReceiving(ReceiveCallback As AsyncCallback) As Handler
        If (Me.Socket.IsConnected) Then Me.Socket.BeginReceive(Me.WriteBuffer, 0, Me.WriteBuffer.Length, SocketFlags.None, ReceiveCallback, Me)
        Return Me
    End Function
    Private Sub ProcessPacket(Packet As Packet)
        Dim received As Object = Packet.PayloadGet
        Try
            If (Packet.ContentType = ContentType.Acknowledge) Then
                Me.Accepted = True
                RaiseEvent Event_Status(TcpEvent.ACCEPTED, New TcpClientEvent(Me))
                Return
            End If
            RaiseEvent Event_Status(TcpEvent.RECEIVED, New TcpClientEvent(Me, Packet))
        Catch ex As Exception
            RaiseEvent Event_Status(TcpEvent.ERR, New TcpClientEvent(Me, ex))
        End Try
    End Sub
#End Region
#Region "Properties"
    Private m_sendqueue As Queue(Of Byte())
    Protected Friend Property SendQueue As Queue(Of Byte())
        Get
            Return Me.m_sendqueue
        End Get
        Set(value As Queue(Of Byte()))
            Me.m_sendqueue = value
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
    Private m_socket As Socket
    Protected Friend Property Socket As Socket
        Get
            Return Me.m_socket
        End Get
        Set(value As Socket)
            Me.m_socket = value
        End Set
    End Property
    Private m_active As Boolean
    Protected Friend Property Active As Boolean
        Get
            Return Me.m_active
        End Get
        Set(value As Boolean)
            Me.m_active = value
        End Set
    End Property
    Private m_gracefulshutdown As ManualResetEvent
    Protected Friend Property GracefulShutdown As ManualResetEvent
        Get
            Return Me.m_gracefulshutdown
        End Get
        Set(value As ManualResetEvent)
            Me.m_gracefulshutdown = value
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
