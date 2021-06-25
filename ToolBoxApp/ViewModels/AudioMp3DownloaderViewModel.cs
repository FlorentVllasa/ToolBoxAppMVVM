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

        public AudioMp3DownloaderViewModel(NavigationService navigationService)
        {
            this.navigationService = navigationService;
            IsDownloadButtonEnabled = true;
        }


        private async Task DownloadMp3File()
        {

            if (!string.IsNullOrEmpty(YoutubeUrl) && YoutubeUrl.Contains("youtube") && YoutubeUrl.Contains("https://www.youtube".ToLower()))
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ErrorMessage = "";
                    IsDownloadButtonEnabled = false;
                });

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
 

                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        ErrorMessage = "File has been saved in your standard music folder";
                        ProgressBarValue = 0;
                        IsDownloadButtonEnabled = true;
                    });
                    //https://www.youtube.com/watch?v=t1YHv1wHAxo

                }
                catch (UnavailableStreamException) 
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        ErrorMessage = "The youtube Url has no video!";
                        IsDownloadButtonEnabled = true;
                    });
                }
                catch (HttpRequestException)
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        ErrorMessage = "There seems to be an internet connection problem. Try again";
                        IsDownloadButtonEnabled = true;
                    });
                }
                catch (FileLoadException)
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        ErrorMessage = "File cannot be accessed. Mp3 will be deleted. Try again!";
                        IsDownloadButtonEnabled = true;
                    });
                    //await DeleteBrokenFiles(VideoFile, Mp3File);
                }
            }
            else
            {
                //await CallFunctionInsideUiThreadActionParameter(SetErrorMessage, "Please insert a valid youtube link!");
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ErrorMessage = "Please insert a valid youtube link!";
                });
            }
        }

        public async Task DeleteBrokenFiles(StorageFile videoFile, StorageFile mp3File)
        {
            StorageFolder saveFolder = SaveFolder;
            StorageFile toDeleteVideo = await saveFolder.GetFileAsync(Path.GetFileName(videoFile.Path));
            StorageFile toDeleteMp3 = await saveFolder.GetFileAsync(Path.GetFileName(mp3File.Path));
            await toDeleteVideo.DeleteAsync();
            await toDeleteMp3.DeleteAsync();
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

                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            ProgressBarValue += 25;
                            byteArrayFourth += toAddValue;
                        });
                    }
                    dataWriter.WriteByte(videoBytes[i]);
                }
                await dataWriter.StoreAsync();
                await outputStream.FlushAsync();
            }

        }

        //public async Task IncrementProgressBar()
        //{
        //    ProgressBarValue += 25;
        //}

        //public async Task ResetProgressBar()
        //{
        //    ProgressBarValue = 0;
        //}

        //public async Task SetErrorMessage(string message)
        //{
        //    ErrorMessage = message;
        //}

        public async Task CallFunctionInsideUiThreadAction(Func<Task> function)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                function.Invoke();
            });
        }

        public async Task CallFunctionInsideUiThreadActionParameter(Action<string> function, string message)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                function(message);
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
