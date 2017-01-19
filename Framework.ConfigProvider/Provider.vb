Imports System.IO
Imports System.Threading
Public NotInheritable Class Provider
	Inherits Dictionary(Of String, Value)
	Implements IDisposable
	Public Event ProviderModifiedValues()
	Sub New(Filename As String)
		Me.Filename = Filename
		Me.ResetEvent = New ManualResetEvent(False)
		Me.CheckSum = Me.ComputeChecksum(File.ReadAllBytes(Filename))
		Threads.BackgroundWorker.Run(Sub() Me.Idler())
	End Sub
	Public Function TryGet(Of T)(key As String) As T
		If (Me.ContainsKey(key) AndAlso TypeOf Me(key).Value Is T) Then
			Return Me(key).Cast(Of T)()
		Else
			Return Nothing
		End If
	End Function
	Public Sub AddRange(Of TKey, TValue)(src As Dictionary(Of TKey, TValue),ByRef dest As Dictionary(Of TKey, TValue),Optional Clear As boolean = False)
		If (Clear) then dest.Clear
		For Each item As KeyValuePair(Of TKey,TValue) In src
			dest.Add(item.Key,item.Value)
		Next
	End Sub
	Private Sub Idler()
		Try
			If (Me.Active) Then
				Me.Active = False
				Me.ResetEvent.WaitOne()
			End If
			Me.Active = True
			Me.ResetEvent.Reset()
			Do
				Threads.Delayed.Run(New Threads.Delayed.TaskDelegate(AddressOf Me.Checkup), 500).WaitOne()
			Loop While Me.Active
		Finally
			Me.ResetEvent.Set()
		End Try
	End Sub
	Private Sub Checkup(ByRef Finished As ManualResetEvent)
		Try
			If (Not Me.ComputeChecksum(File.ReadAllBytes(Me.Filename)).SequenceEqual(Me.CheckSum)) then
				Me.CheckSum = Me.ComputeChecksum(File.ReadAllBytes(Filename))
				Me.AddRange(Of String,value)(New Parser(Me.Filename).Create,me,True)
				RaiseEvent ProviderModifiedValues()
			End If
		Finally
			Finished.Set
		End Try
	End Sub
	Private Function ComputeChecksum(buffer() As Byte) As Byte()
		Using Cryptograph As Security.Cryptography.SHA256 = Security.Cryptography.SHA256.Create()
			Return Cryptograph.ComputeHash(buffer)
		End Using
	End Function
	Private Property Active As Boolean
	Private Property Filename As String
	Private Property CheckSum As Byte()
	Private Property ResetEvent As ManualResetEvent
#Region "IDisposable Support"
	Private disposedValue As Boolean
	Protected Sub Dispose(disposing As Boolean)
		If Not Me.disposedValue Then
			If disposing Then
				If (Me.Active) Then
					Me.Active = False
					Me.ResetEvent.WaitOne()
				End If
				Me.Clear()
			End If
		End If
		Me.disposedValue = True
	End Sub
	Public Sub Dispose() Implements IDisposable.Dispose
		Dispose(True)
		GC.SuppressFinalize(Me)
	End Sub
#End Region
End Class
