using Richasy.Controls.Reader.Enums;
using Richasy.Controls.Reader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Composition.Interactions;
using Windows.UI.Input;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Richasy.Controls.Reader.Views
{
    [TemplatePart(Name = "TxtGrid", Type = typeof(Grid))]
    [TemplatePart(Name = "TxtBlock", Type = typeof(RichTextBlock))]
    public partial class TxtView : Control, IInteractionTrackerOwner
    {
        public TxtView()
        {
            this.DefaultStyleKey = typeof(TxtView);

            _gestureRecognizer = new GestureRecognizer();
            _gestureRecognizer.GestureSettings = GestureSettings.ManipulationTranslateX;
            _gestureRecognizer.ManipulationStarted += _gestureRecognizer_ManipulationStarted;
            _gestureRecognizer.ManipulationUpdated += _gestureRecognizer_ManipulationUpdated;
            _gestureRecognizer.ManipulationCompleted += _gestureRecognizer_ManipulationCompleted;

            PointerWheelChangedEventHandler = new PointerEventHandler(_PointerWheelChanged);
            PointerPressedEventHandler = new PointerEventHandler(_PointerPressed);
            PointerMovedEventHandler = new PointerEventHandler(_PointerMoved);
            PointerReleasedEventHandler = new PointerEventHandler(_PointerReleased);
            PointerCanceledEventHandler = new PointerEventHandler(_PointerCanceled);
            TouchTappedEventHandler = new TappedEventHandler(_TouchTapped);
            TouchHoldingEventHandler = new HoldingEventHandler(_TouchHolding);

            this.AddHandler(UIElement.PointerWheelChangedEvent, PointerWheelChangedEventHandler, true);
            this.AddHandler(UIElement.PointerPressedEvent, PointerPressedEventHandler, true);
            this.AddHandler(UIElement.PointerMovedEvent, PointerMovedEventHandler, true);
            this.AddHandler(UIElement.PointerReleasedEvent, PointerReleasedEventHandler, true);
            this.AddHandler(UIElement.PointerCanceledEvent, PointerCanceledEventHandler, true);
            this.AddHandler(UIElement.TappedEvent, TouchTappedEventHandler, true);
            this.AddHandler(UIElement.HoldingEvent, TouchHoldingEventHandler, true);
            this.SizeChanged += TxtView_SizeChanged;


            IndexWaiter = new EventWaiter();
            CreateContentDelayer = new EventDelayer();
            CreateContentDelayer.ResetWhenDelayed = true;
            CreateContentDelayer.Arrived += CreateContentWaiter_Arrived;
        }

        protected override void OnApplyTemplate()
        {
            _txtBlock = GetTemplateChild("TxtBlock") as RichTextBlock;
            _txtGrid = GetTemplateChild("TxtGrid") as Grid;

            FlyoutInit();

            SetupComposition();
            SetupTracker();

            base.OnApplyTemplate();
        }

        public void SetContent(string Content, ReaderStartMode mode = ReaderStartMode.First, int startLength = 0)
        {
            _isSizeChangeLoaded = false;
            _content = Content;
            LoadingChanged?.Invoke(this, true);
            CreateContent();

            _startTextIndex = startLength;
            IsCoreSelectedChanged = true;
            var index = 0;
            switch (mode)
            {
                case ReaderStartMode.First:
                    index = 0;
                    break;
                case ReaderStartMode.Last:
                    index = Count - 1;
                    break;
                case ReaderStartMode.Stay:
                    index = Index > Count - 1 ? Count - 1 : Index;
                    break;
            }
            if (startLength != 0)
            {
                int childrenCount = _txtGrid.Children.Count;
                var signNumber = Content.Length / childrenCount;
                index = Convert.ToInt32(Math.Floor(startLength / (signNumber * 1.0)));
                index = Convert.ToInt32(Math.Round(index / (_columns * 1.0)));
                index = index > Count - 1 ? Count - 1 : index;
            }
            Index = index;
            GoToIndex(Index, false);
            IsCoreSelectedChanged = false;
            LoadingChanged?.Invoke(this, false);
        }

        private void CreateContent()
        {
            int count = 0;
            _txtGrid.ColumnDefinitions.Clear();
            if (_txtGrid.Children.Count > 1)
            {
                for (int i = _txtGrid.Children.Count - 1; i > 0; i--)
                {
                    _txtGrid.Children.RemoveAt(i);
                }
            }
            _txtBlock.Blocks.Clear();

            double singleWidth = ParentWidth / _columns;
            double singleHeight = _txtGrid.ActualHeight;
            double actualWidth = singleWidth - ViewStyle.Padding.Left - ViewStyle.Padding.Right;
            double actualHeight = singleHeight - ViewStyle.Padding.Top - ViewStyle.Padding.Bottom;
            _txtGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(singleWidth) });

            RenderContent(_content);
            count++;

            _txtBlock.Width = actualWidth;
            _txtBlock.Height = actualHeight;
            _txtBlock.Measure(new Size(_txtGrid.ActualWidth, _txtGrid.ActualHeight));

            FrameworkElement renderTarget = _txtBlock;
            bool hasOverflow = _txtBlock.HasOverflowContent;

            while (hasOverflow)
            {
                var tmp = RenderOverflow(renderTarget);
                tmp.Width = actualWidth;
                _txtGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(singleWidth) });
                count++;
                _txtGrid.Children.Add(tmp);
                Grid.SetColumn(tmp, _txtGrid.ColumnDefinitions.Count - 1);

                tmp.Height = actualHeight;
                tmp.Measure(new Size(singleWidth, singleHeight));
                renderTarget = tmp;
                hasOverflow = tmp.HasOverflowContent;
            }

            Count = Convert.ToInt32(Math.Ceiling(count / (_columns * 1.0)));
            UpdateStyle();
        }

        public void UpdateStyle(TxtViewStyle style = null)
        {
            if (style != null)
                ViewStyle = style;
            else
                style = ViewStyle;
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
            foreach (var item in _txtGrid.Children)
            {
                if (item is RichTextBlock rtb)
                {
                    rtb.FontFamily = new FontFamily(style.FontFamily);
                    rtb.FontSize = style.FontSize;
                    rtb.Foreground = new SolidColorBrush(style.Foreground);
                    rtb.LineHeight = style.LineHeight;
                    rtb.Margin = style.Padding;
                    var title = rtb.Blocks.First();
                    title.FontSize = style.HeaderFontSize;
                }
                else if (item is RichTextBlockOverflow of)
                {
                    of.Margin = style.Padding;
                }
            }
        }

        private void RenderContent(string content)
        {
            if (string.IsNullOrEmpty(content)) return;
            var firstLine = content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).First();
            var title = new Paragraph();
            title.Inlines.Add(new Run()
            {
                Text = firstLine.Trim(),
                FontWeight = FontWeights.Bold
            });
            var paragraphs = content.Replace("\r", string.Empty).Split('\n').Select(x =>
            {
                if (x.Trim() != firstLine.Trim())
                {
                    var run = new Run() { Text = x.Trim() };
                    var paragraph = new Paragraph();
                    paragraph.Inlines.Add(run);
                    paragraph.TextIndent = ViewStyle.FontSize * ViewStyle.TextIndent;
                    return paragraph;
                }
                return null;
            });
            _txtBlock.Blocks.Add(title);
            foreach (var paragraph in paragraphs)
            {
                if (paragraph != null)
                    _txtBlock.Blocks.Add(paragraph);
            }
        }

        private RichTextBlockOverflow RenderOverflow(FrameworkElement target)
        {
            var tmp = new RichTextBlockOverflow();
            if (target is RichTextBlock richBlock)
                richBlock.OverflowContentTarget = tmp;
            else if (target is RichTextBlockOverflow of)
                of.OverflowContentTarget = tmp;

            tmp.Padding = Padding;
            return tmp;
        }
    }
}
