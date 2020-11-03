using Richasy.Controls.Reader.Enums;
using Richasy.Controls.Reader.Models;
using Richasy.Controls.Reader.Models.Epub;
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
    public partial class EpubView : Control, IInteractionTrackerOwner
    {
        public EpubView()
        {
            this.DefaultStyleKey = typeof(EpubView);

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
            this.SizeChanged += EpubView_SizeChanged;


            IndexWaiter = new EventWaiter();
            CreateContentDelayer = new EventDelayer();
            CreateContentDelayer.ResetWhenDelayed = true;
            CreateContentDelayer.Arrived += CreateContentWaiter_Arrived;
        }
        
        public void Init(EpubBook book, EpubViewStyle style)
        {
            Book = book;
            helper = new HtmlHelper(book.Resources.Images.ToList(), style);
            helper.LinkTapped += (_s, _e) => { LinkTapped?.Invoke(_s, _e); };
            helper.ImageTapped += (_s, _e) => { ImageTapped?.Invoke(_s, _e); };
        }
        protected override void OnApplyTemplate()
        {
            _epubBlock = GetTemplateChild("EpubBlock") as RichTextBlock;
            _epubGrid = GetTemplateChild("EpubGrid") as Grid;

            FlyoutInit();

            SetupComposition();
            SetupTracker();

            base.OnApplyTemplate();
        }

        public async void SetContent(string Content, ReaderStartMode mode = ReaderStartMode.First, int startLength = 0)
        {
            _isSizeChangeLoaded = false;
            _content = Content;
            LoadingChanged?.Invoke(this, true);
            await CreateContent();

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
                int childrenCount = _epubGrid.Children.Count;
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

        private async Task CreateContent()
        {
            int count = 0;
            _epubGrid.ColumnDefinitions.Clear();
            if (_epubGrid.Children.Count > 1)
            {
                for (int i = _epubGrid.Children.Count - 1; i > 0; i--)
                {
                    _epubGrid.Children.RemoveAt(i);
                }
            }
            _epubBlock.Blocks.Clear();

            double singleWidth = ParentWidth / _columns;
            double singleHeight = _epubGrid.ActualHeight;
            double actualWidth = singleWidth - ViewStyle.Padding.Left - ViewStyle.Padding.Right;
            double actualHeight = singleHeight - ViewStyle.Padding.Top - ViewStyle.Padding.Bottom;
            _epubGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(singleWidth) });

            await RenderContentAsync(_content);
            count++;

            _epubBlock.Width = actualWidth;
            _epubBlock.Height = actualHeight;
            _epubBlock.Measure(new Size(_epubGrid.ActualWidth, _epubGrid.ActualHeight));

            FrameworkElement renderTarget = _epubBlock;
            bool hasOverflow = _epubBlock.HasOverflowContent;

            while (hasOverflow)
            {
                var tmp = RenderOverflow(renderTarget);
                tmp.Width = actualWidth;
                _epubGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(singleWidth) });
                count++;
                _epubGrid.Children.Add(tmp);
                Grid.SetColumn(tmp, _epubGrid.ColumnDefinitions.Count - 1);

                tmp.Height = actualHeight;
                tmp.Measure(new Size(singleWidth, singleHeight));
                renderTarget = tmp;
                hasOverflow = tmp.HasOverflowContent;
            }

            Count = Convert.ToInt32(Math.Ceiling(count / (_columns * 1.0)));
            UpdateStyle();
        }

        public void UpdateStyle(EpubViewStyle style = null)
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
            foreach (var item in _epubGrid.Children)
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

        private async Task RenderContentAsync(string content)
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
                        _epubBlock.Blocks.Add(item);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
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
