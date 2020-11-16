using Richasy.Controls.Reader.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Richasy.Controls.Reader.Views
{
    public partial class ReaderViewBase
    {
        private int _index;
        public int Index
        {
            get => _index;
            set
            {
                _index = value;
                int index = value;
                if (!IsCoreSelectedChanged)
                {
                    GoToIndex(index);
                }
                try
                {
                    int currentOverflowIndex = _columns * index;
                    int length = 0;
                    length = Convert.ToInt32(_content.Length * (currentOverflowIndex / (_tempOverflowList.Count * 1.0)));
                    _startTextIndex = length;
                }
                catch (Exception ex)
                {
#if DEBUG
                    Debug.WriteLine(ex.Message);
#endif
                }
                if (index < 0) OnPrevPageSelected();
                else if (index > Count - 1) OnNextPageSelected();
                OnProgressChanged();
            }
        }
        
        public int Count
        {
            get { return (int)GetValue(CountProperty); }
            set { SetValue(CountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Count.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CountProperty =
            DependencyProperty.Register("Count", typeof(int), typeof(ReaderViewBase), new PropertyMetadata(0, new PropertyChangedCallback(Count_Changed)));

        private static void Count_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                if (d is ReaderViewBase sender)
                {
                    sender.InitTrackerPositions();
                }
            }
        }

        public double SingleColumnMaxWidth
        {
            get { return (double)GetValue(SingleColumnMaxWidthProperty); }
            set { SetValue(SingleColumnMaxWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SingleColumnMaxWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SingleColumnMaxWidthProperty =
            DependencyProperty.Register("SingleColumnMaxWidth", typeof(double), typeof(ReaderViewBase), new PropertyMetadata(800.0, new PropertyChangedCallback(SingleColumnMaxWidth_Changed)));

        private static void SingleColumnMaxWidth_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = d as ReaderViewBase;
            sender.SizeChangeHandle();
        }

        public ReaderStyle ViewStyle
        {
            get { return (ReaderStyle)GetValue(ViewStyleProperty); }
            set { SetValue(ViewStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewStyleProperty =
            DependencyProperty.Register("ViewStyle", typeof(ReaderStyle), typeof(ReaderViewBase), new PropertyMetadata(new ReaderStyle()));

        public FlyoutBase ReaderFlyout
        {
            get { return (FlyoutBase)GetValue(ReaderFlyoutProperty); }
            set { SetValue(ReaderFlyoutProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ReadFlyout.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ReaderFlyoutProperty =
            DependencyProperty.Register("ReaderFlyout", typeof(FlyoutBase), typeof(ReaderViewBase), new PropertyMetadata(null, new PropertyChangedCallback(ReaderFlyout_Changed)));

        private static void ReaderFlyout_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is FlyoutBase flyout && e.NewValue != null)
            {
                var sender = d as ReaderViewBase;
                if (sender._displayBlock != null)
                    sender._displayBlock.SelectionFlyout = sender._displayBlock.ContextFlyout = flyout;
            }
        }

        public int OverflowRenderMaximum
        {
            get { return (int)GetValue(OverflowRenderMaximumProperty); }
            set { SetValue(OverflowRenderMaximumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OverflowRenderMaximum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OverflowRenderMaximumProperty =
            DependencyProperty.Register("OverflowRenderMaximum", typeof(int), typeof(ReaderViewBase), new PropertyMetadata(40));


    }
}
