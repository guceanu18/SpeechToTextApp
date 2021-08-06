using System;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.System.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Client;
using System.IO;
using Windows.Storage.Streams;

namespace SpeechToTextApp
{
    public sealed partial class MainPage : Page
    {
        MediaCapture mediaCapture;

        DisplayRequest displayRequest = new DisplayRequest();

        public static MemoryStream memoryStream = new MemoryStream();
        public IRandomAccessStream stream = memoryStream.AsRandomAccessStream();

        public MainPage()
        {
            this.InitializeComponent();

            btnRec.IsEnabled = true;
            btnStop.IsEnabled = false;

            var taskInitializeMediaCaptureAsync = Task.Run(async () => await InitializeMediaCaptureAsync());

        }

        async Task InitializeMediaCaptureAsync()
        {
            mediaCapture = new MediaCapture();
            mediaCapture = new MediaCapture();
            await mediaCapture.InitializeAsync();
            //mediaCapture.Failed += MediaCapture_Failed;
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
            btnRec.IsEnabled = true;
            btnStop.IsEnabled = false;
        }

        void btnRec_Click(object sender, RoutedEventArgs e)
        {
            btnStop.IsEnabled = true;
            btnRec.IsEnabled = false;

            Task taskStartRecording = Task.Run(async () => await startRecording());
        }

        void btnStop_Click(object sender, RoutedEventArgs e)
        {
            btnStop.IsEnabled = false;
            btnRec.IsEnabled = true;

            var task = stopRecording();

            Client.Client.start(stream);
        }
    }
}