using Richasy.Controls.Reader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Richasy.Controls.Reader
{
    public partial class ReaderPanel
    {
        public event EventHandler<Chapter> ChapterChanged;
        public event EventHandler<History> ProgressChanged;
        public event EventHandler<List<Chapter>> ChapterLoaded;
        public event EventHandler OpenStarting;
        public event EventHandler OpenCompleted;
        public event EventHandler SetContentStarting;
        public event EventHandler SetContentCompleted;
        public event EventHandler<PositionEventArgs> TouchTapped;
        public event EventHandler<LinkEventArgs> LinkTapped;
        public event EventHandler<ImageEventArgs> ImageTapped;

        public void OnPrevPageSelected(object sender, EventArgs args)
        {
            int index = CurrentChapter.Index;
            if (index < 1 || Chapters.Count == 0 || _isLoading)
                return;
            var prev = Chapters.Where(p => p.Index == index - 1).FirstOrDefault();
            if (prev == null)
            {
                return;
            }
            if (ReaderType == Enums.ReaderType.Txt)
            {
                string content = _txtContent.Substring(prev.StartLength, CurrentChapter.StartLength - prev.StartLength);
                _txtView.SetContent(content, Enums.ReaderStartMode.Last);
            }
            else
            {
                var orders = _epubContent.SpecialResources.HtmlInReadingOrder;
                if (_tempEpubChapterIndex < 1)
                    return;
                _tempEpubChapterIndex -= 1;
                var prevOrder = orders[_tempEpubChapterIndex];
                prev = GetLastEpubChapter(prevOrder);
                string content = prevOrder?.TextContent ?? prev.Title;
                _epubView.SetContent(content, Enums.ReaderStartMode.Last);
            }

            if (!prev.Equals(CurrentChapter))
            {
                CurrentChapter = prev;
                ChapterChanged?.Invoke(this, prev);
            }   
        }

        public void OnNextPageSelected(object sender, EventArgs args)
        {
            int index = CurrentChapter.Index;
            if (index >= Chapters.Count || Chapters.Count == 0 || _isLoading)
                return;
            var next = Chapters.Where(p => p.Index == index + 1).FirstOrDefault();
            if (next == null)
            {
                return;
            }
            if (ReaderType == Enums.ReaderType.Txt)
            {
                string content = string.Empty;
                if (next.Index == Chapters.Count - 1)
                    content = _txtContent.Substring(next.StartLength);
                else
                    content = _txtContent.Substring(next.StartLength, Chapters[next.Index + 1].StartLength - next.StartLength);
                _txtView.SetContent(content, Enums.ReaderStartMode.First);
            }
            else
            {
                var orders = _epubContent.SpecialResources.HtmlInReadingOrder;
                if (_tempEpubChapterIndex > orders.Count - 2)
                    return;
                _tempEpubChapterIndex += 1;
                var nextOrder = orders[_tempEpubChapterIndex];
                next = GetLastEpubChapter(nextOrder);
                string content = nextOrder?.TextContent ?? next.Title;
                _epubView.SetContent(content, Enums.ReaderStartMode.First);
            }
            
            if (!next.Equals(CurrentChapter))
            {
                CurrentChapter = next;
                ChapterChanged?.Invoke(this, next);
            }
        }

        public void OnProgressChanged(object sender, int addonLength)
        {
            RaiseProgressChanged(addonLength);
        }

        private void RaiseProgressChanged(int addonLength)
        {
            if (Chapters.Count == 0)
                return;

            double progress = 0;
            if (ReaderType == Enums.ReaderType.Epub)
                progress = (_tempEpubChapterIndex*1.0 / _epubContent.SpecialResources.HtmlInReadingOrder.Count * 1.0) * 100.0;
            else
                progress = ((CurrentChapter.StartLength + addonLength) * 1.0 / _txtContent.Length * 1.0) * 100;
            var history = new History(CurrentChapter, addonLength, progress);
            ProgressChanged?.Invoke(this, history);
        }

        public void OnLoad(object sender, bool e)
        {
            if (e)
                SetContentCompleted?.Invoke(this, EventArgs.Empty);
            else
                SetContentStarting?.Invoke(this, EventArgs.Empty);
        }

        public void OnTouchHolding(object sender, PositionEventArgs e)
        {
            if (!string.IsNullOrEmpty(SelectedText))
            {
                ReaderFlyout.LightDismissOverlayMode = LightDismissOverlayMode.Off;
                ReaderFlyout.ShowAt(this, new FlyoutShowOptions
                {
                    Position = e.Position,
                    ShowMode = FlyoutShowMode.Standard
                });
            }
        }
    }
}
