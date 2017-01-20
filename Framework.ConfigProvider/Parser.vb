Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Public NotInheritable Class Parser
	Public Const LINE_PATTERN As String = "^([a-z0-9_.]*).*?\:.*?(\bbyte\b|\bint16\b|\bint32\b|\bint64\b|\bstring\b|\bboolean\b|\bfloat\b).*?\:.*?(\bfalse\b|\btrue\b|[-+]?\d*\.\d+([eE][-+]?\d+)?|[-+]?\d*([eE]?\d+)|'.*?'|\""(.*?)\"")\;"
	Sub New(Filename As String)
		Me.Filename = Filename
	End Sub
	Public Function Create() As Provider
		If (File.Exists(Me.Filename)) Then
			Using fs As New FileStream(Me.Filename, FileMode.Open, FileAccess.Read)
				Dim buffer() As Byte = New Byte(Convert.ToInt32(fs.Length - 1)) {}
				fs.Read(buffer, 0, buffer.Length)
				Dim userconfig As New Provider(Me.Filename)
				Dim rx As New Regex(Parser.LINE_PATTERN, RegexOptions.IgnoreCase Or RegexOptions.Multiline)
				Dim collection As MatchCollection = rx.Matches(Encoding.UTF8.GetString(buffer))
				For Each item As Match In collection
					If (item.Groups(2).Value.Equals("byte", StringComparison.OrdinalIgnoreCase)) Then
						userconfig.Add(item.Groups(1).Value, New Value(item.Groups(3).Value, ValueType.Byte))
					ElseIf (item.Groups(2).Value.Equals("int16", StringComparison.OrdinalIgnoreCase)) Then
						userconfig.Add(item.Groups(1).Value, New Value(item.Groups(3).Value, ValueType.Int16))
					ElseIf (item.Groups(2).Value.Equals("int32", StringComparison.OrdinalIgnoreCase)) Then
						userconfig.Add(item.Groups(1).Value, New Value(item.Groups(3).Value, ValueType.Int32))
					ElseIf (item.Groups(2).Value.Equals("int64", StringComparison.OrdinalIgnoreCase)) Then
						userconfig.Add(item.Groups(1).Value, New Value(item.Groups(3).Value, ValueType.Int64))
					ElseIf (item.Groups(2).Value.Equals("float", StringComparison.OrdinalIgnoreCase)) Then
						userconfig.Add(item.Groups(1).Value, New Value(item.Groups(3).Value, ValueType.Decimal))
					ElseIf (item.Groups(2).Value.Equals("string", StringComparison.OrdinalIgnoreCase)) Then
						userconfig.Add(item.Groups(1).Value, New Value(item.Groups(3).Value, ValueType.String))
					ElseIf (item.Groups(2).Value.Equals("boolean", StringComparison.OrdinalIgnoreCase)) Then
						userconfig.Add(item.Groups(1).Value, New Value(item.Groups(3).Value, ValueType.Boolean))
					Else
						userconfig.Add(item.Groups(1).Value, New Value(item.Groups(3).Value, ValueType.Undefined))
					End If
				Next
				Return userconfig
			End Using
		End If
		Throw New Exception(String.Format("File '{0}' does not exist", Filename))
	End Function
	Public Property Filename As String
End Class
