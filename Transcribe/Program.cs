using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using NAudio.Wave;

namespace TestConnectionWebSocket
{
    public class Program
    {
        async Task speechtotext(Uri uri)
        {

            ClientWebSocket ws = new ClientWebSocket();
            await ws.ConnectAsync(uri, CancellationToken.None);

            byte[] key = Encoding.UTF8.GetBytes("{\"config\": { \"key\": \"e4b70d22ca4b47369fbbc46b2afa3c33\"}}");
            await ws.SendAsync(new ArraySegment<byte>(key), WebSocketMessageType.Text, true, CancellationToken.None);

            string fileName = "guc.wav";
            string sourcePath = @"C:\Users\40731\Pictures";
            string targetPath = @"C:\Users\40731\source\repos\ContinuousAudio\Transcribe\bin\Debug\netcoreapp3.1";

            // Use Path class to manipulate file and directory paths.
            string sourceFile = System.IO.Path.Combine(sourcePath, fileName);
            string destFile = System.IO.Path.Combine(targetPath, fileName);

            System.IO.File.Copy(sourceFile, destFile, true);


            int outRate = 8000;
            var inFile = @"guc.wav";
            var outFile = @"guc1.wav";
            using (var reader = new WaveFileReader(inFile))
            {
                var outFormat = new WaveFormat(outRate, reader.WaveFormat.Channels);
                using (var resampler = new MediaFoundationResampler(reader, outFormat))
                {
                    // resampler.ResamplerQuality = 60;
                    WaveFileWriter.CreateWaveFile(outFile, resampler);
                  
                }
            }

            FileStream fsSource = new FileStream("guc1.wav",
                       FileMode.Open, FileAccess.Read);

            byte[] data = new byte[16000];
            while (true)
            {
                int count = fsSource.Read(data, 0, 16000);
                if (count == 0)
                    break;

                await ws.SendAsync(new ArraySegment<byte>(data, 0, count), WebSocketMessageType.Binary, true, CancellationToken.None);
                byte[] result1 = new byte[4096];
                Task<WebSocketReceiveResult> receiveTask1 = ws.ReceiveAsync(new ArraySegment<byte>(result1), CancellationToken.None);
                await receiveTask1;
                var receivedString1 = Encoding.UTF8.GetString(result1, 0, receiveTask1.Result.Count);
                Console.WriteLine("Result {0}", receivedString1);
                if (receivedString1.Contains("message"))
                {
                    await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                }

            }

            byte[] eof = Encoding.UTF8.GetBytes("{\"eof\" : 1}");
            await ws.SendAsync(new ArraySegment<byte>(eof), WebSocketMessageType.Text, true, CancellationToken.None);
            byte[] result = new byte[4096];
            Task<WebSocketReceiveResult> receiveTask = ws.ReceiveAsync(new ArraySegment<byte>(result), CancellationToken.None);
            await receiveTask;
            var receivedString = Encoding.UTF8.GetString(result, 0, receiveTask.Result.Count);
            Console.WriteLine("Result {0}", receivedString);

            System.Net.ServicePointManager.Expect100Continue = false;
        }
        public static void Main()
        {
            Task.Run(async () => {
                await new Program().speechtotext(new Uri("ws://live-transcriber.zevo-tech.com:12320"));
            }).GetAwaiter().GetResult();
        }
    }
}
