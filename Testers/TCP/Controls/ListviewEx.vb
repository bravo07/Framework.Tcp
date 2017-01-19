Public Class ListviewEx
    Inherits System.Windows.Forms.ListView
    Public Sub New()
        Me.SetStyle(ControlStyles.OptimizedDoubleBuffer Or ControlStyles.AllPaintingInWmPaint, True)
        Me.SetStyle(ControlStyles.EnableNotifyMessage, True)
    End Sub
    Protected Overrides Sub OnNotifyMessage(m As Message)
        If m.Msg <> &H14 Then MyBase.OnNotifyMessage(m)
    End Sub
End Class
