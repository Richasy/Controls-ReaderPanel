using Richasy.Controls.Reader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Richasy.Controls.Reader
{
    public partial class ReaderPanel
    {
        public event EventHandler<Chapter> ChapterChanged;
        public event EventHandler<History> ProgressChanged;
        public event EventHandler<bool> LoadingChanged;
        public event EventHandler<PositionEventArgs> TouchHolding;
        public event EventHandler<PositionEventArgs> TouchTapped;

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
                var prevOrder = _epubContent.SpecialResources.HtmlInReadingOrder[prev.Index];
                string content = prevOrder.TextContent;
                _epubView.SetContent(content, Enums.ReaderStartMode.Last);
            }
            CurrentChapter = prev;
            ChapterChanged?.Invoke(this, prev);
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
                _txtView.SetContent(content, Enums.ReaderStartMode.Last);
            }
            else
            {
                var nextOrder = _epubContent.SpecialResources.HtmlInReadingOrder[next.Index];
                string content = nextOrder.TextContent;
                _epubView.SetContent(content, Enums.ReaderStartMode.Last);
            }
            CurrentChapter = next;
            ChapterChanged?.Invoke(this, next);
        }

        public void OnProgressChanged(object sender, int addonLength)
        {
            if (Chapters.Count == 0)
                return;

            double progress = 0;
            if (CurrentChapter.Index == Chapters.Count - 1)
                progress = 100.0;
            else
            {
                if (ReaderType == Enums.ReaderType.Epub)
                    progress = (CurrentChapter.Index / Chapters.Count * 1.0) * 100.0;
                else
                    progress = ((CurrentChapter.StartLength + addonLength) / _txtContent.Length * 1.0) * 100;
            }
            var history = new History(CurrentChapter, addonLength, progress);
            ProgressChanged?.Invoke(this, history);
        }
    }
}
