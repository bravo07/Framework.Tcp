Imports System.IO
Imports System.Reflection
Imports System.Net.Sockets
Imports System.IO.Compression
Public NotInheritable Class Config
    ''' <summary>
    ''' Global settings for client and server
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared VERSION_MAJOR As Byte						= &H1
    Public Shared VERSION_PATCH As Byte						= &H0
    Public Shared CYCLE_TIME As Int16						= &HFA
    Public Shared BUFFER_SIZE As Int16						= &H1000
	Public Shared PORT_SAFERANGE As Int16					= &H400

	''' <summary>
    ''' Server settings
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared RESPONSE_MAXTIME As Int16					= &H1E

    ''' <summary>
    ''' Client settings
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared CLIENT_CONTIMEOUT As Int16				= &H1388

    ''' <summary>
    ''' Packet settings
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared PACKET_COMPRESS As Boolean				= True
    Public Shared PACKET_MAXSIZE As Int32					= &H500000
    Public Shared PACKET_COMPRESSLEVEL As CompressionLevel	= CompressionLevel.Fastest
    Public Shared PACKET_CHECKSUMTYPE As HashType			= HashType.SHA256

    ''' <summary>
    ''' Socket settings
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared SOCKET_VALUE As Boolean = True
    Public Shared SOCKET_LEVEL As SocketOptionLevel			= SocketOptionLevel.Socket
    Public Shared SOCKET_OPT As SocketOptionName			= SocketOptionName.DontLinger
    Public Shared NAT_TRAVERSAL As IPProtectionLevel		= IPProtectionLevel.Unrestricted

    ''' <summary>
    ''' Packet stream terminator sequence
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared ENDOFPACKET As Byte()						= New Byte() {&H0, &H0, &H45, &H4F, &H50}

    ''' <summary>
    ''' Global error messages
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared ERR_BADPORT As String						= "Avoid using ports below the safe range"
    Public Shared ERROR_REQTIMEOUT As String				= "Failed to connect, request timed out"
    Public Shared ERROR_BADPACKET As String					= "Packet dropped, wrong checksum or missing data"
    Public Shared ERROR_CLIENTLOST As String				= "Client dropped, lost connection"
    Public Shared ERROR_CLIENTTIMEOUT As String				= "Client dropped, response time exceeded"
    Public Shared ERROR_BADHANDSHAKE As String				= "Client dropped, handshake version mismatch"
	Public Shared ERROR_MAXREICEIVE As String				= "Connection dropped, buffer limit exceeded"
    Public Shared ERROR_CONTENTTYPEWARNING As String		= "The selected content type '{0}' is not intended for data transfers"
End Class
