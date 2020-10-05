namespace Richasy.Controls.Reader.Models
{
    public class TxtViewStyle : ReaderStyle
    {
        public double HeaderFontSize { get; set; }

        public TxtViewStyle() : base()
        {
            HeaderFontSize = FontSize * 1.6;
        }

        public new TxtViewStyle Copy()
        {
            var temp = base.Copy();
            var view = temp as TxtViewStyle;
            view.HeaderFontSize = HeaderFontSize;
            return view;
        }
    }
}
