using Richasy.Controls.Reader.Models;
using Windows.UI.Composition;
using Windows.UI.Composition.Interactions;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Richasy.Controls.Reader.Views
{
    public partial class TxtView
    {
        private RichTextBlock _txtBlock;
        private Grid _txtGrid;

        public int _startTextIndex = 0;
        string _content;
        double _startX = 0;
        int _columns = 1;
        bool IsCoreSelectedChanged;
        bool IsAnimating;
        EventWaiter IndexWaiter;
        EventDelayer CreateContentDelayer;

        private bool _isSizeChangeLoaded = false;

        private double ParentWidth
        {
            get => (VisualTreeHelper.GetParent(_txtGrid) as FrameworkElement).ActualWidth;
        }

        public string SelectedText
        {
            get => _txtBlock.SelectedText;
        }

        Compositor compositor;
        Vector3KeyFrameAnimation OffsetAnimation;
        Visual PanelVisual;
        Visual ReaderViewVisual;

        InteractionTracker _tracker;
        VisualInteractionSource _source;
        ExpressionAnimation OffsetBind;

        PointerEventHandler PointerWheelChangedEventHandler;
        PointerEventHandler PointerPressedEventHandler;
        PointerEventHandler PointerMovedEventHandler;
        PointerEventHandler PointerReleasedEventHandler;
        PointerEventHandler PointerCanceledEventHandler;
        TappedEventHandler TouchTappedEventHandler;
        HoldingEventHandler TouchHoldingEventHandler;
        GestureRecognizer _gestureRecognizer;
    }
}
