//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

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
            await mediaCapture.InitializeAsync();
            //mediaCapture.Failed += MediaCapture_Failed;
        }

        async Task startRecording()
        {
            
            var myVideos = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Videos);
            file = await myVideos.SaveFolder.CreateFileAsync("audio.wav", CreationCollisionOption.GenerateUniqueName);
            _mediaRecording = await mediaCapture.PrepareLowLagRecordToStorageFileAsync(
                    MediaEncodingProfile.CreateWav(AudioEncodingQuality.High), file);
            await _mediaRecording.StartAsync();
        }

        async Task stopRecording()
        {
            await _mediaRecording.StopAsync();
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

            Task taskStopRecording = Task.Run(async () => await stopRecording());

            Client.Client.Main(file.n);
        }
    }
}