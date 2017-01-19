Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports System.Windows.Forms
Imports Framework
Imports Framework.TCP.Protocol
Public Class Listener
    Public Event Event_Status(EventType As TcpEvent, TcpEventArgs As TcpEventArgs)
    Sub New()
        Me.Active				= False
		Me.Clients				= New List(Of Handler)
        Me.GracefulShutdown		= New ManualResetEvent(False)
        Me.BusyAcceptingClient	= New ManualResetEvent(False)
    End Sub
#Region "Routines"
    Public Sub Start(Port As Integer)
		If (Port < Config.PORT_SAFERANGE) then
			Throw New Exception(Config.ERR_BADPORT)
		End If
        Threads.BackgroundWorker.Run(Sub() Me.Worker(Port))
    End Sub
    Public Sub Send(Handler As Handler, Type As ContentType, Optional obj As Object = Nothing)
        Me.EnqueueSend(Handler, Packet.Create(Type, obj))
    End Sub
    Public Sub SendFile(Handler As Handler, Filename As String, Optional AttachmentID As Object = Nothing)
        Me.EnqueueSend(Handler, Packet.Create(ContentType.File, New Attachment(Filename, AttachmentID)))
    End Sub
    Public Function Shutdown() As ManualResetEvent
        Me.Active = False
        Return Me.GracefulShutdown
    End Function
    Public Sub Shutdown(Handler As Handler)
        Handler.Shutdown()
    End Sub
	Private Sub Worker(Port As Integer)
        Try
            If (Me.Active) Then
                Me.Shutdown()
                Me.GracefulShutdown.WaitOne()
            End If
            Me.Active = True
            Me.EndPoint = New IPEndPoint(IPAddress.Any, Port)
            Me.Listener = New TcpListener(Me.EndPoint)
            Me.Listener.Server.SetIPProtectionLevel(Config.NAT_TRAVERSAL)
            Me.Listener.Server.SetSocketOption(Config.SOCKET_LEVEL, Config.SOCKET_OPT, Config.SOCKET_VALUE)
            Me.Listener.Start()
            Me.GracefulShutdown.Reset()
            RaiseEvent Event_Status(TcpEvent.LISTENING, New TcpServerEvent(Me))
            Do
                If (Me.Listener.Pending) Then
                    Me.BusyAcceptingClient.Reset()
                    Threads.BackgroundWorker.Run(Sub() Me.AcceptCallback(Me.Listener.AcceptTcpClient))
                    Me.BusyAcceptingClient.WaitOne()
                End If
                Threads.Delayed.Run(New Threads.Delayed.TaskDelegate(AddressOf Me.Cycle), Config.CYCLE_TIME).WaitOne()
            Loop While Me.Active
        Catch ex As Exception
            RaiseEvent Event_Status(TcpEvent.ERR, New TcpServerEvent(Me, ex))
        Finally
            Me.CleanUp.WaitOne()
            Me.GracefulShutdown.Set()
            RaiseEvent Event_Status(TcpEvent.INACTIVE, New TcpServerEvent(Me))
        End Try
    End Sub
	Private Sub AcceptCallback(Socket As TcpClient)
		try
			Socket.Client.SetSocketOption(Config.SOCKET_LEVEL, Config.SOCKET_OPT, Config.SOCKET_VALUE)
			Dim handler As Handler = New Handler(Socket, AddressOf Me.HandlerMessageCallback).BeginReceive(New AsyncCallback(AddressOf Me.ReceiveCallback))
			Me.Clients.Add(handler)
            RaiseEvent Event_Status(TcpEvent.NEWCONNECTION, New TcpServerEvent(Me, handler))
		Finally
			Me.BusyAcceptingClient.Set()
		End Try
    End Sub
    Private Sub ReceiveCallback(ar As IAsyncResult)
        Dim handler As Handler = CType(ar.AsyncState, Handler)
        If (handler IsNot Nothing AndAlso handler.Socket.IsConnected) Then
            handler.CopyReceivedData(handler.Socket.Client, ar)
            If (handler.ReadBuffer.Contains(Config.ENDOFPACKET)) Then
                Dim recvd As Packet = Nothing
                If (Packet.TryCast(handler.ReadBuffer, recvd)) Then
                    Threads.BackgroundWorker.Run(Sub() Me.ProcessPacket(handler, recvd))
                    handler.Reallocate(Buffer.Both)
                    handler.BeginReceive(New AsyncCallback(AddressOf Me.ReceiveCallback))
                Else
                    handler.Reallocate(Buffer.Both)
                    RaiseEvent Event_Status(TcpEvent.ERR, New TcpServerEvent(Me, New Exception(Config.ERROR_BADPACKET)))
                End If
            Else
                If (handler.ReadBuffer.Length <= Config.PACKET_MAXSIZE) Then
                    handler.Reallocate(Buffer.Writer)
                    handler.BeginReceive(New AsyncCallback(AddressOf Me.ReceiveCallback))
                Else
                    Me.Shutdown(handler)
                    RaiseEvent Event_Status(TcpEvent.ERR, New TcpServerEvent(Me, New Exception(Config.ERROR_MAXREICEIVE)))
                End If
            End If
        End If
    End Sub
    Private Sub SendCallback(ar As IAsyncResult)
        Dim handler As Handler = CType(ar.AsyncState, Handler)
        If (handler.Socket.IsConnected) Then handler.Socket.Client.EndSend(ar)
    End Sub
    Private Sub EnqueueSend(Handler As Handler, bytes() As Byte)
        If (Handler.Socket.IsConnected) Then Handler.SendQueue.Enqueue(bytes)
    End Sub
    Private Sub Cycle(ByRef Finished As ManualResetEvent)
        Try
            If (Me.Clients.Any) Then
                For Each handler As Handler In Me.Clients
                    handler.Cycle(New AsyncCallback(AddressOf Me.SendCallback))
                Next
                Me.Clients.RemoveAll(Function(handler As Handler) Not handler.Active)
            End If
        Finally
            Finished.Set()
        End Try
    End Sub
    Private Sub HandlerMessageCallback(Handler As Handler, Message As String, DropConnection As Boolean)
        If (DropConnection) Then Me.Shutdown(Handler)
        RaiseEvent Event_Status(TcpEvent.ERR, New TcpServerEvent(Me, New Exception(Message)))
    End Sub
    Private Sub ProcessPacket(Handler As Handler, Packet As Packet)
        Dim received As Object = Packet.PayloadGet
        Try
            If (Not Handler.Accepted) Then
                If (Packet.ContentType = ContentType.Handshake AndAlso Packet.Version = Version.Current) Then
                    Handler.Accepted = True
                    Me.EnqueueSend(Handler, Packet.Create(ContentType.Acknowledge))
                    RaiseEvent Event_Status(TcpEvent.HANDSHAKE, New TcpServerEvent(Me, Handler))
                    Return
                Else
                    Me.Shutdown(Handler)
                    RaiseEvent Event_Status(TcpEvent.ERR, New TcpServerEvent(Me, New Exception(Config.ERROR_BADHANDSHAKE)))
                    Return
                End If
            Else
                If (Packet.ContentType = ContentType.Synchronize) Then
                    Handler.Synchronize(Packet.Timestamp)
                    Return
                End If
                RaiseEvent Event_Status(TcpEvent.RECEIVED, New TcpServerEvent(Me, Handler, Packet))
            End If
        Catch ex As Exception
            RaiseEvent Event_Status(TcpEvent.ERR, New TcpServerEvent(Me, ex))
        End Try
    End Sub
    Private Function CleanUp() As ManualResetEvent
        Dim done As New ManualResetEvent(False)
        Try
            With Me
				If (.Clients.Any) then
					.Clients.ForEach(Sub(handler) handler.Shutdown())
				End If
                .Clients.Clear()
                .Listener.Stop()
                .Listener.Server.Close()
                .Listener.Server.Dispose()
            End With
            done.Set()
            Return done
        Finally
            done.WaitOne()
        End Try
    End Function
#End Region
#Region "Properties"
	Private m_clients As List(Of Handler)
    Public Property Clients As List(Of Handler)
        Get
            Return Me.m_clients
        End Get
        Set(value As List(Of Handler))
            Me.m_clients = value
        End Set
    End Property
	Private m_endpoint As IPEndPoint
    Public Property EndPoint As IPEndPoint
        Get
            Return Me.m_endpoint
        End Get
        Set(value As IPEndPoint)
            Me.m_endpoint = value
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
    Private m_listener As TcpListener
    Protected Friend Property Listener As TcpListener
        Get
            Return Me.m_listener
        End Get
        Set(value As TcpListener)
            Me.m_listener = value
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
    Private m_busyaccepting As ManualResetEvent
    Protected Friend Property BusyAcceptingClient As ManualResetEvent
        Get
            Return Me.m_busyaccepting
        End Get
        Set(value As ManualResetEvent)
            Me.m_busyaccepting = value
        End Set
    End Property
#End Region
End Class
