Imports System.IO
<Serializable>
Public Class Attachment
    Sub New(Filename As String, AttachmentID As Object)
		If (AttachmentID.GetType.IsSerializable) then
			Me.id = AttachmentID
		End If
        Me.Filename = New FileInfo(Filename).Name
        If (File.Exists(Filename)) Then
            Using fs As New FileStream(Filename, FileMode.Open, FileAccess.Read)
                Me.Data = New Byte(Convert.ToInt32(fs.Length) - 1) {}
                fs.Read(Me.Data, 0, Me.Data.Length)
            End Using
        Else
            Me.Data = New Byte() {}
        End If
    End Sub
    Public Sub SaveAs(Filename As String, Optional Overwrite As Boolean = False)
        If (Me.Data IsNot Nothing) Then
            If (File.Exists(Filename) AndAlso Overwrite) Then File.Delete(Filename)
            Using bw As New BinaryWriter(File.Open(Filename, FileMode.OpenOrCreate))
                bw.Write(Me.Data)
            End Using
        End If
    End Sub
    Public Shared Function [TryCast](Obj As Object, ByRef Result As Attachment) As Boolean
        If (TypeOf Obj Is Attachment) Then
            Result = CType(Obj, Attachment)
            Return True
        End If
        Return False
    End Function
    Public Overrides Function ToString() As String
        Return String.Format("{0} ({1} bytes) [ID: {2}]", Me.Filename, Me.Data.Length.ToString, If(Me.ID IsNot Nothing, Me.ID.ToString, String.Empty))
    End Function
    Public Property Data As Byte()
    Public Property Filename As String
    Public Property ID As Object
End Class 