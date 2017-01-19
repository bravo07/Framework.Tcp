<Serializable>
Public Class Version
    Sub New(Major As Byte, Patch As Byte)
        Me.Major = Major
    End Sub
    Public Shared Function Current() As Version
        Return New Protocol.Version(Protocol.Config.VERSION_MAJOR, Protocol.Config.VERSION_PATCH)
    End Function
    Private m_major As Byte
    Public Property Major As Byte
        Get
            Return Me.m_major
        End Get
        Set(value As Byte)
            Me.m_major = value
        End Set
    End Property
    Private m_patch As Byte
    Public Property Patch As Byte
        Get
            Return Me.m_patch
        End Get
        Set(value As Byte)
            Me.m_patch = value
        End Set
    End Property
    Public Shared Operator <>(a As Version, b As Version) As Boolean
        Return a.Major <> b.Major AndAlso a.Patch <> b.Patch
    End Operator
    Public Shared Operator =(a As Version, b As Version) As Boolean
        Return a.Major = b.Major AndAlso a.Patch = b.Patch
    End Operator
    Public Shared Operator >(a As Version, b As Version) As Boolean
        Return a.Major >= b.Major AndAlso a.Patch > b.Patch
    End Operator
    Public Shared Operator <(a As Version, b As Version) As Boolean
        Return a.Major <= b.Major AndAlso a.Patch < b.Patch
    End Operator
    Public Shared Operator <=(a As Version, b As Version) As Boolean
        Return a.Major <= b.Major AndAlso a.Patch <= b.Patch
    End Operator
    Public Shared Operator >=(a As Version, b As Version) As Boolean
        Return a.Major >= b.Major AndAlso a.Patch >= b.Patch
    End Operator
    Public Overrides Function ToString() As String
        Return String.Format("{0}.{1}", Me.Major, Me.Patch)
    End Function
End Class
