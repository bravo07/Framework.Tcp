Imports System.Threading
Public Class Delayed
    Public Delegate Sub TaskDelegate(ByRef done As ManualResetEvent)
    Public Shared Function Run(TaskToExecute As TaskDelegate, DelayInMilliseconds As Double) As ManualResetEvent
        Dim done As New ManualResetEvent(False)
        Using Timer As New Timers.Timer(DelayInMilliseconds) With {.AutoReset = False}
            Try
                AddHandler Timer.Elapsed, Sub(obj, args) TaskToExecute.Invoke(done)
                Timer.Start()
                Return done
            Finally
                done.WaitOne()
                Timer.Stop()
				Timer.Dispose()
            End Try
        End Using
    End Function
End Class
