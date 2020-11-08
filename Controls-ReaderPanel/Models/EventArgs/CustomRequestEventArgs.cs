using Richasy.Controls.Reader.Enums;
using System;

namespace Richasy.Controls.Reader.Models
{
    public class CustomRequestEventArgs : EventArgs
    {
        /// <summary>
        /// 起始位置
        /// </summary>
        public ReaderStartMode StartMode { get; set; }
        /// <summary>
        /// 发起数据请求的章节
        /// </summary>
        public Chapter RequestChapter { get; set; }
        /// <summary>
        /// 偏移值
        /// </summary>
        public int AddonLength { get; set; }
        public CustomRequestEventArgs() { }
        public CustomRequestEventArgs(ReaderStartMode mode, Chapter chapter, int addon = 0)
        {
            StartMode = mode;
            RequestChapter = chapter;
            AddonLength = addon;
        }
    }
}
