using Richasy.Controls.Reader.Models;
using Richasy.Controls.Reader.Models.Epub;
using System;
using System.Linq;
using System.Text;
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

        private async void View_Loaded_1(object sender, RoutedEventArgs e)
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/中信国学大典·第1辑.epub"));
            var book = await EpubReader.Read(file, Encoding.Default);
            var chapter = book.SpecialResources.HtmlInReadingOrder[7];
            //var htmlFile = book.Resources.Html.Where(p => p.AbsolutePath.Equals(chapter.AbsolutePath, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (chapter != null)
            {
                View.Init(book, new EpubViewStyle());
                View.SetContent(chapter.TextContent);
            }
        }
    }
}
