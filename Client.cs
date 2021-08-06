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
        private static string port;
        private static string key;
        private static string filename;

        public Client()
        {
            port = "12320";
            key = "e4b70d22ca4b47369fbbc46b2afa3c33";
    }

        public Client(string fileName)
        {
            port = "12320";
            key = "e4b70d22ca4b47369fbbc46b2afa3c33";
            filename = fileName;
        }

        public static void start(string[] args)
        {
            filename = args[0];
            Task.Run(async () =>
            {
                await new Client(filename).DecodeFile();
            }).Wait();
        }

        async Task DecodeFile()
        {
            ClientWebSocket ws = new ClientWebSocket();
            await ws.ConnectAsync(new Uri("ws://live-transcriber.zevo-tech.com:" + port), CancellationToken.None);

            byte[] api = Encoding.UTF8.GetBytes("{\"config\": {\"key\": \"" + key + "\"}}");
            await ws.SendAsync(new ArraySegment<byte>(api, 0, api.Length), WebSocketMessageType.Text, true, CancellationToken.None);

            /*int outRate = 8000;
            var intFile = filename;
            var outFile = "modified_"+filename;

            using (var reader = new WaveFileReader(intFile))
            {
                var outFormat = new WaveFormat(outRate, reader.WaveFormat.Channels);
                *//*using (var resampler = new MediaFoundationResampler(reader, outFormat))
                {
                    WaveFileWriter.CreateWaveFile(outFile, resampler);
                }*//*
            }*/

            FileStream fileStream = new FileStream(@"C:\Users\cioca\source\repos\SpeechToTextApp\bin\x86\Debug\AppX\"+filename, FileMode.Open, FileAccess.Read);

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
            System.Diagnostics.Debug.WriteLine("Result {0}", receivedString);
        }
    }
}



/*
namespace Client
{
    public class Client
    {
        static string filename;
        static string port;
        static string key;

        static StreamWriter writefile;

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
            //Console.WriteLine("Result {0}", receivedString);
            writefile.WriteLine("Result {0}", receivedString);
        }

        public static void Main(string[] file)
        {

            filename = file.GetValue(0).ToString();
            port = "12320";
            key = "e4b70d22ca4b47369fbbc46b2afa3c33";

            File.SetAttributes(file.GetValue(0).ToString(), FileAttributes.Normal);
            writefile = new StreamWriter("Response.txt");

            Task.Run(async () =>
            {
                await new Client().DecodeFile();
            }).GetAwaiter().GetResult();
        }
    }
}

*/