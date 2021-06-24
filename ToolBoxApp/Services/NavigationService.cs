using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolBoxApp.Interfaces;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ToolBoxApp.Services
{
    public class NavigationService : INavigationService
    {
        public void GoBack()
        {
            var frame = (Frame)Window.Current.Content;
            frame.GoBack();
        }

        public void Navigate(Type sourcePage)
        {
            var frame = (Frame)Window.Current.Content;
            frame.Navigate(sourcePage);    
        }

        public void Navigate(Type sourcePage, object parameter)
        {
            var frame = (Frame)Window.Current.Content;
            frame.Navigate(sourcePage, parameter);
        }

        public void NavigateScrollViewer(Type sourcePage, Type injectPage)
        {
            var frame = (Frame)Window.Current.Content;
            var window = Window.Current.Content;
            Debug.WriteLine(window.XamlRoot);
                
            var page = frame.CurrentSourcePageType;
            Debug.WriteLine(page);
        }
    }
}
