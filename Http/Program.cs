using System.Text;

namespace HttpServer;

class Program
{
    static void Main(string[] args)
    {
        // sending a http request
        var httpMessage = ApplicationLayerService.CreateHttpPostRequestMessage();
        var tcpPacket = TransportLayerService.AddTcpHeader(httpMessage);
        

        Console.WriteLine("test");
    }
}