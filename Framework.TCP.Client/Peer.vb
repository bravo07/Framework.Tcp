Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading
Imports TCP.Protocol
Public Class Peer
    Inherits Protocol.TCPBase
    Public Event ClientStatus(Sender As Object, ev As TCPEvents, Param As Object)
    Sub New(Encoder As Encoding)
        Me.Running = False
        Me.SendQueue = New Queue(Of Byte())
    End Sub
#Region "Public Routines"
    Public Sub Connect(ipAddr As String, Port As Integer, Optional Timeout As Integer = 5)
        Me.UpdateStatus(Me, TCPEvents.CONNECTING)
        If (Me.Running) Then
            Me.Running = False
            Me.ThreadReset.WaitOne()
        End If

        Me.Socket = New TcpClient
        Me.ThreadReset = New AutoResetEvent(False)
        Dim connectionResult As IAsyncResult = Me.Socket.BeginConnect(IPAddress.Parse(ipAddr), Port, Nothing, Nothing)
        If (connectionResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(Timeout))) Then
            If (Me.Socket.Connected) Then
                Me.Socket.EndConnect(connectionResult)
                Threads.Create.Worker(Sub() Me.Run(), True)
                Me.UpdateStatus(Me, TCPEvents.CONNECTED)
                Return
            End If
        End If
        Me.Running = False
        Me.UpdateStatus(Me, TCPEvents.DISCONNECTED)
    End Sub
    Public Sub Send(Obj As IPacket)
        If (Me.Socket.Connected) Then
            Me.UpdateStatus(Me, TCPEvents.SEND, Obj)
            Me.SendQueue.Enqueue(Packet.Assemble(Obj))
        End If
    End Sub
    Public Sub Abort()
        Me.Running = False
    End Sub
#End Region
#Region "Private Routines"
    Private Sub Run()
        Me.Running = True
        Me.Stream = Me.Socket.GetStream
        Do While Me.Running
            If (Me.Stream.CanRead AndAlso Me.Socket.Available > 0) Then
                Dim data() As Byte = New Byte(Me.Socket.Available) {}
                Me.Stream.Read(data, 0, data.Length)
                If (data.Contains(Packet.HEADER) And data.Contains(Packet.EOF)) Then
                    Me.Buffer.Clear()
                    Me.Buffer.AddRange(data)
                    Threads.Create.Worker(Sub() Me.PeerProcess(Packet.TryConvert(Me.Buffer.ToArray)), True)
                ElseIf (data.Contains(Packet.HEADER) And Not data.Contains(Packet.EOF)) Then
                    Me.Clear()
                    Me.IsReceiving = True
                    Me.Write(data)
                ElseIf (Not data.Contains(Packet.HEADER) And Not data.Contains(Packet.EOF)) Then
                    If (Me.IsReceiving) Then
                        Me.Write(data)
                        Me.UpdateStatus(Me, TCPEvents.DEBUG, String.Format(String.Format("Receiving {0}", Me.Buffer.Count)))
                    Else
                        Me.IsReceiving = False
                        Me.Write(data)
                        If (Packet.Validate(Me.Buffer.ToArray)) Then
                            Threads.Create.Worker(Sub() Me.PeerProcess(Packet.TryConvert(Me.Buffer.ToArray)), True)
                        Else
                            Me.Clear()
                        End If
                    End If
                ElseIf (Not data.Contains(Packet.HEADER) And data.Contains(Packet.EOF)) Then

                End If
            End If
            If (Me.Stream.CanWrite) AndAlso (Me.SendQueue.Count > 0) Then
                Dim buffer() As Byte = Me.SendQueue.Dequeue
                Me.Stream.Write(buffer, 0, buffer.Length)
            End If
        Loop

        Me.SendQueue.Clear()
        Me.UpdateStatus(Me, TCPEvents.DISCONNECTED)
        Me.Socket.Close()
        Me.ThreadReset.Set()
    End Sub
    Private Sub PeerProcess(Package As Object)
        Me.UpdateStatus(Me, TCPEvents.RECEIVED, Package)
    End Sub
    Private Sub UpdateStatus(Sender As Object, ev As TCPEvents, Optional Param As Object = Nothing)
        RaiseEvent ClientStatus(Sender, ev, Param)
    End Sub
#End Region
#Region "Private Properties"
    Private m_threadreset As AutoResetEvent
    Private Property ThreadReset As AutoResetEvent
        Get
            Return Me.m_threadreset
        End Get
        Set(value As AutoResetEvent)
            Me.m_threadreset = value
        End Set
    End Property
#End Region
#Region "Pulic Properties"
    Private m_running As Boolean
    Public Property Running As Boolean
        Get
            Return Me.m_running
        End Get
        Set(value As Boolean)
            Me.m_running = value
        End Set
    End Property
    Private m_stream As NetworkStream
    Public Property Stream As NetworkStream
        Get
            Return Me.m_stream
        End Get
        Set(value As NetworkStream)
            Me.m_stream = value
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
    Private m_sendqueue As Queue(Of Byte())
    Public Property SendQueue As Queue(Of Byte())
        Get
            Return Me.m_sendqueue
        End Get
        Set(value As Queue(Of Byte()))
            Me.m_sendqueue = value
        End Set
    End Property
#End Region
End Class