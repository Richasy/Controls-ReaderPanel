using HtmlAgilityPack;
using Richasy.Controls.Reader.Enums;
using Richasy.Controls.Reader.Models;
using Richasy.Controls.Reader.Models.Epub;
using Richasy.Controls.Reader.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Richasy.Controls.Reader
{
    [TemplatePart(Name = "MainPresenter", Type = typeof(ContentPresenter))]
    public partial class ReaderPanel : Control
    {
        public ReaderPanel()
        {
            this.DefaultStyleKey = typeof(ReaderPanel);
            // 默认分章表达式
            ChapterDivisionRegex = new Regex(@"第(.{1,9})[章节卷集部篇回幕计](\s{0})(.*)($\s*)");
            ChapterEndKey = new string[] { "部", "卷", "章", "节", "集", "回", "幕", "计", " " };
            ChapterExtraKey = new string[] { "序", "前言", "后记", "楔子", "附录", "外传" };
        }

        protected override void OnApplyTemplate()
        {
            _mainPresenter = GetTemplateChild("MainPresenter") as ContentPresenter;
            base.OnApplyTemplate();
        }

        /// <summary>
        /// 打开书籍文件
        /// </summary>
        /// <param name="bookFile">书籍文件</param>
        /// <param name="style">阅读器样式</param>
        /// <returns></returns>
        public async Task OpenAsync(StorageFile bookFile, ReaderStyle style)
        {
            if (bookFile == null)
                throw new ArgumentNullException();

            string extension = Path.GetExtension(bookFile.Path).ToLower();
            if (extension != ".epub" && extension != ".txt")
            {
                throw new NotSupportedException("File type not support (Currently only support txt and epub file)");
            }
            OpenStarting?.Invoke(this, EventArgs.Empty);
            if (extension == ".txt")
            {
                if (!(style is TxtViewStyle))
                    throw new ArgumentException("Open txt file need TxtViewStyle argument");
                ReaderType = Enums.ReaderType.Txt;
                Chapters = await GetTxtChapters(bookFile);
                ChapterLoaded?.Invoke(this, Chapters);
                if (_mainPresenter.Content == null || !(_mainPresenter.Content is TxtView))
                {
                    if (_txtView == null)
                    {
                        _txtView = new TxtView();
                        _txtView.PrevPageSelected += OnPrevPageSelected;
                        _txtView.NextPageSelected += OnNextPageSelected;
                        _txtView.LoadingStatusChanged += OnLoad;
                        _txtView.ProgressChanged += OnProgressChanged;
                        _txtView.TouchHolding += OnTouchHolding;
                        _txtView.TouchTapped += (_s, _e) => { TouchTapped?.Invoke(_s, _e); };
                        _txtView.Loaded += (_s, _e) =>
                        {
                            _txtView.SingleColumnMaxWidth = SingleColumnMaxWidth;
                            _txtView.ReaderFlyout = ReaderFlyout;
                            ViewLoaded?.Invoke(this, EventArgs.Empty);
                        };
                    }
                    _mainPresenter.Content = _txtView;
                }
                _txtContent = await GetTxtContent(bookFile);
                _txtView.ViewStyle = style as TxtViewStyle;
            }
            else
            {
                if (!(style is EpubViewStyle))
                    throw new ArgumentException("Open epub file need EpubViewStyle argument");
                ReaderType = Enums.ReaderType.Epub;
                _epubContent = await EpubReader.Read(bookFile, Encoding.Default);
                Chapters = GetEpubChapters(_epubContent);
                ChapterLoaded?.Invoke(this, Chapters);
                if (_mainPresenter.Content == null || !(_mainPresenter.Content is EpubView))
                {
                    if (_epubView == null)
                    {
                        _epubView = new EpubView();
                        _epubView.PrevPageSelected += OnPrevPageSelected;
                        _epubView.NextPageSelected += OnNextPageSelected;
                        _epubView.LoadingStatusChanged += OnLoad;
                        _epubView.ProgressChanged += OnProgressChanged;
                        _epubView.TouchHolding += OnTouchHolding;
                        _epubView.TouchTapped += (_s, _e) => { TouchTapped?.Invoke(_s, _e); };
                        _epubView.Loaded += (_s, _e) =>
                        {
                            _epubView.SingleColumnMaxWidth = SingleColumnMaxWidth;
                            _epubView.ReaderFlyout = ReaderFlyout;
                            ViewLoaded?.Invoke(this, EventArgs.Empty);
                        };
                        _epubView.LinkTapped += (_s, _e) => { LinkTapped?.Invoke(this, _e); };
                        _epubView.ImageTapped += (_s, _e) => { ImageTapped?.Invoke(this, _e); };
                    }

                    _mainPresenter.Content = _epubView;
                }
                _epubView.Init(_epubContent, style as EpubViewStyle);
            }
            OpenCompleted?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 加载自定义视图（通常用于在线阅读）
        /// </summary>
        /// <param name="chapters">目录列表</param>
        /// <param name="style">视图样式</param>
        /// <param name="details">目录详情列表（其中包含单个章节的内容）</param>
        public void LoadCustomView(List<Chapter> chapters, TxtViewStyle style, List<ChapterDetail> details = null)
        {
            if (chapters == null || chapters.Count == 0 || style == null)
                throw new ArgumentNullException();

            ReaderType = Enums.ReaderType.Custom;
            Chapters = chapters;
            if (_mainPresenter.Content == null || !(_mainPresenter.Content is TxtView))
            {
                if (_txtView == null)
                {
                    _txtView = new TxtView();
                    _txtView.PrevPageSelected += OnPrevPageSelected;
                    _txtView.NextPageSelected += OnNextPageSelected;
                    _txtView.LoadingStatusChanged += OnLoad;
                    _txtView.ProgressChanged += OnProgressChanged;
                    _txtView.TouchHolding += OnTouchHolding;
                    _txtView.TouchTapped += (_s, _e) => { TouchTapped?.Invoke(_s, _e); };
                    _txtView.Loaded += (_s, _e) =>
                    {
                        _txtView.SingleColumnMaxWidth = SingleColumnMaxWidth;
                        _txtView.ReaderFlyout = ReaderFlyout;
                        ViewLoaded?.Invoke(this, EventArgs.Empty);
                    };
                }
                _mainPresenter.Content = _txtView;
            }
            _txtView.ViewStyle = style;
            if (details != null)
                CustomChapterDetailList = details;
            else
                CustomChapterDetailList = new List<ChapterDetail>();
        }

        /// <summary>
        /// 加载阅读历史（需要与当前书籍匹配）
        /// </summary>
        /// <param name="history">阅读历史</param>
        public void LoadHistory(History history)
        {
            SetProgress(history.Chapter, history.Start);
        }

        /// <summary>
        /// 加载章节（需要与当前书籍匹配）
        /// </summary>
        /// <param name="chapter">章节</param>
        public void LoadChapter(Chapter chapter)
        {
            SetProgress(chapter, 0);
        }

        /// <summary>
        /// 更新阅读器样式（视图与样式需匹配，比如阅读Txt时需传入TxtViewStyle）
        /// </summary>
        /// <param name="style">阅读器样式</param>
        public void UpdateStyle(ReaderStyle style)
        {
            if ((ReaderType == Enums.ReaderType.Txt && !(style is TxtViewStyle))
                || (ReaderType == Enums.ReaderType.Epub && !(style is EpubViewStyle)))
                throw new ArgumentException("The format of the view and style must match");
            var view = _mainPresenter.Content as ReaderViewBase;
            if (view != null)
                view.UpdateStyle(style);
        }

        /// <summary>
        /// 阅读器翻页（下一页）
        /// </summary>
        public void Next()
        {
            var display = _mainPresenter.Content as ReaderViewBase;
            if (display == null)
                throw new InvalidOperationException("Reader not loaded or no open book");
            display.Next();
        }

        /// <summary>
        /// 阅读器翻页（上一页）
        /// </summary>
        public void Previous()
        {
            var display = _mainPresenter.Content as ReaderViewBase;
            if (display == null)
                throw new InvalidOperationException("Reader not loaded or no open book");
            display.Previous();
        }

        /// <summary>
        /// 定位到指定文件名的章节（EPUB）
        /// </summary>
        /// <param name="fileName"></param>
        public void LocateToSpecificFile(string fileName)
        {
            if (ReaderType != Enums.ReaderType.Epub)
                throw new NotSupportedException("This method can only use in epub view");
            else if (_epubView == null)
                throw new InvalidOperationException("Epub view not loaded");
            else if (_epubContent == null)
                throw new InvalidOperationException("Epub content not loaded");
            else if (string.IsNullOrEmpty(fileName) || !fileName.Contains(".html", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Invalid file name");

            var orders = _epubContent.SpecialResources.HtmlInReadingOrder;
            var info = orders.Where(p => p.AbsolutePath.Contains(fileName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            string content = string.Empty;
            if (info != null)
            {
                content = info.TextContent;
                _tempEpubChapterIndex = orders.IndexOf(info);
            }
            else
                throw new FileNotFoundException("File not found");
            _epubView.SetContent(content, Enums.ReaderStartMode.First, 0);
            var chapter = GetLastEpubChapter(info);
            if (!chapter.Equals(CurrentChapter))
            {
                CurrentChapter = chapter;
                ChapterChanged?.Invoke(this, chapter);
                RaiseProgressChanged(0);
            }

        }

        /// <summary>
        /// 获取指定章节内指定ID的内容，通常是注释（EPUB）
        /// </summary>
        /// <param name="fileName">文件名（为空时指当前章节）</param>
        /// <param name="id">ID值</param>
        public Tip GetSpecificIdContent(string id, string fileName = "")
        {
            if (ReaderType != Enums.ReaderType.Epub)
                throw new NotSupportedException("This method can only use in epub view");
            else if (_epubView == null)
                throw new InvalidOperationException("Epub view not loaded");
            else if (_epubContent == null)
                throw new InvalidOperationException("Epub content not loaded");
            else if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Invalid Id");
            else if (!string.IsNullOrEmpty(fileName) && !fileName.Contains(".html", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Invalid File name");

            EpubTextFile info = null;
            var orders = _epubContent.SpecialResources.HtmlInReadingOrder;
            if (!string.IsNullOrEmpty(fileName))
                info = orders.Where(p => p.AbsolutePath.Contains(fileName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            else
                info = orders[_tempEpubChapterIndex];
            if (info == null)
                throw new FileNotFoundException("File not found");

            var doc = new HtmlDocument();
            doc.LoadHtml(info.TextContent);
            var node = doc.GetElementbyId(id);
            var tip = new Tip();
            tip.Id = id;
            if (node != null)
            {
                string title = node.InnerText;
                string desc = node.ParentNode.InnerText;
                if (!string.IsNullOrEmpty(title))
                    desc.Replace(title, "");
                tip.Title = title.Trim();
                tip.Description = desc.Trim();
            }
            return tip;
        }

        /// <summary>
        /// 设置自定义内容
        /// </summary>
        /// <param name="content">文本内容</param>
        /// <param name="mode">起始位置</param>
        public void SetCustomContent(string content, ReaderStartMode mode)
        {
            _txtView.SetContent(content, mode);
        }
    }
}
