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
        public TxtView():base()
        {
            this.DefaultStyleKey = typeof(TxtView);
        }

        protected override void OnApplyTemplate()
        {
            _displayBlock = GetTemplateChild("TxtBlock") as RichTextBlock;
            _displayContainer = GetTemplateChild("TxtGrid") as Grid;

            base.OnApplyTemplate();
        }

        protected override async Task CreateContent()
        {
            int count = 0;
            _displayContainer.ColumnDefinitions.Clear();
            if (_displayContainer.Children.Count > 1)
            {
                for (int i = _displayContainer.Children.Count - 1; i > 0; i--)
                {
                    _displayContainer.Children.RemoveAt(i);
                }
            }
            _displayBlock.Blocks.Clear();

            double singleWidth = ParentWidth / _columns;
            double singleHeight = _displayContainer.ActualHeight;
            double actualWidth = singleWidth - ViewStyle.Padding.Left - ViewStyle.Padding.Right;
            double actualHeight = singleHeight - ViewStyle.Padding.Top - ViewStyle.Padding.Bottom;
            _displayContainer.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(singleWidth) });

            await RenderContent(_content);
            count++;

            _displayBlock.Width = actualWidth;
            _displayBlock.Height = actualHeight;
            _displayBlock.Measure(new Size(_displayContainer.ActualWidth, _displayContainer.ActualHeight));

            FrameworkElement renderTarget = _displayBlock;
            bool hasOverflow = _displayBlock.HasOverflowContent;

            while (hasOverflow)
            {
                var tmp = RenderOverflow(renderTarget);
                tmp.Width = actualWidth;
                _displayContainer.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(singleWidth) });
                count++;
                _displayContainer.Children.Add(tmp);
                Grid.SetColumn(tmp, _displayContainer.ColumnDefinitions.Count - 1);

                tmp.Height = actualHeight;
                tmp.Measure(new Size(singleWidth, singleHeight));
                renderTarget = tmp;
                hasOverflow = tmp.HasOverflowContent;
            }

            Count = Convert.ToInt32(Math.Ceiling(count / (_columns * 1.0)));
            UpdateStyle();
        }

        public override void UpdateStyle(ReaderStyle inputStyle = null)
        {
            if (inputStyle != null)
                ViewStyle = inputStyle;
            var style = ViewStyle as TxtViewStyle;
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
                    paragraph.Margin= new Thickness(0, 0, 0, ViewStyle.SegmentSpacing);
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
