using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using Windows.Storage.Streams;
using Newtonsoft.Json.Linq;
using Windows.Storage;

namespace Client
{
    public class Client
    {
        public static ClientWebSocket ws;
        private static string port;
        private static string key;
        private static string sourceFile;

        public Client() 
        {
            port = "12320";
            key = "e4b70d22ca4b47369fbbc46b2afa3c33";
            ws = new ClientWebSocket();
        }
        
        public async Task ConnectToServer()
        {
            await ws.ConnectAsync(new Uri("ws://live-transcriber.zevo-tech.com:" + port), CancellationToken.None);

            byte[] api = Encoding.UTF8.GetBytes("{\"config\": {\"key\": \"" + key + "\"}}");
            await ws.SendAsync(new ArraySegment<byte>(api, 0, api.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public void StreamHandler(IRandomAccessStream inputStream)
        {
            MemoryStream memoryStream = (MemoryStream)inputStream.AsStreamForRead();

            byte[] data = new byte[16000];

            while (true)
            {
                int count = memoryStream.Read(data, 0, 16000);
                if (count == 0) break;

                Task.Run(async () => { await ProcessData(ws, data, count); } ).Wait();
            }
        }
        
        public async Task CreateFile()
        {
            string fileName = "text.txt";
            var picturesLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
            StorageFolder captureFolder = picturesLibrary.SaveFolder;
            var textFile = await captureFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
            fileName = textFile.Name;

            string localfolder = ApplicationData.Current.LocalFolder.Path;
            var array = localfolder.Split('\\');
            var username = array[2];
            string path = @"C:\Users\" + username + @"\Pictures";
            sourceFile = System.IO.Path.Combine(path, fileName);
        }

        async static Task ProcessData(ClientWebSocket ws, byte[] data, int count)
        {
            await ws.SendAsync(new ArraySegment<byte>(data, 0, count), WebSocketMessageType.Binary, true, CancellationToken.None);
            await RecieveResult(ws);
        }

        async static Task ProcessFinalData(ClientWebSocket ws)
        {
            byte[] eof = Encoding.UTF8.GetBytes("{\"eof\" : 1}");
            await ws.SendAsync(new ArraySegment<byte>(eof), WebSocketMessageType.Text, true, CancellationToken.None);
            await RecieveResult(ws);
        }

        async static Task RecieveResult(ClientWebSocket ws)
        {
            byte[] result = new byte[4096];
            Task<WebSocketReceiveResult> receiveTask = ws.ReceiveAsync(new ArraySegment<byte>(result), CancellationToken.None);
            await receiveTask;
            var receivedString = Encoding.UTF8.GetString(result, 0, receiveTask.Result.Count);

            if (receivedString.Contains("partial"))
            {
                saveText(jsonHandler(receivedString));
            }
            if (receivedString.Contains("message"))
            {
                await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }
        }

       static string jsonHandler(string receivedString)
        {
            JObject obj = JObject.Parse(receivedString);
            var item = obj["partial"].ToString();
            return item;
        }

        static void saveText(string item)
        {
            using (var destination = File.AppendText(sourceFile))
            {
                destination.WriteLine(item);
                System.Diagnostics.Debug.WriteLine(item);
            }
        }

        public async Task DisconnectFromServer()
        {
            await ProcessFinalData(ws);
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "OK", CancellationToken.None);
            ws.Abort();
        }

        /*private static string port;
        private static string key;
        private static IRandomAccessStream stream;
        


        public Client(IRandomAccessStream inputStream)
        {
            port = "12320";
            key = "e4b70d22ca4b47369fbbc46b2afa3c33";
            stream = inputStream;
        }

        public static void start(IRandomAccessStream inputStream)
        {
            stream = inputStream;
            Task.Run(async () =>
            {
                await new Client(inputStream).DecodeFile();
            }).Wait();
        }

        async Task DecodeFile()
        {
            ClientWebSocket ws = new ClientWebSocket();
            await ws.ConnectAsync(new Uri("ws://live-transcriber.zevo-tech.com:" + port), CancellationToken.None);

            byte[] api = Encoding.UTF8.GetBytes("{\"config\": {\"key\": \"" + key + "\"}}");
            await ws.SendAsync(new ArraySegment<byte>(api, 0, api.Length), WebSocketMessageType.Text, true, CancellationToken.None);

            MemoryStream memoryStream = (MemoryStream)stream.AsStreamForRead();

            string fileName = "text.txt";
            var picturesLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
            StorageFolder captureFolder = picturesLibrary.SaveFolder;
            var audioFile = await captureFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);

            byte[] data = new byte[16000];

            while (true)
            {
                int count = memoryStream.Read(data, 0, 16000);
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

            JObject obj = JObject.Parse(receivedString);
            string fileName = "text.txt";

            if (receivedString.Contains("text"))
            {
                
                string localfolder = ApplicationData.Current.LocalFolder.Path;
                var array = localfolder.Split('\\');
                var username = array[2];
                string path = @"C:\Users\" + username + @"\Pictures";
                string sourceFile = System.IO.Path.Combine(path, fileName);
                var item = obj["text"].ToString();
                //System.Diagnostics.Debug.WriteLine(item);
                using (var destination = File.AppendText(sourceFile))
                {
                    destination.WriteLine(item);
                }

            }
            if (receivedString.Contains("message"))
            {
                await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }
        }*/
    }
}