using Newtonsoft.Json;
using Richasy.Controls.Reader.Models;
using Richasy.Controls.Reader.Models.Epub;
using Richasy.Helper.UWP;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using Windows.Storage;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SampleApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<Chapter> ChapterCollection = new ObservableCollection<Chapter>();
        private Instance instance = new Instance("Reader");
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

        private async void Reader_OpenCompleted(object sender, EventArgs e)
        {
            
            
            LoadingRing.IsActive = false;
        }

        private void Reader_ProgressChanged(object sender, History e)
        {
            if (e != null)
                instance.App.WriteLocalSetting(Settings.History, JsonConvert.SerializeObject(e));
            ProgressBlock.Text = Math.Ceiling(e.Progress) + "%";
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

        private async void Button_Click(object sender, RoutedEventArgs e)
        {

            var file = await instance.IO.OpenLocalFileAsync(".epub", ".txt");
            if (file != null)
            {
                DisplayGrid.Visibility = Visibility.Visible;
                FileButton.Visibility = Visibility.Collapsed;
                try
                {
                    if (Path.GetExtension(file.Path) == ".epub")
                    {
                        await Reader.OpenAsync(file, new EpubViewStyle());
                    }
                    else
                    {
                        await Reader.OpenAsync(file, new TxtViewStyle());
                    }
                    
                }
                catch (Exception ex)
                {
                    await new MessageDialog(ex.Message).ShowAsync();
                    DisplayGrid.Visibility = Visibility.Collapsed;
                    FileButton.Visibility = Visibility.Visible;
                    LoadingRing.IsActive = false;
                }

            }
        }

        private void Reader_SetContentStarting(object sender, EventArgs e)
        {
            LoadingRing.IsActive = true;
        }

        private void Reader_SetContentCompleted(object sender, EventArgs e)
        {
            LoadingRing.IsActive = false;
        }

        private async void CommandButton_Click(object sender, RoutedEventArgs e)
        {
            await new MessageDialog($"Current selected text: {Reader.SelectedText}").ShowAsync();
        }

        private async void Reader_ImageTapped(object sender, ImageEventArgs e)
        {
            var byteArray = Convert.FromBase64String(e.Base64);
            var stream = byteArray.AsBuffer().AsStream().AsRandomAccessStream();
            using (stream)
            {
                var bitmap = new BitmapImage();
                await bitmap.SetSourceAsync(stream);
                // do other thing
            }
        }

        private async void Reader_LinkTapped(object sender, LinkEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Link))
                await Launcher.LaunchUriAsync(new Uri(e.Link));
            else
            {
                if (!string.IsNullOrEmpty(e.Id))
                {
                    var tip = Reader.GetSpecificIdContent(e.Id, e.FileName);
                    await new MessageDialog(tip.Description, tip.Title).ShowAsync();
                }
                else if (!string.IsNullOrEmpty(e.FileName))
                    Reader.LocateToSpecificFile(e.FileName);
            }
        }

        private async void Reader_ViewLoaded(object sender, EventArgs e)
        {
            try
            {
                var history = JsonConvert.DeserializeObject<History>(instance.App.GetLocalSetting(Settings.History, "{}"));
                if (history?.Chapter != null)
                    Reader.LoadHistory(history);
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private void Reader_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Left)
                Reader.Previous();
            else if (e.Key == Windows.System.VirtualKey.Right)
                Reader.Next();
        }
    }

    public class LevelMarginCovnerter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var level = (int)value;
            return new Thickness((level - 1) * 20, 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public enum Settings
    {
        History
    }
}
