using Richasy.Controls.Reader.Models;
using System;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SampleApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            
        }

        private async void View_Loaded(object sender, RoutedEventArgs e)
        {
            string content = await PathIO.ReadTextAsync("ms-appx:///Assets/01.txt");
            View.SetContent(content);
        }
    }
}
