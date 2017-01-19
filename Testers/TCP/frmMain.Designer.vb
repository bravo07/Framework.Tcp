<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
		Me.components = New System.ComponentModel.Container()
		Me.Log = New System.Windows.Forms.TextBox()
		Me.btnStart = New System.Windows.Forms.Button()
		Me.btnStop = New System.Windows.Forms.Button()
		Me.Refresher = New System.Windows.Forms.Timer(Me.components)
		Me.btnAddClient = New System.Windows.Forms.Button()
		Me.lvServerClients = New TCP_Tester.ListviewEx()
		Me.SuspendLayout
		'
		'Log
		'
		Me.Log.BackColor = System.Drawing.Color.White
		Me.Log.Font = New System.Drawing.Font("Consolas", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.Log.Location = New System.Drawing.Point(12, 243)
		Me.Log.Multiline = true
		Me.Log.Name = "Log"
		Me.Log.ReadOnly = true
		Me.Log.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
		Me.Log.Size = New System.Drawing.Size(596, 156)
		Me.Log.TabIndex = 0
		'
		'btnStart
		'
		Me.btnStart.Location = New System.Drawing.Point(12, 12)
		Me.btnStart.Name = "btnStart"
		Me.btnStart.Size = New System.Drawing.Size(120, 27)
		Me.btnStart.TabIndex = 2
		Me.btnStart.Text = "Start"
		Me.btnStart.UseVisualStyleBackColor = true
		'
		'btnStop
		'
		Me.btnStop.Enabled = false
		Me.btnStop.Location = New System.Drawing.Point(264, 12)
		Me.btnStop.Name = "btnStop"
		Me.btnStop.Size = New System.Drawing.Size(121, 27)
		Me.btnStop.TabIndex = 3
		Me.btnStop.Text = "Stop"
		Me.btnStop.UseVisualStyleBackColor = true
		'
		'Refresher
		'
		Me.Refresher.Interval = 1000
		'
		'btnAddClient
		'
		Me.btnAddClient.Enabled = false
		Me.btnAddClient.Location = New System.Drawing.Point(138, 12)
		Me.btnAddClient.Name = "btnAddClient"
		Me.btnAddClient.Size = New System.Drawing.Size(120, 27)
		Me.btnAddClient.TabIndex = 5
		Me.btnAddClient.Text = "Add Client"
		Me.btnAddClient.UseVisualStyleBackColor = true
		'
		'lvServerClients
		'
		Me.lvServerClients.Font = New System.Drawing.Font("Consolas", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
		Me.lvServerClients.Location = New System.Drawing.Point(12, 45)
		Me.lvServerClients.Name = "lvServerClients"
		Me.lvServerClients.Size = New System.Drawing.Size(596, 192)
		Me.lvServerClients.TabIndex = 4
		Me.lvServerClients.UseCompatibleStateImageBehavior = false
		'
		'frmMain
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6!, 13!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(620, 411)
		Me.Controls.Add(Me.btnAddClient)
		Me.Controls.Add(Me.lvServerClients)
		Me.Controls.Add(Me.btnStop)
		Me.Controls.Add(Me.btnStart)
		Me.Controls.Add(Me.Log)
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
		Me.MaximizeBox = false
		Me.MinimizeBox = false
		Me.Name = "frmMain"
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
		Me.Text = "TCP Framework Tester"
		Me.ResumeLayout(false)
		Me.PerformLayout

End Sub
    Friend WithEvents Log As System.Windows.Forms.TextBox
    Friend WithEvents btnStart As System.Windows.Forms.Button
    Friend WithEvents btnStop As System.Windows.Forms.Button
    Friend WithEvents lvServerClients As TCP_Tester.ListviewEx
    Friend WithEvents Refresher As System.Windows.Forms.Timer
    Friend WithEvents btnAddClient As System.Windows.Forms.Button

End Class
