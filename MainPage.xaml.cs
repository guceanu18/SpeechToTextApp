using System;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.System.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.IO;
using Windows.Storage.Streams;
using Windows.UI.Core.Preview;

namespace SpeechToTextApp
{
    public sealed partial class MainPage : Page
    {
        MediaCapture mediaCapture;

        DisplayRequest displayRequest = new DisplayRequest();

        public static MemoryStream memoryStream = new MemoryStream();
        public IRandomAccessStream stream = memoryStream.AsRandomAccessStream();

        private static Client.Client client;

        public MainPage()
        {
            this.InitializeComponent();
            btnRec.IsEnabled = false;
            btnStop.IsEnabled = false;

            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += this.OnCloseRequest;

            client = new Client.Client();
            Task task1 =  client.ConnectToServer();
            Task task2 = client.CreateFile();

            var taskInitializeMediaCaptureAsync = Task.Run(async () => await InitializeMediaCaptureAsync());

            btnRec.IsEnabled = true;
            btnStop.IsEnabled = false;
        }

        private void OnCloseRequest(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            Task task = client.DisconnectFromServer();
            task.Wait();
        }

        async Task InitializeMediaCaptureAsync()
        {
            mediaCapture = new MediaCapture();
            await mediaCapture.InitializeAsync();
        }

        async Task startRecording()
        {
            MediaEncodingProfile encodingProfile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.Medium);
            encodingProfile.Audio = AudioEncodingProperties.CreatePcm(16000, 1, 16);
            await mediaCapture.StartRecordToStreamAsync(encodingProfile, stream);
        }

        async Task stopRecording()
        {
            await mediaCapture.StopRecordAsync();
        }

        async void btnRec_Click(object sender, RoutedEventArgs e)
        {
            await startRecording();
            btnStop.IsEnabled = true;
            btnRec.IsEnabled = false;
        }

        async void btnStop_Click(object sender, RoutedEventArgs e)
        {
            await stopRecording();
            client.StreamHandler(stream);

            btnStop.IsEnabled = false;
            btnRec.IsEnabled = true;        
        }
    }
}