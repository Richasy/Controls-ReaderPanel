using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;

namespace Richasy.Controls.Reader
{
    public partial class ReaderPanel
    {

        public object Header
        {
            get { return (object)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(object), typeof(ReaderPanel), new PropertyMetadata(null));

        public object Footer
        {
            get { return (object)GetValue(FooterProperty); }
            set { SetValue(FooterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Footer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FooterProperty =
            DependencyProperty.Register("Footer", typeof(object), typeof(ReaderPanel), new PropertyMetadata(null));

        public object Content
        {
            get { return (object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Content.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(object), typeof(ReaderPanel), new PropertyMetadata(null));

        public double SingleColumnMaxWidth
        {
            get { return (double)GetValue(SingleColumnMaxWidthProperty); }
            set { SetValue(SingleColumnMaxWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SingleColumnMaxWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SingleColumnMaxWidthProperty =
            DependencyProperty.Register("SingleColumnMaxWidth", typeof(double), typeof(ReaderPanel), new PropertyMetadata(800.0, new PropertyChangedCallback(SingleColumnMaxWidth_Changed)));

        private static void SingleColumnMaxWidth_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            double width = (double)e.NewValue;
            var instance = d as ReaderPanel;
            if (width <= 100)
                instance.SingleColumnMaxWidth = (double)e.OldValue;
            else if(instance._readerView != null)
            {
                instance._readerView.SingleColumnMaxWidth = width;
            }
        }

        public FlyoutBase ReaderFlyout
        {
            get { return (FlyoutBase)GetValue(ReaderFlyoutProperty); }
            set { SetValue(ReaderFlyoutProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ReaderFlyout.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ReaderFlyoutProperty =
            DependencyProperty.Register("ReaderFlyout", typeof(FlyoutBase), typeof(ReaderPanel), new PropertyMetadata(null,new PropertyChangedCallback(ReaderFlyout_Changed)));

        private static void ReaderFlyout_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue is FlyoutBase flyout)
            {
                var instance = d as ReaderPanel;
                if(instance._readerView!=null)
                    instance._readerView.ReaderFlyout = flyout;
            }
        }
    }
}
