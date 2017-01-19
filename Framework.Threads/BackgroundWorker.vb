Imports System.Threading
Public Class BackgroundWorker
    Public Shared Function Run(TaskToExecute As Action) As Action
        Dim thr As New Thread(New ThreadStart(Sub() TaskToExecute())) With {.IsBackground = True}
        thr.Start()
        Return TaskToExecute
    End Function
End Class
