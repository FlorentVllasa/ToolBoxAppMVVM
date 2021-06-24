using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ToolBoxApp.BaseClasses;
using ToolBoxApp.Commands;
using ToolBoxApp.Interfaces;
using ToolBoxApp.Views;

namespace ToolBoxApp.ViewModels
{
    public class HomeViewModel : MainViewModelBase
    {
        private INavigationService _navigationService;

        private ICommand _navigateToAudioHome;
        public ICommand NavigateToAudioHome
        {
            get
            {
                return _navigateToAudioHome =
                new RelayCommand((a) =>
                {
                    _navigationService.Navigate(typeof(AudioHomeView));
                });
            }
        }

        private ICommand _navigateToVideoHome;

        public ICommand NavigateToVideoHome
        {
            get 
            {
                return _navigateToVideoHome =
                    new RelayCommand((a) => 
                    {
                        _navigationService.Navigate(typeof(VideoHomeView));
                    });
            }
        }


        public HomeViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }
    }
}
