Imports Framework
Imports Framework.TCP.Server
Imports Framework.TCP.Client
Imports Framework.TCP.Protocol

Public Class frmMain
    Public Clients As List(Of TCP.Client.Handler)
    Public WithEvents Server As TCP.Server.Listener
    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
		Me.Server = New TCP.Server.Listener
        AddHandler Me.Server.Event_Status, AddressOf Me.Event_Status

		Me.Clients = New List(Of TCP.Client.Handler)

        With Me.lvServerClients
            .GridLines		= True
            .MultiSelect	= False
			.View			= View.Details
            .Columns.Add("Client Unique ID", .ClientRectangle.Width \ 4)
            .Columns.Add("Client End Point", .ClientRectangle.Width \ 4)
            .Columns.Add("Uptime (latency)", .ClientRectangle.Width \ 2)
        End With

    End Sub
    Private Sub frmMain_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If (Me.Server.Active) Then
            Me.btnStop.PerformClick()
        End If
    End Sub
    Private Sub btnStart_Click(sender As Object, e As EventArgs) Handles btnStart.Click
        Me.Server.Start(41200)
    End Sub
    Private Sub btnStop_Click(sender As Object, e As EventArgs) Handles btnStop.Click
        Me.Server.Shutdown().WaitOne()
    End Sub
	Private Sub btnAddClient_Click( sender As Object,  e As EventArgs) Handles btnAddClient.Click
		Dim client As New TCP.Client.Handler
		AddHandler client.Event_Status,AddressOf Me.Event_Status
		client.Connect("192.168.178.22", 41200)
		Me.Clients.Add(client)

	End Sub
    Private Sub Refresher_Tick(sender As Object, e As EventArgs) Handles Refresher.Tick
        Me.ListViewUpdate()
    End Sub
    Private Sub UpdateGUI(bool As Boolean)
        If (Not Me.IsDisposed AndAlso Me.InvokeRequired) Then
            Me.Invoke(Sub() Me.UpdateGUI(bool))
        Else
            If (bool) Then
                Me.btnStart.Enabled		= False
                Me.btnStop.Enabled		= True
				Me.btnAddClient.Enabled = True
                Me.Refresher.Start()
            Else
                Me.btnStart.Enabled		= True
                Me.btnStop.Enabled		= False
				Me.btnAddClient.Enabled = False
                Me.Refresher.Stop()
            End If
        End If
    End Sub
    Public Sub LogMessage(Message As String)
	    If (Not Me.Log.IsDisposed AndAlso Me.Log.InvokeRequired) Then
            Me.Log.Invoke(Sub() Me.LogMessage(Message))
        Else
            With Me.Log
                .AppendText(String.Format("{0}{1}", Message, ControlChars.CrLf))
                .SelectionStart = .Text.Length - 1
                .ScrollToCaret()
            End With
        End If
    End Sub
    Public Sub ListViewUpdate()
        If (Me.lvServerClients.InvokeRequired) Then
            Me.lvServerClients.Invoke(Sub() Me.ListViewUpdate())
        Else
            If (Me.Server.Clients.Any) Then
                For Each Handler As TCP.Server.Handler In Me.Server.Clients
                    Dim item As ListViewItem = Nothing
                    If (Me.lvServerClients.Items.ContainsKey(Handler.UniqueClientID.HexStr)) Then
                        item = Me.lvServerClients.Items(Handler.UniqueClientID.HexStr)
                        item.SubItems(2).Text =String.Format("{0} secs ({1})",(DateTime.Now - Handler.FirstSeen).Seconds.ToString,
																			  (DateTime.Now - Handler.LastResponse).ToString)
                    Else
                        item = Me.lvServerClients.Items.Add(Handler.UniqueClientID.HexStr, Handler.UniqueClientID.HexStr, -1)
                        item.SubItems.Add(Handler.EndPoint.ToString)
                        item.SubItems.Add( (DateTime.Now - Handler.FirstSeen).Seconds.ToString)
                    End If
                Next
            Else
                Me.lvServerClients.Items.Clear()
            End If
        End If
    End Sub
    Private Sub Event_Status(EventType As TcpEvent, TcpEventArgs As TcpEventArgs)

        If (TcpEventArgs.IsFromServer) Then
			Dim args As TcpServerEvent = CType(TcpEventArgs,TcpServerEvent)

            Select Case EventType

                Case TcpEvent.LISTENING
                    Me.LogMessage(String.Format("[server] Listening...{0}", args.Listener.EndPoint.ToString))
                    Me.UpdateGUI(True)
                   
                Case TcpEvent.NEWCONNECTION
                    Me.LogMessage("[server] New connection, awaiting handshake...")

                Case TcpEvent.HANDSHAKE
                    Me.LogMessage("[server] Client handshake accepted")
					args.Listener.SendFile(args.Client,".\logo.png","Image")

                Case TcpEvent.RECEIVED
                    Me.LogMessage(String.Format("[server] Received {0}", args.Packet.ContentType))

					If (args.HasPacket) then
						Select Case args.Packet.ContentType

							Case ContentType.SerializedData
								Me.LogMessage(String.Format("[server] Received data: {0}", args.Packet.PayloadGet.ToString))

							Case ContentType.File
								args.GetAttachement.SaveAs(String.Format(".\server_received_{0}",args.GetAttachement.Filename))

						End Select
					End If

                Case TcpEvent.ERR
                    Me.LogMessage(String.Format("[server] Error {0}", args.Exception.Message))

                Case TcpEvent.INACTIVE
                    Me.LogMessage(String.Format("[server] Closed"))
                    Me.UpdateGUI(False)
                    Me.ListViewUpdate()

            End Select
        End If

        If (TcpEventArgs.IsFromClient) Then
			
            Dim args As TcpClientEvent =  CType(TcpEventArgs,TcpClientEvent)
						
            Select Case EventType

                Case TCP.Protocol.TcpEvent.CONNECTING
                    Me.LogMessage(String.Format("[client] Connecting...{0}", args.Client.EndPoint.ToString))

                Case TCP.Protocol.TcpEvent.CONNECTED
                    Me.LogMessage(String.Format("[client] Connected {0}", args.Client.EndPoint.ToString))

                Case TCP.Protocol.TcpEvent.ACCEPTED
				    Me.LogMessage("[client] Connection accepted")
					args.Client.Send(ContentType.SerializedData, "Hello, World!")
                    
                Case TCP.Protocol.TcpEvent.RECEIVED
                    Me.LogMessage(String.Format("[client] Received {0}", args.Packet.ContentType))

					If (args.HasPacket) then
						Select Case args.Packet.ContentType

							Case ContentType.SerializedData
								Me.LogMessage(String.Format("[client] Received data: {0}", args.Packet.PayloadGet.ToString))

							Case ContentType.File
								args.GetAttachement.SaveAs(String.Format(".\client_received_{0}",args.GetAttachement.Filename))
								

						End Select
					End If

                Case TCP.Protocol.TcpEvent.ERR
                    Me.LogMessage(String.Format("[client] Error {0}", args.Exception.Message))

                Case TCP.Protocol.TcpEvent.DISCONNECTED
                    Me.LogMessage(String.Format("[client] Disconnected"))

                Case TCP.Protocol.TcpEvent.INACTIVE
                    Me.LogMessage(String.Format("[client] Closed"))

            End Select
        End If
    End Sub
End Class
