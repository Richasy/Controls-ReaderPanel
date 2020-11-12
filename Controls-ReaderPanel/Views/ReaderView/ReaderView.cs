using Richasy.Controls.Reader.Enums;
using Richasy.Controls.Reader.Models;
using Richasy.Controls.Reader.Models.Epub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

namespace Richasy.Controls.Reader.Views
{
    public partial class ReaderView : ReaderViewBase
    {
        public ReaderView() : base()
        {

        }
        public ReaderView(ReaderType type) : base()
        {
            ReaderType = type;
        }

        public override void UpdateStyle(ReaderStyle style = null)
        {
            if (ReaderType == ReaderType.Epub)
                UpdateEpubStyle(style);
            else
                UpdateTxtStyle(style);
        }

        protected override async Task RenderContent(string content)
        {
            if (ReaderType == ReaderType.Epub)
                await RenderEpubContent(content);
            else
                RenderTxtContent(content);
        }

        private void UpdateTxtStyle(ReaderStyle inputStyle = null)
        {
            if (inputStyle != null)
                ViewStyle = inputStyle;
            var style = ViewStyle;
            if (style.IsAcrylicBackground)
            {
                var opacity = Convert.ToInt32(style.Background.A) / 255.0;
                var tempBackground = style.Background;
                tempBackground.A = 255;
                var acrylic = new AcrylicBrush()
                {
                    TintColor = tempBackground,
                    TintOpacity = opacity,
                    FallbackColor = style.Background,
                    BackgroundSource = AcrylicBackgroundSource.HostBackdrop
                };
                Background = acrylic;
            }
            else
                Background = new SolidColorBrush(style.Background);
            foreach (var item in _displayContainer.Children)
            {
                if (item is RichTextBlock rtb)
                {
                    rtb.FontFamily = new FontFamily(style.FontFamily);
                    rtb.FontSize = style.FontSize;
                    rtb.Foreground = new SolidColorBrush(style.Foreground);
                    rtb.LineHeight = style.LineHeight;
                    rtb.Margin = style.Padding;
                    var title = rtb.Blocks.FirstOrDefault();
                    if (title != null)
                    {
                        title.FontSize = style.HeaderFontSize;
                        title.Margin = style.HeaderMargin;
                    }
                    rtb.Blocks.OfType<Paragraph>().Skip(1).ToList().ForEach(p => { p.Margin = new Thickness(0, 0, 0, style.SegmentSpacing); p.TextIndent = style.TextIndent * style.FontSize; });
                }
                else if (item is RichTextBlockOverflow of)
                {
                    of.Margin = style.Padding;
                }
            }
        }

        private void RenderTxtContent(string content)
        {
            if (string.IsNullOrEmpty(content)) return;
            var firstLine = content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).First();
            var title = new Paragraph();
            title.Inlines.Add(new Run()
            {
                Text = firstLine.Trim(),
                FontWeight = FontWeights.Bold,
            });
            var paragraphs = content.Split(Environment.NewLine).Select(x =>
            {
                if (x.Trim() != firstLine.Trim())
                {
                    var run = new Run() { Text = x.Trim() };
                    var paragraph = new Paragraph();
                    paragraph.Inlines.Add(run);
                    paragraph.TextIndent = ViewStyle.FontSize * ViewStyle.TextIndent;
                    paragraph.Margin = new Thickness(0, 0, 0, ViewStyle.SegmentSpacing);
                    return paragraph;
                }
                return null;
            });
            _displayBlock.Blocks.Add(title);
            foreach (var paragraph in paragraphs)
            {
                if (paragraph != null)
                    _displayBlock.Blocks.Add(paragraph);
            }
        }

        public void EpubInit(EpubBook book, ReaderStyle style)
        {
            Book = book;
            ViewStyle = style;
            helper = new HtmlHelper(book.Resources.Images.ToList(), style);
            helper.LinkTapped += (_s, _e) => { LinkTapped?.Invoke(_s, _e); };
            helper.ImageTapped += (_s, _e) => { ImageTapped?.Invoke(_s, _e); };
        }

        private void UpdateEpubStyle(ReaderStyle inputStyle = null)
        {
            if (inputStyle != null)
                ViewStyle = inputStyle;
            var style = ViewStyle;
            if (style.IsAcrylicBackground)
            {
                var opacity = Convert.ToInt32(style.Background.A) / 255.0;
                var tempBackground = style.Background;
                tempBackground.A = 255;
                var acrylic = new AcrylicBrush()
                {
                    TintColor = tempBackground,
                    TintOpacity = opacity,
                    FallbackColor = style.Background,
                    BackgroundSource = AcrylicBackgroundSource.HostBackdrop
                };
                Background = acrylic;
            }
            else
                Background = new SolidColorBrush(style.Background);
            foreach (var item in _displayContainer.Children)
            {
                if (item is RichTextBlock rtb)
                {
                    rtb.FontFamily = new FontFamily(style.FontFamily);
                    rtb.FontSize = style.FontSize;
                    rtb.Foreground = new SolidColorBrush(style.Foreground);
                    rtb.LineHeight = style.LineHeight;
                    rtb.Margin = style.Padding;
                    rtb.Blocks.OfType<Paragraph>().Skip(1).ToList().ForEach(p => { p.Margin = new Thickness(0, 0, 0, style.SegmentSpacing); p.TextIndent = style.TextIndent * style.FontSize; });
                }
                else if (item is RichTextBlockOverflow of)
                {
                    of.Margin = style.Padding;
                }
            }
        }

        private async Task RenderEpubContent(string content)
        {
            if (string.IsNullOrEmpty(content)) return;
            await helper.InitAsync(content);
            if (helper.RenderBlocks.Count > 0)
            {
                foreach (var item in helper.RenderBlocks)
                {
                    try
                    {
                        if (item is Paragraph p)
                        {
                            if (p.Inlines.Count == 0)
                                continue;
                        }
                        _displayBlock.Blocks.Add(item);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
        }
    }
}
