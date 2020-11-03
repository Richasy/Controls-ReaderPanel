using Windows.UI;
using Windows.UI.Xaml;

namespace Richasy.Controls.Reader.Models
{
    public class ReaderStyle
    {
        public string FontFamily { get; set; }
        public double FontSize { get; set; }
        public double TextIndent { get; set; }
        public Color Foreground { get; set; }
        public Color Background { get; set; }
        public double LineHeight { get; set; }
        public Thickness Padding { get; set; }
        public bool IsAcrylicBackground { get; set; }

        public ReaderStyle()
        {
            FontFamily = "Microsoft YaHei";
            FontSize = 17;
            TextIndent = 2;
            Foreground = Colors.Black;
            Background = Colors.White;
            LineHeight = 30;
            Padding = new Thickness(40, 20, 40, 20);
            IsAcrylicBackground = false;
        }

        public ReaderStyle Copy()
        {
            var style = new ReaderStyle();
            style.FontFamily = FontFamily;
            style.FontSize = FontSize;
            style.TextIndent = TextIndent;
            style.Foreground = Foreground;
            style.Background = Background;
            style.LineHeight = LineHeight;
            style.Padding = Padding;
            style.IsAcrylicBackground = IsAcrylicBackground;
            return style;
        }
    }
}
