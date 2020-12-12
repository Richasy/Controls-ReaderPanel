using Richasy.Controls.Reader.Enums;
using Richasy.Controls.Reader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public event EventHandler ViewLoaded;
        public event EventHandler<CustomRequestEventArgs> CustomContentRequest;
        public event EventHandler<SpeechCueEventArgs> SpeechCueChanged;

        public async void OnPrevPageSelected(object sender, EventArgs args)
        {
            if (CurrentChapter == null)
                return;
            int index = CurrentChapter.Index;
            if (index < 1 || Chapters.Count == 0 || _isLoading)
                return;
            var prev = Chapters.Where(p => p.Index == index - 1).FirstOrDefault();
            if (prev == null)
            {
                return;
            }
            if (ReaderType == ReaderType.Txt)
            {
                string content = _txtContent.Substring(prev.StartLength, CurrentChapter.StartLength - prev.StartLength);
                await _readerView.SetContent(content, ReaderStartMode.Last);
            }
            else if (ReaderType == ReaderType.Custom)
            {
                var detail = CustomChapterDetailList.Where(p => p.Index == prev.Index).FirstOrDefault();
                if (detail != null)
                    await _readerView.SetContent(detail.GetReadContent(), ReaderStartMode.Last);
                else
                    CustomContentRequest?.Invoke(this, new CustomRequestEventArgs(ReaderStartMode.Last, prev));
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
                await _readerView.SetContent(content, ReaderStartMode.Last);
            }

            if (!prev.Equals(CurrentChapter))
            {
                CurrentChapter = prev;
                ChapterChanged?.Invoke(this, prev);
            }
        }

        public async void OnNextPageSelected(object sender, EventArgs args)
        {
            if (CurrentChapter == null)
                return;
            int index = CurrentChapter.Index;
            if (index >= Chapters.Count || Chapters.Count == 0 || _isLoading)
                return;
            var next = Chapters.Where(p => p.Index == index + 1).FirstOrDefault();
            if (next == null)
            {
                return;
            }
            if (ReaderType == ReaderType.Txt)
            {
                string content = string.Empty;
                if (next.Index == Chapters.Count - 1)
                    content = _txtContent.Substring(next.StartLength);
                else
                    content = _txtContent.Substring(next.StartLength, Chapters[next.Index + 1].StartLength - next.StartLength);
                await _readerView.SetContent(content, ReaderStartMode.First);
            }
            else if (ReaderType == ReaderType.Custom)
            {
                var detail = CustomChapterDetailList.Where(p => p.Index == next.Index).FirstOrDefault();
                if (detail != null)
                    await _readerView.SetContent(detail.GetReadContent(), ReaderStartMode.First);
                else
                    CustomContentRequest?.Invoke(this, new CustomRequestEventArgs(ReaderStartMode.First, next));
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
                await _readerView.SetContent(content, ReaderStartMode.First);
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
            if (Chapters.Count == 0 || addonLength < 0)
                return;

            double progress = 0;
            if (ReaderType == ReaderType.Epub)
                progress = (_tempEpubChapterIndex * 1.0 / _epubContent.SpecialResources.HtmlInReadingOrder.Count * 1.0) * 100.0;
            else if (ReaderType == ReaderType.Custom)
                progress = CurrentChapter.Index * 1.0 / Chapters.Count * 100.0;
            else
                progress = ((CurrentChapter.StartLength + addonLength) * 1.0 / _txtContent.Length * 1.0) * 100;
            if (progress > 100)
                progress = 100;
            else if (progress < 0)
                progress = 0;
            var history = new History(CurrentChapter, addonLength, progress);
            ProgressChanged?.Invoke(this, history);
        }

        public void OnLoad(object sender, LoadingStatus e)
        {
            if (e == LoadingStatus.Completed)
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
