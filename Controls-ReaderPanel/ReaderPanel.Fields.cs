using Richasy.Controls.Reader.Enums;
using Richasy.Controls.Reader.Models;
using Richasy.Controls.Reader.Models.Epub;
using Richasy.Controls.Reader.Views;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml.Controls;

namespace Richasy.Controls.Reader
{
    public partial class ReaderPanel
    {
        private Grid _rootGrid;
        private Grid _contentContainer;
        private ReaderView _readerView;
        /// <summary>
        /// 读取Txt文件时的全部文本缓存
        /// </summary>
        private string _txtContent;
        /// <summary>
        /// 解析的Epub书籍
        /// </summary>
        private EpubBook _epubContent;
        /// <summary>
        /// 解析Txt文件进行分章时最大解析行数
        /// </summary>
        private static int MAX_TXT_PARSE_LINES = 200;
        /// <summary>
        /// 章节字符数限制
        /// </summary>
        private static int MAX_CHAPTER_LENGTH = 50;

        private bool _isLoading = false;

        private int _tempEpubChapterIndex = 0;

        internal SpeechSynthesisStream _tempSpeechStream = null;

        /// <summary>
        /// 章节划分正则表达式
        /// </summary>
        public Regex ChapterDivisionRegex { get; set; }

        /// <summary>
        /// 目录列表
        /// </summary>
        public List<Chapter> Chapters { get; internal set; }
        /// <summary>
        /// 当前在读章节
        /// </summary>
        public Chapter CurrentChapter { get; internal set; }
        /// <summary>
        /// 阅读器类型
        /// </summary>
        public ReaderType ReaderType { get; private set; }
        /// <summary>
        /// 自定义分章列表
        /// </summary>
        public List<ChapterDetail> CustomChapterDetailList { get; set; }

        public string[] ChapterEndKey { get; set; }
        public string[] ChapterExtraKey { get; set; }

        /// <summary>
        /// 选中文本
        /// </summary>
        public string SelectedText
        {
            get
            {
                return _readerView.SelectedText;
            }
        }

        public Func<double> RootWidth()
        {
            if (_rootGrid != null)
                return () =>
                {
                    return _rootGrid.ActualWidth;
                };
            else
                return () =>
                {
                    return 0;
                };
        }
        public Func<double> ContentHeight()
        {
            return () =>
            {
                return _contentContainer.ActualHeight;
            };
        }
    }
}
