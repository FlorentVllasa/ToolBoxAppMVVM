using System;
using System.Diagnostics;
using System.Windows.Input;
using ToolBoxApp.BaseClasses;
using ToolBoxApp.Commands;
using ToolBoxApp.Services;
using ToolBoxApp.Views;
using Windows.UI.Xaml.Controls;

namespace ToolBoxApp.ViewModels
{
    public class AudioHomeViewModel : MainViewModelBase
    {
        private NavigationService _navigationService;

        private Type _scrollAudioView;

        public Type ScrollAudioView
        {
            get
            {
                return _scrollAudioView;
            }
            set
            {
                _scrollAudioView = value;
                OnPropertyChanged("ScrollAudioView");
            }
        }

        public AudioHomeViewModel(NavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        private ICommand _navigateToView;

        public ICommand NavigateToView
        {
            get 
            {
                return _navigateToView =
                    new GenericRelayCommand<NavigationViewItemInvokedEventArgs>(OnItemInvoked);
            }
        }

        private ICommand _goBackToHome;

        public ICommand GoBackToHome
        {
            get 
            { 
                return _goBackToHome = new RelayCommand((a) => 
                {
                    _navigationService.Navigate(typeof(HomeView));
                }); 
            }
        }


        public void OnItemInvoked(NavigationViewItemInvokedEventArgs args)
        {
            string invokedItemName = args.InvokedItem.ToString();
            Debug.WriteLine(args.InvokedItem.ToString());

            if (invokedItemName.Equals("Text to Speech"))
            {
                ScrollAudioView = typeof(AudioTextToSpeechView);
                
            }
            else if (invokedItemName.Equals("Youtube to Mp3"))
            {
                ScrollAudioView = typeof(AudioMp3DownloaderView);
            }
        }

    }
}
