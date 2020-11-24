using Newtonsoft.Json;
using Richasy.Controls.Reader.Enums;
using Richasy.Controls.Reader.Models;
using Richasy.Controls.Reader.Models.Epub;
using Richasy.Helper.UWP;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
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
        private StorageFile _localFile = null;
        private MediaPlaybackItem _playSource = null;
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
                _localFile = file;
                try
                {
                    await Reader.OpenAsync(file, new ReaderStyle());
                }
                catch (Exception ex)
                {
                    await new MessageDialog(ex.Message).ShowAsync();
                    LoadingRing.IsActive = false;
                }

            }
        }

        private void Reader_SetContentStarting(object sender, EventArgs e)
        {
            LoadingRing.IsActive = true;
            ReadButton.IsEnabled = false;
        }

        private void Reader_SetContentCompleted(object sender, EventArgs e)
        {
            LoadingRing.IsActive = false;
            ReadButton.IsEnabled = true;
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
                    var node = Reader.GetSpecificIdNode(e.Id, e.FileName);
                    if (node.Name == "body")
                        Reader.LocateToSpecificFile(e.FileName);
                    else
                    {
                        var tip = Reader.GetSpecificIdContent(node, e.Id);
                        await new MessageDialog(tip.Description, tip.Title).ShowAsync();
                    }
                }
                else if (!string.IsNullOrEmpty(e.FileName))
                    Reader.LocateToSpecificFile(e.FileName);
            }
        }

        private async void Reader_ViewLoaded(object sender, EventArgs e)
        {
            if (_localFile == null)
                return;
            try
            {
                var history = JsonConvert.DeserializeObject<History>(instance.App.GetLocalSetting(Settings.History, "{}"));
                if (history?.Chapter != null)
                    Reader.LoadHistory(history);
                else
                    Reader.LoadChapter(Reader.Chapters.First());
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private void Reader_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Left)
                Reader.Previous();
            else if (e.Key == VirtualKey.Right)
                Reader.Next();
        }

        private async void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            try
            {
                string text = args.QueryText;
                if (!string.IsNullOrEmpty(text))
                {
                    var result = await Reader.GetInsideSearchResultAsync(text);
                    if (result.Count > 0)
                    {
                        StringBuilder builder = new StringBuilder();
                        foreach (var item in result)
                        {
                            builder.AppendLine($"章节：{item.Chapter.Title}");
                            builder.AppendLine($"匹配文本：{item.SearchText}");
                            builder.AppendLine($"上下文：{item.DisplayText}");
                            builder.AppendLine("------");
                        }
                        await new MessageDialog(builder.ToString()).ShowAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                var source = await Reader.GetChapterVoiceAsync(Reader.CurrentChapter, false, new Windows.Media.SpeechSynthesis.SpeechSynthesizer());
                _playSource = source;
                var player = new MediaPlayer();
                player.Source = source;
                player.MediaEnded += MediaPlayer_Ended;
                MPE.SetMediaPlayer(player);
                player.Play();
                SpeechContainer.Visibility = Visibility.Visible;
                MPE.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
            
        }

        private async void MediaPlayer_Ended(MediaPlayer sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
             () =>
             {
                 SpeechContainer.Visibility = Visibility.Collapsed;
                 if (_playSource != null)
                 {
                     _playSource.Source.Dispose();
                     _playSource = null;
                 }
             });
        }

        private void Reader_SpeechCueChanged(object sender, SpeechCueEventArgs e)
        {
            if (e.Type == SpeechCueType.Word)
            {
                Reader.CheckCurrentReaderIndex(e.SpeechCue.StartPositionInInput);
            }
            else
            {
                SpeechBlock.Text = e.SpeechCue.Text;
            }
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
