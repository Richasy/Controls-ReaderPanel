namespace Richasy.Controls.Reader.Models
{
    public class InsideSearchItem
    {
        /// <summary>
        /// 章节
        /// </summary>
        public Chapter Chapter { get; set; }
        /// <summary>
        /// 偏移值
        /// </summary>
        public int AddonLength { get; set; }
        /// <summary>
        /// 匹配的字符
        /// </summary>
        public string SearchText { get; set; }
        /// <summary>
        /// 上下文
        /// </summary>
        public string DisplayText { get; set; }
    }
}
