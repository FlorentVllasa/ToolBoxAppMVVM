using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ToolBoxApp.BaseClasses;
using ToolBoxApp.Commands;
using ToolBoxApp.Services;
using ToolBoxApp.Views;

namespace ToolBoxApp.ViewModels
{
    public class VideoHomeViewModel : MainViewModelBase
    {

        private NavigationService _navigationService;

        public VideoHomeViewModel(NavigationService navigationService)
        {
            _navigationService = navigationService;
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
    }
}
