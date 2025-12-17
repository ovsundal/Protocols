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
        "Hello World" //body
    );

    static void Main(string[] args)
    {
        var (startLine, headers, body) = HttpParser.ParseHttpMessage(GetHttpResponseInByteArray);

        Console.WriteLine(startLine);
        Console.WriteLine(headers);
        Console.WriteLine(body);
    }
}