Imports System.Globalization
Public NotInheritable Class Value
	Sub New(Value As String, Type As ValueType)
		Try
			Me.Type = Type
			Select Case Type
				Case ConfigProvider.ValueType.Byte
					Dim v As Byte = Nothing
					If (Byte.TryParse(Value, v)) Then
						Me.Value = v
						Return
					End If
				Case ConfigProvider.ValueType.Int16
					Dim v As Int16 = Nothing
					If (Int16.TryParse(Value, v)) Then
						Me.Value = v
						Return
					End If
				Case ConfigProvider.ValueType.Int32
					Dim v As Int32 = Nothing
					If (Int32.TryParse(Value, v)) Then
						Me.Value = v
						Return
					End If
				Case ConfigProvider.ValueType.Int64
					Dim v As Int64 = Nothing
					If (Int64.TryParse(Value, v)) Then
						Me.Value = v
						Return
					End If
				Case ConfigProvider.ValueType.Boolean
					Dim v As Boolean = Nothing
					If (Boolean.TryParse(Value, v)) Then
						Me.Value = v
						Return
					End If
				Case ValueType.Decimal
					Dim v As Double = Nothing
					If (Double.TryParse(Value,NumberStyles.Float, New CultureInfo("en-US") ,v)) Then
						Me.Value = v
						Return
					End If
				Case ConfigProvider.ValueType.String
					Me.Value = Convert.ToString(Value)
			End Select
			Me.Value = Value
			Me.Type = ValueType.Undefined
		Catch ex As Exception
			Me.Value = Value
			Me.Exception = ex
			Me.Type = ValueType.Undefined
		End Try
	End Sub
	Public Function [TryCast](Of T)() As T
		If (TypeOf Me.Value Is T) Then Return CType(Me.Value, T)
		Return Nothing
	End Function
	Public Function Cast(Of T)() As T
		Return CType(Me.Value, T)
	End Function
	Public Function ToByte() As Byte
		Return CType(Me.Value, Byte)
	End Function
	Public Function ToInt16() As Int16
		Return CType(Me.Value, Int16)
	End Function
	Public Function ToInt32() As Int32
		Return CType(Me.Value, Int32)
	End Function
	Public Function ToInt64() As Int64
		Return CType(Me.Value, Int64)
	End Function
	Public Function ToBoolean() As Boolean
		Return CType(Me.Value, Boolean)
	End Function
	Public Function ToDouble() As Double
		Return CType(Me.Value, Double)
	End Function
	Public Overrides Function ToString() As String
		Return CType(Me.Value, String)
	End Function
	Public Property Value As Object
	Public Property Type As ValueType
	Public Property Exception As Exception
End Class
