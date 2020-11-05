using Richasy.Controls.Reader.Enums;
using Richasy.Controls.Reader.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Composition.Interactions;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Richasy.Controls.Reader.Views
{
    public partial class ReaderViewBase : Control, IInteractionTrackerOwner
    {
        public ReaderViewBase()
        {
            this.DefaultStyleKey = typeof(ReaderViewBase);

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
            this.SizeChanged += ReaderViewBase_SizeChanged;

            CreateContentDelayer = new EventDelayer();
            _tempOverflowList = new List<Tuple<bool, FrameworkElement>>();
            CreateContentDelayer.ResetWhenDelayed = true;
            CreateContentDelayer.Arrived += CreateContentWaiter_Arrived;
        }

        protected override void OnApplyTemplate()
        {
            FlyoutInit();

            SetupComposition();
            SetupTracker();

            base.OnApplyTemplate();
        }

        public async void SetContent(string Content, ReaderStartMode mode = ReaderStartMode.First, int startLength = 0)
        {
            _isSizeChangeLoaded = false;
            _content = Content;
            LoadingStatusChanged?.Invoke(this, LoadingStatus.Loading);
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
                int childrenCount = _tempOverflowList.Count;
                var signNumber = Content.Length / childrenCount;
                index = Convert.ToInt32(Math.Floor(startLength / (signNumber * 1.0)));
                index = Convert.ToInt32(Math.Round(index / (_columns * 1.0)));
                index = index > Count - 1 ? Count - 1 : index;
            }
            Index = index;
            GoToIndex(Index, false);
            IsCoreSelectedChanged = false;
            LoadingStatusChanged?.Invoke(this, LoadingStatus.Completed);
        }

        protected virtual async Task CreateContent() { }

        public virtual void UpdateStyle(ReaderStyle style = null) { }

        protected virtual async Task RenderContent(string content) { }

        protected virtual RichTextBlockOverflow RenderOverflow(FrameworkElement target)
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
