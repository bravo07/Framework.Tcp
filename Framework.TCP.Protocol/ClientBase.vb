Imports System.Net
Imports System.Net.Sockets
Public MustInherit Class ClientBase
    Sub New()
        Me.m_buffersize = Config.BUFFER_SIZE
		Me.m_uncid = Me.NewGuid(8)
        Me.ReadBuffer = New Byte() {}
        Me.WriteBuffer = New Byte(Me.BufferSize - 1) {}
    End Sub
#Region "Routines"
    Public Sub Reallocate(bType As Buffer)
        If (bType = Buffer.Reader) Then
            Me.ReadBuffer = New Byte() {}
        ElseIf (bType = Buffer.Writer) Then
            Me.WriteBuffer = New Byte(Me.BufferSize - 1) {}
        ElseIf (bType = Buffer.Both) Then
            Me.ReadBuffer = New Byte() {}
            Me.WriteBuffer = New Byte(Me.BufferSize - 1) {}
        End If
    End Sub
    Public Sub CopyReceivedData(Handler As Socket, ar As IAsyncResult)
        Dim len As Integer = Handler.EndReceive(ar)
        Dim offset As Integer = Me.ReadBuffer.Length
        Array.Resize(Of Byte)(Me.WriteBuffer, len)
        Array.Resize(Of Byte)(Me.ReadBuffer, Me.ReadBuffer.Length + Me.WriteBuffer.Length)
        Me.WriteBuffer.CopyTo(Me.ReadBuffer, offset)
    End Sub
	Public Function NewGuid(len As Integer) as byte()
		Dim buffer() As Byte = New Byte(len) {}
		Return buffer.Randomize
	End Function
#End Region
#Region "Properties"
    Private m_readbuffer As Byte()
    Public Property ReadBuffer As Byte()
        Get
            Return Me.m_readbuffer
        End Get
        Set(value As Byte())
            Me.m_readbuffer = value
        End Set
    End Property
    Private m_writebuffer As Byte()
    Public Property WriteBuffer As Byte()
        Get
            Return Me.m_writebuffer
        End Get
        Set(value As Byte())
            Me.m_writebuffer = value
        End Set
    End Property
    Private m_buffersize As Integer
    Public ReadOnly Property BufferSize As Integer
        Get
            Return Me.m_buffersize
        End Get
    End Property
	Private m_uncid As byte()
    Public ReadOnly Property UniqueClientID As byte()
        Get
           Return Me.m_uncid 
        End Get
    End Property
#End Region
End Class
