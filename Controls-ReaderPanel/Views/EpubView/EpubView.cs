using Richasy.Controls.Reader.Enums;
using Richasy.Controls.Reader.Models;
using Richasy.Controls.Reader.Models.Epub;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Composition.Interactions;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Richasy.Controls.Reader.Views
{
    [TemplatePart(Name = "EpubGrid", Type = typeof(Grid))]
    [TemplatePart(Name = "EpubBlock", Type = typeof(RichTextBlock))]
    public partial class EpubView : ReaderViewBase
    {
        public EpubView() : base()
        {
            this.DefaultStyleKey = typeof(EpubView);
        }

        public void Init(EpubBook book, EpubViewStyle style)
        {
            Book = book;
            ViewStyle = style;
            helper = new HtmlHelper(book.Resources.Images.ToList(), style);
            helper.LinkTapped += (_s, _e) => { LinkTapped?.Invoke(_s, _e); };
            helper.ImageTapped += (_s, _e) => { ImageTapped?.Invoke(_s, _e); };
        }
        protected override void OnApplyTemplate()
        {
            _displayBlock = GetTemplateChild("EpubBlock") as RichTextBlock;
            _displayContainer = GetTemplateChild("EpubGrid") as Grid;

            base.OnApplyTemplate();
        }


        public override void UpdateStyle(ReaderStyle inputStyle = null)
        {
            if (inputStyle != null)
                ViewStyle = inputStyle;
            var style = ViewStyle as EpubViewStyle;
            if (style == null)
                style = new EpubViewStyle();
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
