using Richasy.Controls.Reader.Models;
using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Composition.Interactions;
using Windows.UI.Input;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;

namespace Richasy.Controls.Reader.Views
{
    public partial class TxtView
    {
        #region Private Method

        private void SetupComposition()
        {
            compositor = Window.Current.Compositor;

            OffsetAnimation = compositor.CreateVector3KeyFrameAnimation();
            OffsetAnimation.Duration = TimeSpan.FromSeconds(0.35d);
            OffsetAnimation.Target = "Offset";
            OffsetAnimation.StopBehavior = AnimationStopBehavior.LeaveCurrentValue;

            PanelVisual = ElementCompositionPreview.GetElementVisual(_txtGrid);
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

        private void GoToIndex(int index, bool UseAnimation = true)
        {
            if (index < 0) return;
            var temp = _txtBlock.GetPositionFromPoint(new Point(0, 0));
            _txtBlock.Select(temp,temp);
            if (UseAnimation)
            {
                OffsetAnimation.InsertKeyFrame(1f, new Vector3((float)(this.ActualWidth * index), 0f, 0f));
                _tracker.TryUpdatePositionWithAnimation(OffsetAnimation);
            }
            else
            {
                _tracker.TryUpdatePosition(new Vector3((float)(this.ActualWidth * index), 0f, 0f));
            }

        }

        private void FlyoutInit()
        {
            _txtBlock.ContextFlyout = ReadFlyout;
            _txtBlock.SelectionFlyout = null;
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
            if (IndexWaiter.IsEnabled)
            {
                IsCoreSelectedChanged = true;
                Index--;
                if (Index < 0) Index = 0;
                GoToIndex(Index);
                IsCoreSelectedChanged = false;
            }
        }

        public void Next()
        {
            if (IndexWaiter.IsEnabled)
            {
                IsCoreSelectedChanged = true;
                Index++;
                if (Index > Count - 1) Index = Count - 1;
                GoToIndex(Index);
                IsCoreSelectedChanged = false;
            }
        }

        private void _PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (IndexWaiter.IsEnabled)
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
        }

        private void _PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (IndexWaiter.IsEnabled)
            {
                if (_source != null)
                {
                    var pointer = e.GetCurrentPoint(this);
                    if (pointer.Properties.IsLeftButtonPressed)
                    {
                        foreach (var item in _txtGrid.Children)
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
                            System.Diagnostics.Debug.WriteLine(ex);
                        }
                    }
                    else
                    {

                        this.CapturePointer(e.Pointer);
                        _gestureRecognizer.ProcessDownEvent(pointer);
                    }
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
                    foreach (var item in _txtGrid.Children)
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

                //var pointers = e.GetIntermediatePoints(this);
                //_gestureRecognizer.ProcessMoveEvents(pointers);
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
            var position = e.GetPosition(this);
            if (e.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                TouchHolding?.Invoke(this, new PositionEventArgs(position));
            }
        }

        private void _TouchTapped(object sender, TappedRoutedEventArgs e)
        {
            var position = e.GetPosition(this);
            var width = this.ActualWidth;
            double controlWidth = width / 5.0;

            TouchTapped?.Invoke(this, new PositionEventArgs(position));
        }
        #endregion
    }
}
