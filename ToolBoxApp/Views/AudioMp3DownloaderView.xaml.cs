using System;
using ToolBoxApp.Services;
using ToolBoxApp.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ToolBoxApp.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AudioMp3DownloaderView : Page
    {
        public AudioMp3DownloaderView()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            var viewModel = new AudioMp3DownloaderViewModel(new NavigationService());
            DataContext = viewModel;
        }

    }
}
