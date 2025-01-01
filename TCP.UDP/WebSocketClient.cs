using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class WebSocketClient : IDisposable
{
    private ClientWebSocket webSocket;

    public bool IsConnected => webSocket.State == WebSocketState.Open;

    public WebSocketClient()
    {
        webSocket = new ClientWebSocket();
    }

    public async Task Connect(string serverUri)
    {
        await webSocket.ConnectAsync(new Uri(serverUri), CancellationToken.None);
    }

    public async Task SendRequestAndReceiveResponse(byte[] requestByteArray)
    {
        if (IsConnected)
        {
            await webSocket.SendAsync(new ArraySegment<byte>(requestByteArray), WebSocketMessageType.Binary, true, CancellationToken.None);

            var buffer = new byte[1024];
            var sb = new StringBuilder();

            WebSocketReceiveResult result;
            do
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                sb.Append(Encoding.ASCII.GetString(buffer, 0, result.Count));
            } while (!result.EndOfMessage);

            string receivedData = sb.ToString();
            Console.WriteLine($"Received data from server: {receivedData}");
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            webSocket.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
