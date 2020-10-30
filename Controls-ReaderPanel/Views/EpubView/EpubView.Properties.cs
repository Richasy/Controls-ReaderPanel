using Richasy.Controls.Reader.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;

namespace Richasy.Controls.Reader.Views
{
    public partial class EpubView
    {

        public int Index
        {
            get { return (int)GetValue(IndexProperty); }
            set { SetValue(IndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Index.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IndexProperty =
            DependencyProperty.Register("Index", typeof(int), typeof(EpubView), new PropertyMetadata(0, new PropertyChangedCallback(Index_Changed)));

        private static void Index_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                var index = (int)e.NewValue;
                if (d is EpubView sender)
                {
                    if (!sender.IsCoreSelectedChanged)
                    {
                        sender.GoToIndex(index);
                    }
                    try
                    {
                        double xi = (Math.Abs(index * sender._columns) * 1.0) / sender._epubGrid.Children.Count;
                        int length = Convert.ToInt32(sender._content.Length * xi);
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
            DependencyProperty.Register("Count", typeof(int), typeof(EpubView), new PropertyMetadata(0, new PropertyChangedCallback(Count_Changed)));

        private static void Count_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                if (d is EpubView sender)
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
            DependencyProperty.Register("SingleColumnMaxWidth", typeof(double), typeof(EpubView), new PropertyMetadata(800.0, new PropertyChangedCallback(SingleColumnMaxWidth_Changed)));

        private static void SingleColumnMaxWidth_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = d as EpubView;
            sender.SizeChangeHandle();
        }

        public EpubViewStyle ViewStyle
        {
            get { return (EpubViewStyle)GetValue(ViewStyleProperty); }
            set { SetValue(ViewStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewStyleProperty =
            DependencyProperty.Register("ViewStyle", typeof(EpubViewStyle), typeof(EpubView), new PropertyMetadata(new EpubViewStyle()));

        public FlyoutBase ReadFlyout
        {
            get { return (FlyoutBase)GetValue(ReadFlyoutProperty); }
            set { SetValue(ReadFlyoutProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ReadFlyout.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ReadFlyoutProperty =
            DependencyProperty.Register("ReadFlyout", typeof(FlyoutBase), typeof(EpubView), new PropertyMetadata(null, new PropertyChangedCallback(ReadFlyout_Changed)));

        private static void ReadFlyout_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is FlyoutBase flyout && e.NewValue != null)
            {
                var sender = d as EpubView;
                if (sender._epubBlock != null)
                    sender._epubBlock.SelectionFlyout = flyout;
            }
        }
    }
}
