using Windows.UI;
using Windows.UI.Xaml;

namespace Richasy.Controls.Reader.Models
{
    public class TxtViewStyle
    {
        public string FontFamily { get; set; }
        public double FontSize { get; set; }
        public double TextIndent { get; set; }
        public Color Foreground { get; set; }
        public Color Background { get; set; }
        public double LineHeight { get; set; }
        public Thickness Padding { get; set; }
        public bool IsAcrylicBackground { get; set; }
        public double HeaderFontSize { get; set; }

        public TxtViewStyle()
        {
            FontFamily = "Microsoft YaHei";
            FontSize = 17;
            HeaderFontSize = FontSize * 1.6;
            TextIndent = 2;
            Foreground = Colors.Black;
            Background = Colors.White;
            LineHeight = 30;
            Padding = new Thickness(50, 20, 50, 20);
            IsAcrylicBackground = false;
        }

        public static TxtViewStyle Copy(TxtViewStyle _source)
        {
            var style = new TxtViewStyle();
            style.FontFamily = _source.FontFamily;
            style.FontSize = _source.FontSize;
            style.TextIndent = _source.TextIndent;
            style.Foreground = _source.Foreground;
            style.Background = _source.Background;
            style.LineHeight = _source.LineHeight;
            style.Padding = _source.Padding;
            style.IsAcrylicBackground = _source.IsAcrylicBackground;
            return style;
        }
    }
}
