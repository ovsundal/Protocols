using System.Buffers.Binary;

namespace HttpServer;

public static class TransportLayerService
{
    private const ushort SourcePort = 50000; // arbitrary ephemeral port
    private const ushort HttpPort   = 80;    // HTTP
    
    /**
     * Adds the standard 20-byte TCP header to the payload.
     */
    public static byte[] AddTcpHeader(byte[] payload)
    {
        var header = new byte[20];
        
        // Row 1: Source port (who is sending this?) | Destination port (Who should receive this?)
        BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(0, 2), SourcePort);
        BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(2, 2), HttpPort);
        
        // Row 2: A 32-bit counter for the first byte of data in this segment
        // Let receiver reassemble bytes in order if out of order and detect missing data
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(4, 4), 1u);
        
        // Row 3: Also a 32-bit counter from the receiver's point of view. 
        // Says "I have successfully received all bytes up to (ack-1); please send me the next one."
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(8, 4), 0u);
        
        // Row 4: DO | RSV | Flags | Window
        // Data Offset (DO) (how long the header is), so it tells where the payload (http message) starts
        header[12] = 0x50;

        // Flags, individual bits like SYN, ACK, FIN, PSH, RST, URG (describes what kind of segment this is)
        header[13] = 0x18;

        // Window (how many more bytes can you send me right now)?
        BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(14, 2), 65535);
        
        // Row 5: Checksum (integrity check, detects corruption in transit) |
        // Urgent pointer (used with the URG flag to mark that some bytes are "urgent" and should be delivered ASAP)
        BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(16, 2), 0);
        BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(18, 2), 0);
        
        // Concatenate header + payload
        var segment = new byte[header.Length + payload.Length];
        Buffer.BlockCopy(header, 0, segment, 0, header.Length);
        Buffer.BlockCopy(payload, 0, segment, header.Length, payload.Length);
        
        return segment;
    }
}