using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using ToolBoxApp.BaseClasses;
using ToolBoxApp.Commands;
using ToolBoxApp.Services;
using VideoLibrary;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;
using Windows.Storage.Pickers;
using VideoLibrary.Exceptions;
using System.Net.Http;

namespace ToolBoxApp.ViewModels
{
    public class AudioMp3DownloaderViewModel : MainViewModelBase
    {
        private NavigationService navigationService;


        private string _youtubeUrl;
        private string _errorMessage;
        private int _progressBarValue;
        private ICommand _downloadYoutubeMp3;
        private bool _isDownloadButtonEnabled;

        public string YoutubeUrl
        {
            get 
            { 
                return _youtubeUrl; 
            }
            set 
            { 
                _youtubeUrl = value;
                OnPropertyChanged("YoutubeUrl");
            }
        }
        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                _errorMessage = value;
                OnPropertyChanged("ErrorMessage");
            }
        }
        public int ProgressBarValue
        {
            get 
            { 
                return _progressBarValue; 
            }
            set 
            { 
                _progressBarValue = value;
                OnPropertyChanged("ProgressBarValue");
            }
        }
        public bool IsDownloadButtonEnabled
        {
            get
            {
                return _isDownloadButtonEnabled;
            }
            set
            {
                _isDownloadButtonEnabled = value;
                OnPropertyChanged("IsDownloadButtonEnabled");
            }
        }
        public ICommand DownloadYoutubeMp3
        {
            get 
            { 
                return _downloadYoutubeMp3 = new RelayCommand(async(a) => 
                {
                    await DownloadMp3File();
                }); 
            }
        }

        public StorageFile VideoFile;
        public StorageFile Mp3File;
        public StorageFolder SaveFolder;
        public YouTubeVideo DownloadedVideo;


        private CoreDispatcher _coreDispatcher;

        public CoreDispatcher CoreDispatcher
        {
            get 
            {
                return _coreDispatcher;
            }
            set
            {
                _coreDispatcher = value;
            }
        }


        public AudioMp3DownloaderViewModel(NavigationService navigationService)
        {
            this.navigationService = navigationService;
            IsDownloadButtonEnabled = true;
            CoreDispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
        }


        private async Task DownloadMp3File()
        {

            if (!string.IsNullOrEmpty(YoutubeUrl) && YoutubeUrl.Contains("youtube") && YoutubeUrl.Contains("https://www.youtube".ToLower()))
            {

                await CallFunctionInsideUiThreadActionParameter(
                    ResetErrorMessageAndButtonVisibility,
                    "",
                    CoreDispatcher
                    );

                //await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                //{
                //    ErrorMessage = "";
                //    IsDownloadButtonEnabled = false;
                //});

                try
                {
                    var video = await GetVideoObject();

                    StorageFolder SaveFolder = KnownFolders.MusicLibrary;

                    string savePath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                    string videoPath = Path.Combine(savePath, video.FullName);
                    string mp3Path = Path.Combine(savePath, videoPath.Replace(".mp4", ".mp3"));

                    StorageFile videoFile = await SaveFolder.CreateFileAsync(Path.GetFileName(videoPath), CreationCollisionOption.ReplaceExisting);
                    StorageFile mp3File = await SaveFolder.CreateFileAsync(Path.GetFileName(mp3Path), CreationCollisionOption.ReplaceExisting);


                    await Task.Run(() => WriteBytesIntoVideoFile(videoFile, video));
                    await ConvertMp4ToMp3(videoFile, mp3File);

                    await videoFile.DeleteAsync();

                    await CallFunctionInsideUiThreadActionParameter(
                        SetErrorMessageWithUiResets,
                        "File has been saved in your standard music folder",
                        CoreDispatcher
                        );
                    //https://www.youtube.com/watch?v=t1YHv1wHAxo

                    //await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    //{
                    //    ErrorMessage = "File has been saved in your standard music folder";
                    //    ProgressBarValue = 0;
                    //    IsDownloadButtonEnabled = true;
                    //    YoutubeUrl = "";
                    //});

                }
                catch (UnavailableStreamException) 
                {
                    await CallFunctionInsideUiThreadActionParameter(
                        SetErrorMessageWithUiResets,
                        "The youtube Url has no video!",
                        CoreDispatcher
                        );
                    //await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    //{
                    //    ErrorMessage = "The youtube Url has no video!";
                    //    ProgressBarValue = 0;
                    //    IsDownloadButtonEnabled = true;
                    //});

                }
                catch (HttpRequestException)
                {
                    await CallFunctionInsideUiThreadActionParameter(
                        SetErrorMessageWithUiResets,
                        "There seems to be an internet connection problem. Try again",
                        CoreDispatcher
                        );

                    //await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    //{
                    //    ErrorMessage = "There seems to be an internet connection problem. Try again";
                    //    ProgressBarValue = 0;
                    //    IsDownloadButtonEnabled = true;
                    //});
                }
                catch (FileLoadException)
                {
                    await CallFunctionInsideUiThreadActionParameter(
                        SetErrorMessageWithUiResets,
                        "File cannot be accessed. Mp3 will be deleted. Try again!",
                        CoreDispatcher
                        );
                    //await DeleteBrokenFiles(VideoFile, Mp3File);

                    //await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    //{
                    //    ErrorMessage = "File cannot be accessed. Mp3 will be deleted. Try again!";
                    //    ProgressBarValue = 0;
                    //    IsDownloadButtonEnabled = true;
                    //});
                }
            }
            else
            {
                await CallFunctionInsideUiThreadActionParameter(
                    SetErrorMessageWithUiResets,
                    "Please insert a valid youtube link!",
                    CoreDispatcher
                    );

                //await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                //{
                //    ErrorMessage = "Please insert a valid youtube link!";
                //    ProgressBarValue = 0;
                //    IsDownloadButtonEnabled = true;
                //});
            }
        }

        public async Task<YouTubeVideo> GetVideoObject()
        {
            var youTube = YouTube.Default;
            var video = await youTube.GetVideoAsync(YoutubeUrl);
            return video;
        }

        public async Task WriteBytesIntoVideoFile(StorageFile videoFile, YouTubeVideo downloadedVideo)
        {
            //await FileIO.WriteBytesAsync(videoFile, downloadedVideo.GetBytes());
            //ResetProgressBar();

            var outputStream = await videoFile.OpenAsync(FileAccessMode.ReadWrite);
            using (var dataWriter = new DataWriter(outputStream))
            {

                byte[] videoBytes = downloadedVideo.GetBytes();
                int videoBytesArrayLength = videoBytes.Length;

                int byteArrayFourth = videoBytesArrayLength / 4;
                int toAddValue = videoBytesArrayLength / 4;


                for (int i = 0; i < videoBytesArrayLength; i++)
                {
                    //Debug.WriteLine(byteArrayFourth);
                    if (i == byteArrayFourth)
                    {

                        await CallFunctionInsideUiThreadAction(
                            IncrementProgressBar,
                            CoreDispatcher
                            );
                        byteArrayFourth += toAddValue;
                        //await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        //{
                        //    ProgressBarValue += 25;
                        //    byteArrayFourth += toAddValue;
                        //});
                    }
                    dataWriter.WriteByte(videoBytes[i]);
                }
                await dataWriter.StoreAsync();
                await outputStream.FlushAsync();
            }

        }

        public void IncrementProgressBar()
        {
            ProgressBarValue += 25;
        }

        public void ResetErrorMessageAndButtonVisibility(string message)
        {
            ErrorMessage = message;
            IsDownloadButtonEnabled = false;
        }

        public void SetErrorMessageWithUiResets(string message)
        {
            ErrorMessage = message;
            ProgressBarValue = 0;
            IsDownloadButtonEnabled = true;
            YoutubeUrl = "";
        }

        public async Task CallFunctionInsideUiThreadAction(Func<Task> function, CoreDispatcher dispatcher)
        {
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                function.Invoke();
            });
        }

        public async Task CallFunctionInsideUiThreadActionParameter(Action<string> function, string message, CoreDispatcher dispatcher)
        {
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                function(message);
            });
        }

        public async Task CallFunctionInsideUiThreadAction(Action function, CoreDispatcher dispatcher)
        {
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                function();
            });
        }

        public async Task ConvertMp4ToMp3(IStorageFile videoFile, IStorageFile mp3File)
        {
            MediaEncodingProfile profile = MediaEncodingProfile.CreateMp3(AudioEncodingQuality.Auto);
            MediaTranscoder transcoder = new MediaTranscoder();

            PrepareTranscodeResult prepareOp = await transcoder.PrepareFileTranscodeAsync(videoFile, mp3File, profile);

            if (prepareOp.CanTranscode)
            {
                await prepareOp.TranscodeAsync();

            }
            else
            {
                switch (prepareOp.FailureReason)
                {
                    case TranscodeFailureReason.CodecNotFound:
                        System.Diagnostics.Debug.WriteLine("Codec not found.");
                        break;
                    case TranscodeFailureReason.InvalidProfile:
                        System.Diagnostics.Debug.WriteLine("Invalid profile.");
                        break;
                    default:
                        System.Diagnostics.Debug.WriteLine("Unknown failure.");
                        break;
                }
            }
        }

        private async Task SetDownloadLocation()
        {

            FolderPicker folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder pickedFolder = await folderPicker.PickSingleFolderAsync();

            if (pickedFolder != null)
            {
                Windows.Storage.AccessCache.StorageApplicationPermissions.
                FutureAccessList.AddOrReplace("PickedFolderToken", pickedFolder);
                Debug.WriteLine(pickedFolder.Path);
            }
        }
    }
}
