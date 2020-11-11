using Richasy.Controls.Reader.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

namespace Richasy.Controls.Reader.Views
{
    [TemplatePart(Name = "TxtGrid", Type = typeof(Grid))]
    [TemplatePart(Name = "TxtBlock", Type = typeof(RichTextBlock))]
    public partial class TxtView : ReaderViewBase
    {
        public TxtView() : base()
        {
            this.DefaultStyleKey = typeof(TxtView);
        }

        protected override void OnApplyTemplate()
        {
            _displayBlock = GetTemplateChild("TxtBlock") as RichTextBlock;
            _displayContainer = GetTemplateChild("TxtGrid") as Grid;

            base.OnApplyTemplate();
        }

        public override void UpdateStyle(ReaderStyle inputStyle = null)
        {
            if (inputStyle != null)
                ViewStyle = inputStyle;
            var style = ViewStyle as TxtViewStyle;
            if (style == null)
                style = new TxtViewStyle();
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
                }
                else if (item is RichTextBlockOverflow of)
                {
                    of.Margin = style.Padding;
                }
            }
        }

        protected override async Task RenderContent(string content)
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
    }
}
