''' <summary>
''' Definitions of packet types
''' </summary>
''' <remarks></remarks>
Public Enum ContentType As Byte
    Synchronize		= 2
    Acknowledge		= 4
    Handshake		= 6
    SerializedData	= 8
    File			= 10
End Enum

''' <summary>
''' Checksum defined types
''' </summary>
''' <remarks></remarks>
Public Enum HashType As Byte
    SHA256			= 2
    SHA384			= 4
    SHA512			= 6
End Enum

''' <summary>
''' Definitions of buffer types
''' </summary>
''' <remarks></remarks>
Public Enum Buffer As Byte
    Reader			= 2
    Writer			= 4
    Both			= Reader Or Writer
End Enum

''' <summary>
''' Tcp event types
''' </summary>
''' <remarks></remarks>
Public Enum TcpEvent As Byte
    ERR				= 0
    INACTIVE		= 1
    CONNECTING		= 2
    CONNECTED		= 4
    LISTENING		= 6
    RECEIVED		= 8
    SEND			= 10
    ACCEPTED		= 14
    HANDSHAKE		= 16
    NEWCONNECTION	= 18
    DISCONNECTED	= 20
End Enum