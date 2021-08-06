using System;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.System.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Client;

namespace SpeechToTextApp
{
    public sealed partial class MainPage : Page
    {

        StorageFile file;

        MediaCapture mediaCapture;

        LowLagMediaRecording _mediaRecording;

        DisplayRequest displayRequest = new DisplayRequest();

        string[] filenames;

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
        {/*
            //var myVideos = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Videos);

            //var myVideos = new Object();

            var myVideos = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(@"C:\Users\cioca\source\repos\SpeechToTextApp\bin\x86\Debug");

            *//*Task.WaitAll(Task.Run(async () => {
                myVideos = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(@"C:\Users\cioca\source\repos\SpeechToTextApp\bin\x86\Debug");
            }));*//*

            //Task task = Windows.Storage.StorageFolder.GetFolderFromPathAsync(@"C:\Users\cioca\source\repos\SpeechToTextApp\bin\x86\Debug");

            //file = await myVideos.SaveFolder.CreateFileAsync("audio.wav", CreationCollisionOption.GenerateUniqueName);
            file = await myVideos.CreateFileAsync("audio.wav", CreationCollisionOption.GenerateUniqueName);

            _mediaRecording = await mediaCapture.PrepareLowLagRecordToStorageFileAsync(
                    MediaEncodingProfile.CreateWav(AudioEncodingQuality.High), file);*/


            /*await Task.Run(async () =>
            {
                await generateFolder();
            });*/

            getFolder();

            await _mediaRecording.StartAsync();
        }

        void getFolder()
        {
            Task.Run(async () =>
            {
                await generateFolder();
            }).Wait();
        }

        async Task generateFolder()
        {
            string root = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;

            var myVideos = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(root);

            file = await myVideos.CreateFileAsync("audio.wav", CreationCollisionOption.GenerateUniqueName);

            _mediaRecording = await mediaCapture.PrepareLowLagRecordToStorageFileAsync(
                    MediaEncodingProfile.CreateWav(AudioEncodingQuality.High), file);
        }

        async Task stopRecording()
        {
            await _mediaRecording.StopAsync();
            //Application.Current.Exit();
            btnRec.IsEnabled = true;
            btnStop.IsEnabled = false;
            /*
                        string sourcePath = "C:\\Users\\cioca\\Videos";
                        string destinationPath = @"C:\\Users\\cioca\\source\\repos\\SpeechToTextApp\\bin\\x86\\Debug";

                        string sourceFile = System.IO.Path.Combine(sourcePath, file.Name);
                        string destinationFile = System.IO.Path.Combine(destinationPath, file.Name);

                        System.IO.File.Copy(sourceFile, destinationFile, true);*/
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

            /* Task taskStopRecording = Task.Run(async () => await stopRecording());*/

            var task = stopRecording();

            Client.Client.start(new string[] { file.Name });
        }
    }
}