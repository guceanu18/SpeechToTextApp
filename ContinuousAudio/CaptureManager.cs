using System;
using Windows.Media.Capture;
using System.Threading.Tasks;
using Windows.System.Display;
using Windows.Storage;
using Windows.Media.MediaProperties;
using System.Diagnostics;
using Windows.Storage.Streams;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using Windows.ApplicationModel.Core;
using static System.Net.Mime.MediaTypeNames;
using NAudio.Wave;

namespace ContinuousAudio
{
    internal class CaptureManager
    {
        public DisplayRequest displayRequest = new DisplayRequest();
        public bool isInitialized;
        public bool isRecording;
        private StorageFolder captureFolder = null;
        public static MemoryStream memStream = new MemoryStream();
        public IRandomAccessStream stream = memStream.AsRandomAccessStream();

        MediaCapture captureManager;

        public async Task InitializeCameraAsync()
        {
            captureManager = new MediaCapture();
            await captureManager.InitializeAsync();
            isInitialized = true;
            if (isInitialized)
            {
                await StartRecordingAudioAsync();
            }
        }

        public async Task StartRecordingAudioAsync()
        {

            /*var picturesLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
            captureFolder = picturesLibrary.SaveFolder;
            //captureFolder = @"C:\Users\40731\source\repos\ConnectToWebSocket\ConnectToWebSocket\bin\Debug\netcoreapp3.1";
            var audioFile = await captureFolder.CreateFileAsync("guc.wav", CreationCollisionOption.GenerateUniqueName);*/
            var encodingProfile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.Medium);
            encodingProfile.Audio = AudioEncodingProperties.CreatePcm(16000, 1, 16);
            //Debug.WriteLine("Starting recording to " + audioFile.Path);

            //await captureManager.StartRecordToStorageFileAsync(encodingProfile, audioFile);

            await captureManager.StartRecordToStreamAsync(encodingProfile, stream);

            Debug.WriteLine("Started recording!");
            isRecording = true;
            
        }

        public async Task StopRecordingAudioAsync()
        {
            isRecording = false;
            await captureManager.StopRecordAsync();
            await speechtotext(new Uri("ws://live-transcriber.zevo-tech.com:12320"));
            Debug.WriteLine("Stopped recording!");
            
        }

        public async Task speechtotext(Uri uri)
        {

            ClientWebSocket ws = new ClientWebSocket();
            await ws.ConnectAsync(uri, CancellationToken.None);

            byte[] key = Encoding.UTF8.GetBytes("{\"config\": { \"key\": \"e4b70d22ca4b47369fbbc46b2afa3c33\"}}");
            await ws.SendAsync(new ArraySegment<byte>(key), WebSocketMessageType.Text, true, CancellationToken.None);

            MemoryStream msSource = (MemoryStream)stream.AsStreamForRead();

        
            byte[] data = new byte[16000];
            while (true)
            {
                int count = msSource.Read(data, 0, 16000);
                if (count == 0)
                    break;

                await ws.SendAsync(new ArraySegment<byte>(data, 0, count), WebSocketMessageType.Binary, true, CancellationToken.None);
                byte[] result1 = new byte[4096];
                Task<WebSocketReceiveResult> receiveTask1 = ws.ReceiveAsync(new ArraySegment<byte>(result1), CancellationToken.None);
                await receiveTask1;
                var receivedString1 = Encoding.UTF8.GetString(result1, 0, receiveTask1.Result.Count);
                Debug.WriteLine("Result {0}", receivedString1);
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
            Debug.WriteLine("Result {0}", receivedString);

            System.Net.ServicePointManager.Expect100Continue = false;
        }


    }
}
