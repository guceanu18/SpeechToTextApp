using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;

namespace Client
{
    public class Client
    {
        static string filename;
        static string port;
        static string key;

        async Task DecodeFile()
        {
            ClientWebSocket ws = new ClientWebSocket();
            await ws.ConnectAsync(new Uri("ws://live-transcriber.zevo-tech.com:" + port), CancellationToken.None);

            byte[] api = Encoding.UTF8.GetBytes("{\"config\": {\"key\": \"" + key + "\"}}");
            await ws.SendAsync(new ArraySegment<byte>(api, 0, api.Length), WebSocketMessageType.Text, true, CancellationToken.None);

            FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

            byte[] data = new byte[16000];

            while (true)
            {
                int count = fileStream.Read(data, 0, 16000);
                if (count == 0) break;

                await ProcessData(ws, data, count);
            }

            await ProcessFinalData(ws);

            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "OK", CancellationToken.None);
        }

        async Task ProcessData(ClientWebSocket ws, byte[] data, int count)
        {
            await ws.SendAsync(new ArraySegment<byte>(data, 0, count), WebSocketMessageType.Binary, true, CancellationToken.None);
            await RecieveResult(ws);
        }

        async Task ProcessFinalData(ClientWebSocket ws)
        {
            byte[] eof = Encoding.UTF8.GetBytes("{\"eof\" : 1}");
            await ws.SendAsync(new ArraySegment<byte>(eof), WebSocketMessageType.Text, true, CancellationToken.None);
            await RecieveResult(ws);
        }

        async Task RecieveResult(ClientWebSocket ws)
        {
            byte[] result = new byte[4096];
            Task<WebSocketReceiveResult> receiveTask = ws.ReceiveAsync(new ArraySegment<byte>(result), CancellationToken.None);
            await receiveTask;
            var receivedString = Encoding.UTF8.GetString(result, 0, receiveTask.Result.Count);
            Console.WriteLine("Result {0}", receivedString);
        }

        public static void Main(string file)
        {

            filename = file;
            port = "12320";
            key = "e4b70d22ca4b47369fbbc46b2afa3c33";

            Task.Run(async () =>
            {
                await new Client().DecodeFile();
            }).GetAwaiter().GetResult();
        }
    }
}

