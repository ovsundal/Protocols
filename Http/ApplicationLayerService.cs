using System.Text;

namespace HttpServer;

// based on HTTP/1.1 specifications https://www.rfc-editor.org/rfc/rfc9112.html
public static class ApplicationLayerService
{
    private const string GetHttpRequest =
        "GET / HTTP/1.1\r\n" + // request status line
        "Host: localhost\r\n" + // headers
        "User-Agent: CustomClient/1.0\r\n" +
        "Accept: */*\r\n" +
        "Connection: close\r\n"; // end headers, no body


    private const string GetHttpResponse =
        "HTTP/1.1 200 OK\r\n" + // response status line
        "Content-Type: text/plain; charset=utf-8\r\n" + // headers
        "Content-Length: 11\r\n" +
        "Connection: close\r\n" +
        "\r\n" + // end headers
        "Hello World"; // body

    private const string PostHttpRequest =
        "POST /orders HTTP/1.1\r\n" + // request-line: method, path, version
        "Host: example.com\r\n" + // required in HTTP/1.1
        "Content-Type: text/plain; charset=utf-8\r\n" + // body is JSON
        "Content-Length: 11\r\n" + // length of the JSON body in bytes
        "Connection: close\r\n" + // do not keep the TCP connection alive
        "\r\n" + // end of headers
        "Hello World"; // body (27 bytes with this exact spacing)

    public static byte[] CreateHttpPostRequestMessage()
    {
        return Encoding.ASCII.GetBytes(PostHttpRequest);
    }

    public static byte[] CreateHttpGetRequestMessage()
    {
        return Encoding.ASCII.GetBytes(GetHttpRequest);
    }
    
    public static byte[] CreateHttpGetResponseMessage()
    {
        return Encoding.ASCII.GetBytes(GetHttpResponse);
    }
    
    public static (string, Dictionary<string, string>, string) ParseHttpMessage(byte[] httpMessage)
    {
        // a http server only accepts requests, so it should throw error if it receives a http response
        var (startLine, indexAfterStartLine) = ParseStartLine(httpMessage);
        var (headers, indexAfterHeaders) = ParseHeaders(httpMessage, indexAfterStartLine);

        // body length is defined by content-length
        headers.TryGetValue("Content-Length", out var contentLengthString);
        int.TryParse(contentLengthString, out var contentLength);
        var body = ParseBody(httpMessage, indexAfterHeaders, contentLength);

        return (startLine, headers, body);
    }

    private static string ParseBody(byte[] httpMessage, int indexAfterHeaders, int contentLength = 0)
    {
        var bodyText = Encoding.ASCII.GetString(httpMessage, indexAfterHeaders, contentLength);

        return bodyText;
    }

    /**
    * Finds the headers of the HTTP message (everything between the first \r\n (CRLF) and an empty \r\n line)
     * Header type is before the colon (:), header value is everything after the colon (:) before the \r\n
    */
    private static (Dictionary<string, string> headers, int nextIndex) ParseHeaders(byte[] bytes, int startIndex)
    {
        var headers = new Dictionary<string, string>();
        var i = startIndex;

        while (i < bytes.Length - 1)
        {
            // End of headers? (denoted by an empty line CRLF)
            if (bytes[i] == 0x0D && bytes[i + 1] == 0x0A)
            {
                i += 2; // skip the CRLF that ends the header section
                return (headers, i);
            }

            // Find end of this header line (CRLF)
            var lineStart = i;
            var lineEnd = -1;
            for (; i < bytes.Length - 1; i++)
            {
                if (bytes[i] == 0x0D && bytes[i + 1] == 0x0A)
                {
                    lineEnd = i;
                    break;
                }
            }

            // if line is not terminated by CRLF, throw exception
            if (lineEnd == -1)
            {
                throw new InvalidOperationException("Invalid HTTP message: header line not terminated by CRLF.");
            }

            // Decode this header line
            var line = Encoding.ASCII.GetString(bytes, lineStart, lineEnd - lineStart);

            // Split on first colon as per field-line = field-name ":" OWS field-value OWS
            var colonIndex = line.IndexOf(':');
            if (colonIndex <= 0)
            {
                throw new InvalidOperationException("Invalid HTTP header: missing ':' in field-line.");
            }

            var name = line[..colonIndex]; // field-name
            var value = line[(colonIndex + 1)..]; // after ':'
            value = value
                .Trim(); // remove OWS around field-value, RFC says OWS is excluded by parsers[web:64][attached_file:1]

            // add header to dictionary
            headers[name] = value;

            i = lineEnd + 2; // move past CRLF for this header line
        }

        return (headers, i);
    }

    /**
     * Finds the start line of the HTTP message (everything before the first \r\n (CRLF))
     * Start line can be both a http request (request-line) or a http response (status-line).
     */
    private static (string, int nextIndex) ParseStartLine(byte[] bytes)
    {
        // Look for CRLF = 0x0D 0x0A
        for (var i = 0; i < bytes.Length - 1; i++)
        {
            if (bytes[i] == 0x0D && bytes[i + 1] == 0x0A)
            {
                // Decode only the start-line bytes [0, i)
                var startLine = Encoding.ASCII.GetString(bytes, 0, i);
                var indexAfterStartLineCrlf = i + 2; // skip the CRLF

                return (startLine, indexAfterStartLineCrlf);
            }
        }

        throw new InvalidOperationException("Invalid HTTP message: No CRLF found.");
    }
}