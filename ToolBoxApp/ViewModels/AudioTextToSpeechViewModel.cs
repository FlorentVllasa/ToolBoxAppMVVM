using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ToolBoxApp.BaseClasses;
using ToolBoxApp.Commands;
using ToolBoxApp.Services;
using Windows.Media.SpeechSynthesis;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;

namespace ToolBoxApp.ViewModels
{
    class AudioTextToSpeechViewModel : MainViewModelBase
    {
        private NavigationService _navigationService;

        public AudioTextToSpeechViewModel(NavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        private string _toSay;
        private string _errorMessage;
        private ICommand _playAudioCommand;
        private ICommand _saveAudioCommand;


        public string ToSay
        {
            get => _toSay;
            set 
            { 
                _toSay = value;
                OnPropertyChanged("ToSay");
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
        public ICommand PlayAudioCommand
        {
            get 
            { 
                return _playAudioCommand = new RelayCommand(async(a) => 
                {
                    await PlayAudio();
                }); 
            }
        }
        public ICommand SaveAudioCommand
        {
            get
            {
                return _saveAudioCommand = new RelayCommand(async (a) =>
                {
                    await SaveAsAudio();
                });
            }
        }


        public async Task PlayAudio()
        {
            if (!string.IsNullOrEmpty(ToSay))
            {
                ErrorMessage = "";
                MediaElement mediaElement = new MediaElement();
                var synth = new SpeechSynthesizer();
                SpeechSynthesisStream stream = await synth.SynthesizeTextToStreamAsync(ToSay);
                mediaElement.SetSource(stream, stream.ContentType);
                mediaElement.Play();
            }
            else
            {
                ErrorMessage = "Please write something to say!";
            }
        }

        private async Task SaveAsAudio()
        {
            if (!string.IsNullOrEmpty(ToSay))
            {
                ErrorMessage = "";
                StorageFolder storageFolder = KnownFolders.MusicLibrary;
                StorageFile storageFile = await storageFolder.CreateFileAsync("audio.mp3", CreationCollisionOption.GenerateUniqueName);

                if (storageFile != null)
                {
                    try
                    {
                        var synth = new SpeechSynthesizer();
                        SpeechSynthesisStream stream = await synth.SynthesizeTextToStreamAsync(ToSay);

                        using (var reader = new DataReader(stream))
                        {
                            await reader.LoadAsync((uint)stream.Size);
                            IBuffer buffer = reader.ReadBuffer((uint)stream.Size);
                            await FileIO.WriteBufferAsync(storageFile, buffer);
                        }
                        ErrorMessage = "Your file has been saved under the Music Folder!";

                    }
                    catch { }
                }
            }
            else
            {
                ErrorMessage = "You should say something before you want to save!";
            }
        }

    }
}
