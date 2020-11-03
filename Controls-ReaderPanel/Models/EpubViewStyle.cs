using Windows.UI;

namespace Richasy.Controls.Reader.Models
{
    public class EpubViewStyle : ReaderStyle
    {
        public double ListGutterWidth { get; set; }
        public Color HeaderLineColor { get; set; }
        public Color BlockquoteBorderColor { get; set; }
        public EpubViewStyle() : base()
        {
            ListGutterWidth = FontSize/2.0;
            HeaderLineColor = Foreground;
            BlockquoteBorderColor = Colors.Gray;
        }

        public new EpubViewStyle Copy()
        {
            var temp = base.Copy();
            var view = temp as EpubViewStyle;
            view.ListGutterWidth = ListGutterWidth;
            view.HeaderLineColor = HeaderLineColor;
            return view;
        }
    }
}
