using Windows.UI;
using Windows.UI.Xaml;

namespace Richasy.Controls.Reader.Models
{
    public class ReaderStyle
    {
        /// <summary>
        /// 字体
        /// </summary>
        public string FontFamily { get; set; }
        /// <summary>
        /// 字体大小
        /// </summary>
        public double FontSize { get; set; }
        /// <summary>
        /// 行首缩进
        /// </summary>
        public double TextIndent { get; set; }
        /// <summary>
        /// 文本颜色
        /// </summary>
        public Color Foreground { get; set; }
        /// <summary>
        /// 阅读器背景色
        /// </summary>
        public Color Background { get; set; }
        /// <summary>
        /// 行高
        /// </summary>
        public double LineHeight { get; set; }
        /// <summary>
        /// 段间距
        /// </summary>
        public double SegmentSpacing { get; set; }
        /// <summary>
        /// 阅读器内间距
        /// </summary>
        public Thickness Padding { get; set; }
        /// <summary>
        /// 是否为亚克力背景（可通过Background.A调整透明度）
        /// </summary>
        public bool IsAcrylicBackground { get; set; }
        /// <summary>
        /// 标题大小（TXT）
        /// </summary>
        public double HeaderFontSize { get; set; }
        /// <summary>
        /// 标题边距（TXT）
        /// </summary>
        public Thickness HeaderMargin { get; set; }
        /// <summary>
        /// 列表沟宽（EPUB）
        /// </summary>
        public double ListGutterWidth { get; set; }
        /// <summary>
        /// 标题下划线颜色（EPUB）
        /// </summary>
        public Color HeaderLineColor { get; set; }
        /// <summary>
        /// 引用块边框颜色（EPUB）
        /// </summary>
        public Color BlockquoteBorderColor { get; set; }

        public ReaderStyle()
        {
            FontFamily = "Microsoft YaHei UI";
            FontSize = 17;
            TextIndent = 2;
            Foreground = Colors.Black;
            Background = Colors.White;
            LineHeight = 30;
            SegmentSpacing = 15;
            Padding = new Thickness(40, 20, 40, 20);
            IsAcrylicBackground = false;
            ListGutterWidth = FontSize / 2.0;
            HeaderLineColor = Foreground;
            BlockquoteBorderColor = Colors.Gray;
            HeaderFontSize = FontSize * 1.6;
            HeaderMargin = new Thickness(0, 0, 0, FontSize);
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
            style.SegmentSpacing = SegmentSpacing;
            style.IsAcrylicBackground = IsAcrylicBackground;

            style.ListGutterWidth = ListGutterWidth;
            style.HeaderLineColor = HeaderLineColor;
            style.BlockquoteBorderColor = BlockquoteBorderColor;

            style.HeaderFontSize = HeaderFontSize;
            style.HeaderMargin = HeaderMargin;

            return style;
        }
    }
}
