using Richasy.Controls.Reader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Richasy.Controls.Reader.Views
{
    public partial class EpubView
    {

        public new EpubViewStyle ViewStyle
        {
            get { return (EpubViewStyle)GetValue(ViewStyleProperty); }
            set { SetValue(ViewStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewStyle.  This enables animation, styling, binding, etc...
        public static new readonly DependencyProperty ViewStyleProperty =
            DependencyProperty.Register("ViewStyle", typeof(EpubViewStyle), typeof(EpubView), new PropertyMetadata(new EpubViewStyle()));


    }
}
