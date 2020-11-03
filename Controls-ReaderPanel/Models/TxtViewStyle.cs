using Windows.UI.Xaml;

namespace Richasy.Controls.Reader.Models
{
    public class TxtViewStyle : ReaderStyle
    {
        public double HeaderFontSize { get; set; }
        public Thickness HeaderMargin { get; set; }

        public TxtViewStyle() : base()
        {
            HeaderFontSize = FontSize * 1.6;
            HeaderMargin = new Thickness(0, 0, 0, FontSize);
        }

        public new TxtViewStyle Copy()
        {
            var temp = base.Copy();
            var view = temp as TxtViewStyle;
            view.HeaderFontSize = HeaderFontSize;
            view.HeaderMargin = HeaderMargin;
            return view;
        }
    }
}
