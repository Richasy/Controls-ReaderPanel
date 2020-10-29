using Richasy.Controls.Reader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Richasy.Controls.Reader
{
    public partial class ReaderPanel
    {
        /// <summary>
        /// 章节划分正则表达式
        /// </summary>
        public Regex ChapterDivisionRegex { get; set; }

        /// <summary>
        /// 目录列表
        /// </summary>
        public List<Chapter> Chapters { get; internal set; }

        /// <summary>
        /// 读取Txt文件时的全部文本缓存
        /// </summary>
        private string _totalTxtContent;
        /// <summary>
        /// 解析Txt文件进行分章时最大解析行数
        /// </summary>
        private static int MAX_TXT_PARSE_LINES = 200;

        /// <summary>
        /// 当前在读章节
        /// </summary>
        public Chapter CurrentChapter { get; internal set; }
    }
}
