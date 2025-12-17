using System.Text;

namespace HttpServer;

class Program
{
    private static readonly byte[] GetHttpRequestInByteArray = Encoding.ASCII.GetBytes(
        "GET / HTTP/1.1\r\n" + // request status line
        "Host: localhost\r\n" + // headers
        "User-Agent: CustomClient/1.0\r\n" +
        "Accept: */*\r\n" +
        "Connection: close\r\n" +
        "\r\n" // end headers
    );

    private static readonly byte[] GetHttpResponseInByteArray = Encoding.ASCII.GetBytes(
        "HTTP/1.1 200 OK\r\n" + // response status line
        "Content-Type: text/plain; charset=utf-8\r\n" + // headers
        "Content-Length: 11\r\n" +
        "Connection: close\r\n" +
        "\r\n" + // end headers
        "Hello World" + //body
        "\r\n"
    );
    
    private static readonly byte[] PostHttpRequestInByteArray = Encoding.ASCII.GetBytes(
        "POST /orders HTTP/1.1\r\n" +                 // request-line: method, path, version
        "Host: example.com\r\n" +                     // required in HTTP/1.1
        "Content-Type: text/plain; charset=utf-8\r\n" + // body is JSON
        "Content-Length: 11\r\n" +                    // length of the JSON body in bytes
        "Connection: close\r\n" +                     // do not keep the TCP connection alive
        "\r\n" +                                      // end of headers
        "Hello World\r\n"              // body (27 bytes with this exact spacing)
    );


    static void Main(string[] args)
    {
        var (startLine, headers, body) = HttpParser.ParseHttpMessage(PostHttpRequestInByteArray);

        Console.WriteLine("test");
    }
}