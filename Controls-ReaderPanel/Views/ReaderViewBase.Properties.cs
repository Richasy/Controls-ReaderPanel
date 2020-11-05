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

        public int Index
        {
            get { return (int)GetValue(IndexProperty); }
            set { SetValue(IndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Index.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IndexProperty =
            DependencyProperty.Register("Index", typeof(int), typeof(ReaderViewBase), new PropertyMetadata(0, new PropertyChangedCallback(Index_Changed)));

        private static void Index_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                var index = (int)e.NewValue;
                if (d is ReaderViewBase sender)
                {
                    if (!sender.IsCoreSelectedChanged)
                    {
                        sender.GoToIndex(index);
                    }
                    try
                    {
                        int currentOverflowIndex = sender._columns * index;
                        int length = 0;
                        //if (currentOverflowIndex > sender._tempOverflowList.Count)
                        //    length = sender._displayBlock.ContentEnd.Offset;
                        //else
                        //{
                        //    var item = sender._tempOverflowList[currentOverflowIndex];
                        //    if (item.Item2 is RichTextBlockOverflow of)
                        //        length = of.ContentStart.Offset;
                        //}
                        length = Convert.ToInt32(sender._content.Length * (currentOverflowIndex / (sender._tempOverflowList.Count * 1.0)));
                        sender._startTextIndex = length;
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        Debug.WriteLine(ex.Message);
#endif
                    }
                    if (index < 0) sender.OnPrevPageSelected();
                    else if (index > sender.Count - 1) sender.OnNextPageSelected();
                    sender.OnProgressChanged();
                }
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
