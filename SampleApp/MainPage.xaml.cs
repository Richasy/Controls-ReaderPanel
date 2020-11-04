using Richasy.Controls.Reader.Models;
using Richasy.Controls.Reader.Models.Epub;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SampleApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<Chapter> ChapterCollection = new ObservableCollection<Chapter>();
        public MainPage()
        {
            this.InitializeComponent();
        }


        private void Reader_ChapterLoaded(object sender, System.Collections.Generic.List<Chapter> e)
        {
            ChapterCollection.Clear();
            e.ForEach(p => ChapterCollection.Add(p));
        }

        private void Reader_OpenStarting(object sender, EventArgs e)
        {
            LoadingRing.IsActive = true;
        }

        private void Reader_OpenCompleted(object sender, EventArgs e)
        {
            LoadingRing.IsActive = false;
        }

        private void Reader_ProgressChanged(object sender, History e)
        {
            ProgressBlock.Text = Math.Ceiling(e.Progress) + "%";
        }

        private async void Reader_Loaded(object sender, RoutedEventArgs e)
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/归藏图：引渡人.txt"));
            try
            {
                Reader.ChapterDivisionRegex = new Regex(@"^(Ⅰ|Ⅱ|Ⅲ|Ⅳ|Ⅴ|Ⅵ|Ⅶ|Ⅷ|Ⅸ|Ⅹ).+");
                await Reader.OpenAsync(file, new TxtViewStyle());
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
                LoadingRing.IsActive = false;
            }

        }

        private void ChapterListView_ItemClick(object sender, ItemClickEventArgs e)
        {

            var chapter = e.ClickedItem as Chapter;
            Reader.LoadChapter(chapter);
        }

        private void Reader_ChapterChanged(object sender, Chapter e)
        {
            string title = e.Title;
            ChapterTitleBlock.Text = title;
            ChapterListView.SelectedItem = e;
            ChapterListView.ScrollIntoView(e, ScrollIntoViewAlignment.Leading);
        }
    }

    public class LevelMarginCovnerter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var level = (int)value;
            return new Thickness((level-1) * 20, 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
