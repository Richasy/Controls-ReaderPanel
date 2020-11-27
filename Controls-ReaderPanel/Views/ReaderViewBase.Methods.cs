using Richasy.Controls.Reader.Models;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Composition.Interactions;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;

namespace Richasy.Controls.Reader.Views
{
    public partial class ReaderViewBase
    {
        #region Private Method

        private void SetupComposition()
        {
            compositor = Window.Current.Compositor;

            OffsetAnimation = compositor.CreateVector3KeyFrameAnimation();
            OffsetAnimation.Duration = TimeSpan.FromSeconds(0.35d);
            OffsetAnimation.Target = "Offset";
            OffsetAnimation.StopBehavior = AnimationStopBehavior.LeaveCurrentValue;

            PanelVisual = ElementCompositionPreview.GetElementVisual(_displayContainer);
            ReaderViewVisual = ElementCompositionPreview.GetElementVisual(this);
        }

        private void SetupTracker()
        {
            _tracker = InteractionTracker.CreateWithOwner(compositor, this);
            InitTrackerPositions();

            _source = VisualInteractionSource.Create(ReaderViewVisual);
            _source.ManipulationRedirectionMode = VisualInteractionSourceRedirectionMode.CapableTouchpadOnly;

            _source.IsPositionXRailsEnabled = false;
            _source.PositionXChainingMode = InteractionChainingMode.Never;
            _source.PositionXSourceMode = InteractionSourceMode.EnabledWithoutInertia;

            _source.IsPositionYRailsEnabled = false;
            _source.PositionYChainingMode = InteractionChainingMode.Never;
            _source.PositionYSourceMode = InteractionSourceMode.Disabled;

            _tracker.InteractionSources.Add(_source);

            OffsetBind = compositor.CreateExpressionAnimation("-tracker.Position");
            OffsetBind.SetReferenceParameter("tracker", _tracker);

            PanelVisual.StartAnimation("Offset", OffsetBind);
        }

        private void InitTrackerPositions()
        {
            if (_tracker != null)
            {
                _tracker.MaxPosition = new Vector3((float)((Count + 1) * this.ActualWidth), 0f, 0f);
                _tracker.MinPosition = new Vector3(-(float)(this.ActualWidth), 0f, 0f);
            }
        }

        public async void GoToIndex(int index, bool UseAnimation = true)
        {
            if (index < 0) return;
            var temp = _displayBlock.GetPositionFromPoint(new Point(0, 0));
            _displayBlock.Select(temp, temp);
            double moveOffset = _displayContainer.ActualWidth / _displayContainer.ColumnDefinitions.Count * _columns;
            if (UseAnimation)
            {
                OffsetAnimation.InsertKeyFrame(1f, new Vector3((float)(moveOffset * index), 0f, 0f));
                _tracker.TryUpdatePositionWithAnimation(OffsetAnimation);
                UpdateContentLayout();
            }
            else
            {
                UpdateContentLayout();
                await Task.Delay(100);
                _tracker.TryUpdatePosition(new Vector3((float)(ParentWidth * index)), 0f, 0f);
            }
        }

        private void FlyoutInit()
        {
            _displayBlock.ContextFlyout = ReaderFlyout;
            _displayBlock.SelectionFlyout = null;
        }

        /// <summary>
        /// 重新填充显示的内容
        /// </summary>
        /// <param name="index"></param>
        internal void UpdateContentLayout()
        {
            int startIndex = Index * _columns;
            if (startIndex > _tempOverflowList.Count)
                NextPageSelected?.Invoke(this, EventArgs.Empty);
            else
            {
                for (int i = 1; i <= startIndex + _columns; i++)
                {
                    if (i < 0)
                        continue;
                    else if (i > _tempOverflowList.Count - 1)
                        break;
                    try
                    {
                        if (!_tempOverflowList[i].IsRendered)
                        {
                            _displayContainer.Children.Add(_tempOverflowList[i].Element);
                            Grid.SetColumn(_tempOverflowList[i].Element, i);
                            _tempOverflowList[i].IsRendered = true;
                        }
                    }
                    catch (Exception)
                    { }
                }
            }
        }

        public void RenderAllOverflows()
        {
            for (int i = 0; i < _tempOverflowList.Count; i++)
            {
                try
                {
                    if (!_tempOverflowList[i].IsRendered)
                    {
                        _displayContainer.Children.Add(_tempOverflowList[i].Element);
                        Grid.SetColumn(_tempOverflowList[i].Element, i);
                        _tempOverflowList[i].IsRendered = true;
                    }
                }
                catch (Exception)
                { }
            }
        }
        #endregion Private Method

        #region Manipulation Events

        private void _gestureRecognizer_ManipulationStarted(GestureRecognizer sender, ManipulationStartedEventArgs args)
        {
            _startX = Index * this.ActualWidth;
        }

        private void _gestureRecognizer_ManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {
            if (Math.Abs(args.Cumulative.Translation.X) < this.ActualWidth)
            {
                _tracker.TryUpdatePosition(new Vector3((float)(_startX - args.Cumulative.Translation.X), 0f, 0f));
            }
        }

        private void _gestureRecognizer_ManipulationCompleted(GestureRecognizer sender, ManipulationCompletedEventArgs args)
        {
            IsCoreSelectedChanged = true;
            if (args.Cumulative.Translation.X > 150 || args.Velocities.Linear.X > 0.5)
            {
                Index--;
                if (Index < 0) Index = 0;
            }
            else if (args.Cumulative.Translation.X < -150 || args.Velocities.Linear.X < -0.5)
            {
                Index++;
                if (Index > Count - 1) Index = Count - 1;
            }
            GoToIndex(Index);
            IsCoreSelectedChanged = false;
        }

        #endregion Manipulation Events

        #region Pointer Events

        public void Previous()
        {
            IsCoreSelectedChanged = true;
            Index--;
            if (Index < 0) Index = 0;
            GoToIndex(Index);
            IsCoreSelectedChanged = false;
        }

        public void Next()
        {
            IsCoreSelectedChanged = true;
            Index++;
            if (Index > Count - 1) Index = Count - 1;
            GoToIndex(Index);
            IsCoreSelectedChanged = false;
        }

        private void _PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            IsCoreSelectedChanged = true;
            if (e.GetCurrentPoint(this).Properties.MouseWheelDelta > 0)
            {
                Index--;
                if (Index < 0) Index = 0;
            }
            else
            {
                Index++;
                if (Index > Count - 1) Index = Count - 1;
            }
            GoToIndex(Index);
            IsCoreSelectedChanged = false;
        }

        private void _PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_source != null)
            {
                var pointer = e.GetCurrentPoint(this);
                if (pointer.Properties.IsLeftButtonPressed)
                {
                    foreach (var item in _displayContainer.Children)
                    {
                        if (item is RichTextBlock rtb)
                        {
                            if (!string.IsNullOrEmpty(rtb.SelectedText))
                            {
                                var temp = rtb.GetPositionFromPoint(new Point(0, 0));
                                rtb.Select(temp, temp);
                            }
                        }
                    }
                }

                if (pointer.PointerDevice.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen || pointer.PointerDevice.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
                {
                    try
                    {
                        _source.TryRedirectForManipulation(pointer);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
                else
                {

                    this.CapturePointer(e.Pointer);
                    _gestureRecognizer.ProcessDownEvent(pointer);
                }
            }
        }

        private void _PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.GetCurrentPoint(this);
            if (pointer.PointerDevice.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                if (pointer.Properties.IsLeftButtonPressed)
                {
                    foreach (var item in _displayContainer.Children)
                    {
                        if (item is RichTextBlock rtb)
                        {
                            if (!string.IsNullOrEmpty(rtb.SelectedText))
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }


        private void _PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.GetCurrentPoint(this);
            if (pointer.PointerDevice.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                _gestureRecognizer.CompleteGesture();
                this.ReleasePointerCapture(e.Pointer);
            }
        }

        private void _PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.GetCurrentPoint(this);
            if (pointer.PointerDevice.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                _gestureRecognizer.ProcessUpEvent(pointer);
                this.ReleasePointerCapture(e.Pointer);
            }
        }

        #endregion Pointer Events

        #region Interaction Tracker Events
        public void CustomAnimationStateEntered(InteractionTracker sender, InteractionTrackerCustomAnimationStateEnteredArgs args)
        {
            IsAnimating = true;
        }

        public void IdleStateEntered(InteractionTracker sender, InteractionTrackerIdleStateEnteredArgs args)
        {
            if (IsAnimating)
            {
                IsAnimating = false;
            }
            else
            {
                IsCoreSelectedChanged = true;
                var delta = sender.Position.X - Index * this.ActualWidth;
                if (delta < -40)
                {
                    Index--;
                    if (Index < 0)
                    {
                        Index = 0;
                        OnPrevPageSelected();
                    }
                }
                else if (delta > 40)
                {
                    Index++;
                    if (Index > Count - 1)
                    {
                        Index = Count - 1;
                        OnNextPageSelected();
                    }
                }
                GoToIndex(Index);
                IsCoreSelectedChanged = false;
            }
        }

        public void InertiaStateEntered(InteractionTracker sender, InteractionTrackerInertiaStateEnteredArgs args)
        {
            IsAnimating = false;

        }

        public void InteractingStateEntered(InteractionTracker sender, InteractionTrackerInteractingStateEnteredArgs args)
        {
            IsAnimating = false;

        }

        public void RequestIgnored(InteractionTracker sender, InteractionTrackerRequestIgnoredArgs args)
        {

        }

        public void ValuesChanged(InteractionTracker sender, InteractionTrackerValuesChangedArgs args)
        {

        }
        #endregion Interaction Tracker Events

        #region Touch Events
        private void _TouchHolding(object sender, HoldingRoutedEventArgs e)
        {
            e.Handled = true;

            if (e.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                var position = e.GetPosition(this);
                TouchHolding?.Invoke(this, new PositionEventArgs(position, e.PointerDeviceType));
            }
        }

        private void _TouchTapped(object sender, TappedRoutedEventArgs e)
        {
            if (e.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                e.Handled = true;
            }
            else
            {
                var temp = _displayBlock.GetPositionFromPoint(new Point(0, 0));
                _displayBlock.Select(temp, temp);
            }
            var position = e.GetPosition(this);
            TouchTapped?.Invoke(this, new PositionEventArgs(position, e.PointerDeviceType));
        }
        #endregion
    }
}
