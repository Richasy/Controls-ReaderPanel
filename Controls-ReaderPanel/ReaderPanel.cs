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
using Windows.Foundation;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

namespace Richasy.Controls.Reader
{
    [TemplatePart(Name = "ReaderView", Type = typeof(ReaderView))]
    [TemplatePart(Name = "RootGrid", Type = typeof(Grid))]
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
            _contentContainer = GetTemplateChild("ContentContainer") as Grid;
            _readerView = GetTemplateChild("ReaderView") as ReaderView;
            _readerView.PrevPageSelected += OnPrevPageSelected;
            _readerView.NextPageSelected += OnNextPageSelected;
            _readerView.LoadingStatusChanged += OnLoad;
            _readerView.ProgressChanged += OnProgressChanged;
            _readerView.TouchHolding += OnTouchHolding;
            _readerView.TouchTapped += (_s, _e) => { TouchTapped?.Invoke(_s, _e); };
            _readerView.Loaded += (_s, _e) =>
            {
                _readerView.SingleColumnMaxWidth = SingleColumnMaxWidth;
                _readerView.ReaderFlyout = ReaderFlyout;
                ViewLoaded?.Invoke(this, EventArgs.Empty);
            };
            _readerView.LinkTapped += (_s, _e) => { LinkTapped?.Invoke(this, _e); };
            _readerView.ImageTapped += (_s, _e) => { ImageTapped?.Invoke(this, _e); };
            _rootGrid = GetTemplateChild("RootGrid") as Grid;
            base.OnApplyTemplate();
        }

        /// <summary>
        /// 打开书籍文件
        /// </summary>
        /// <param name="bookFile">书籍文件</param>
        /// <param name="style">阅读器样式</param>
        /// <param name="chapters">外部导入的目录（通常是前一次生成的目录，以避免重复生成目录），为<c>null</c>或空列表将重新生成目录</param>
        /// <returns></returns>
        public async Task OpenAsync(StorageFile bookFile, ReaderStyle style, List<Chapter> chapters = null)
        {
            if (bookFile == null)
                throw new ArgumentNullException();

            string extension = Path.GetExtension(bookFile.Path).ToLower();
            if (extension != ".epub" && extension != ".txt")
            {
                throw new NotSupportedException("File type not support (Currently only support txt and epub file)");
            }
            OpenStarting?.Invoke(this, EventArgs.Empty);
            if (_tempSpeechStream != null)
            {
                _tempSpeechStream.Dispose();
                _tempSpeechStream = null;
            }
            bool hasExternalChapters = chapters != null && chapters.Count > 0;
            if (hasExternalChapters)
            {
                Chapters = chapters;
                ChapterLoaded?.Invoke(this, Chapters);
            }
            _readerView.ViewStyle = style;
            UpdateBackground(style);
            if (extension.ToLower() == ".txt")
            {
                ReaderType = _readerView.ReaderType = ReaderType.Txt;
                if (!hasExternalChapters)
                {
                    Chapters = await GetTxtChapters(bookFile);
                    ChapterLoaded?.Invoke(this, Chapters);
                }
                _txtContent = await GetTxtContent(bookFile);
            }
            else
            {
                ReaderType = _readerView.ReaderType = ReaderType.Epub;
                _epubContent = await EpubReader.Read(bookFile, Encoding.Default);
                if (!hasExternalChapters)
                {
                    Chapters = GetEpubChapters(_epubContent);
                    ChapterLoaded?.Invoke(this, Chapters);
                }
                _readerView.EpubInit(_epubContent, style);
            }

            OpenCompleted?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 重新生成Txt章节目录
        /// </summary>
        /// <param name="splitRegex">分章正则表达式</param>
        /// <param name="bookTitle">书籍标题（用于目录首章）</param>
        /// <param name="replaceCurrentChapters">是否替换当前阅读器内的章节目录（会重新加载最近的章节内容）</param>
        /// <returns></returns>
        public List<Chapter> RegenerateTxtChapters(Regex splitRegex, string bookTitle, bool replaceCurrentChapters = false)
        {
            if (ReaderType != ReaderType.Txt)
                throw new InvalidOperationException("The reader type must be Txt");
            if (string.IsNullOrEmpty(bookTitle))
                throw new ArgumentNullException("Should have the book title");
            ChapterDivisionRegex = splitRegex;
            var chapters = GetTxtChapters(_txtContent, bookTitle);
            if (replaceCurrentChapters)
            {
                Chapters = chapters;
                if (CurrentChapter != null)
                {
                    var tempChapter = chapters.Where(p => p.StartLength >= CurrentChapter.StartLength).FirstOrDefault();
                    if (tempChapter != null)
                        CurrentChapter = tempChapter;
                    else
                        CurrentChapter = chapters.First();
                }
                else
                    CurrentChapter = chapters.First();
                LoadChapter(CurrentChapter);
            }
            return chapters;
        }

        /// <summary>
        /// 加载自定义视图（通常用于在线阅读）
        /// </summary>
        /// <param name="chapters">目录列表</param>
        /// <param name="style">视图样式</param>
        /// <param name="details">目录详情列表（其中包含单个章节的内容）</param>
        public void LoadCustomView(List<Chapter> chapters, ReaderStyle style, List<ChapterDetail> details = null)
        {
            if (chapters == null || chapters.Count == 0 || style == null)
                throw new ArgumentNullException();
            OpenStarting?.Invoke(this, EventArgs.Empty);
            if (_tempSpeechStream != null)
            {
                _tempSpeechStream.Dispose();
                _tempSpeechStream = null;
            }
            ReaderType = _readerView.ReaderType = ReaderType.Custom;
            Chapters = chapters;
            ChapterLoaded?.Invoke(this, Chapters);
            _readerView.ViewStyle = style;
            UpdateBackground(style);
            if (details != null)
                CustomChapterDetailList = details;
            else
                CustomChapterDetailList = new List<ChapterDetail>();
            OpenCompleted?.Invoke(this, EventArgs.Empty);
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
        public void LoadChapter(Chapter chapter, int addonLength = 0)
        {
            SetProgress(chapter, addonLength);
        }

        /// <summary>
        /// 加载搜索结果对应的位置（需要与当前书籍匹配）
        /// </summary>
        /// <param name="search">搜索结果</param>
        public void LoadSearchItem(InsideSearchItem search)
        {
            SetProgress(search.Chapter, search.AddonLength);
        }

        /// <summary>
        /// 更新阅读器样式
        /// </summary>
        /// <param name="style">阅读器样式</param>
        public void UpdateStyle(ReaderStyle style)
        {
            UpdateBackground(style);
            _readerView.UpdateStyle(style);
        }

        /// <summary>
        /// 更新阅读器背景
        /// </summary>
        /// <param name="style">阅读器样式</param>
        public void UpdateBackground(ReaderStyle style)
        {
            if (style.IsAcrylicBackground)
            {
                var opacity = Convert.ToInt32(style.Background.A) / 255.0;
                var tempBackground = style.Background;
                tempBackground.A = 255;
                var acrylic = new AcrylicBrush()
                {
                    TintColor = tempBackground,
                    TintOpacity = opacity,
                    FallbackColor = style.Background,
                    BackgroundSource = AcrylicBackgroundSource.HostBackdrop
                };
                _rootGrid.Background = acrylic;
                _readerView.Background = acrylic;
            }
            else
                _rootGrid.Background = _readerView.Background = new SolidColorBrush(style.Background);
        }

        /// <summary>
        /// 阅读器翻页（下一页）
        /// </summary>
        public void Next()
        {
            if (_readerView == null)
                throw new InvalidOperationException("Reader not loaded or no open book");
            _readerView.Next();
        }

        /// <summary>
        /// 阅读器翻页（上一页）
        /// </summary>
        public void Previous()
        {
            if (_readerView == null)
                throw new InvalidOperationException("Reader not loaded or no open book");
            _readerView.Previous();
        }

        /// <summary>
        /// 定位到指定文件名的章节（EPUB）
        /// </summary>
        /// <param name="fileName"></param>
        public void LocateToSpecificFile(string fileName)
        {
            if (ReaderType != Enums.ReaderType.Epub)
                throw new NotSupportedException("This method can only use in epub view");
            else if (_readerView == null)
                throw new InvalidOperationException("Epub view not loaded");
            else if (_epubContent == null)
                throw new InvalidOperationException("Epub content not loaded");
            else if (string.IsNullOrEmpty(fileName))
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
            _readerView.SetContent(content, Enums.ReaderStartMode.First, 0);
            var chapter = GetLastEpubChapter(info);
            if (!chapter.Equals(CurrentChapter))
            {
                CurrentChapter = chapter;
                ChapterChanged?.Invoke(this, chapter);
                RaiseProgressChanged(0);
            }

        }

        /// <summary>
        /// 获取指定文件名的HTML内容
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns></returns>
        public HtmlDocument GetSpecificFileDocument(string fileName = "")
        {
            if (ReaderType != ReaderType.Epub)
                throw new NotSupportedException("This method can only use in epub view");
            else if (_readerView == null)
                throw new InvalidOperationException("Epub view not loaded");
            else if (_epubContent == null)
                throw new InvalidOperationException("Epub content not loaded");

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
            return doc;
        }

        /// <summary>
        /// 获取指定章节内指定ID的内容，通常是注释（EPUB）
        /// </summary>
        /// <param name="fileName">文件名（为空时指当前章节）</param>
        /// <param name="id">ID值</param>
        public Tip GetSpecificIdContent(string id, string fileName = "")
        {
            var node = GetSpecificIdNode(id, fileName);
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
        /// 获取指定章节内指定ID的内容，通常是注释（EPUB）
        /// </summary>
        /// <param name="node">包含ID在内的HTML节点</param>
        public Tip GetSpecificIdContent(HtmlNode node, string id)
        {
            if (node == null)
                throw new ArgumentNullException("Invalid node");
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
        /// 获取指定章节内指定ID的节点
        /// </summary>
        /// <param name="fileName">文件名（为空时指当前章节）</param>
        /// <param name="id">ID值</param>
        public HtmlNode GetSpecificIdNode(string id, string fileName = "")
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Invalid Id");
            var doc = GetSpecificFileDocument(fileName);
            var node = doc.GetElementbyId(id);
            return node;
        }

        /// <summary>
        /// 设置自定义内容
        /// </summary>
        /// <param name="content">文本内容</param>
        /// <param name="mode">起始位置</param>
        public void SetCustomContent(string content, ReaderStartMode mode)
        {
            _readerView.SetContent(content, mode);
        }

        /// <summary>
        /// 设置自定义章节详情
        /// </summary>
        /// <param name="detail">章节详情</param>
        /// <param name="mode">起始位置</param>
        /// <param name="addonLength">偏移值</param>
        public void SetCustomContent(ChapterDetail detail, ReaderStartMode mode, int addonLength = 0)
        {
            _readerView.SetContent(detail.GetReadContent(), mode, addonLength);
        }

        /// <summary>
        /// 内部查询
        /// </summary>
        /// <param name="searchText">关键词（可以作为正则表达式）</param>
        public async Task<List<InsideSearchItem>> GetInsideSearchResultAsync(string searchText)
        {
            if (Chapters == null || Chapters.Count == 0)
                throw new InvalidCastException("Chapters not loaded");
            else if (string.IsNullOrEmpty(searchText))
                throw new ArgumentNullException("Search text is null or empty");

            var result = new List<InsideSearchItem>();
            var regex = new Regex(searchText, RegexOptions.IgnoreCase);
            if (ReaderType == ReaderType.Txt && !string.IsNullOrEmpty(_txtContent))
            {
                var matches = regex.Matches(_txtContent);
                if (matches.Count > 0)
                {
                    var tasks = new List<Task>();
                    for (int i = 0; i < matches.Count; i++)
                    {
                        int index = matches[i].Index;
                        string value = matches[i].Value;
                        if (value.Length > 60)
                            value = value.Substring(0, 60).Trim();
                        tasks.Add(Task.Run(() =>
                        {
                            string display = GetDisplayText(_txtContent, index, value);
                            var chapter = Chapters.Where(p => p.StartLength <= index).OrderByDescending(p => p.Index).FirstOrDefault();
                            if (chapter == null)
                                return;
                            var item = new InsideSearchItem();
                            item.Chapter = chapter;
                            item.DisplayText = display;
                            item.SearchText = value;
                            item.AddonLength = index - chapter.StartLength;
                            item.DisplayText = item.DisplayText.Trim();
                            result.Add(item);
                        }));
                    }
                    await Task.WhenAll(tasks.ToArray());
                }
            }
            else if (ReaderType == ReaderType.Epub && _epubContent != null)
            {
                var orders = _epubContent.SpecialResources.HtmlInReadingOrder;
                var tasks = new List<Task>();
                foreach (var order in orders)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        string text = Regex.Replace(order.TextContent, @"<[^>]*>", string.Empty);
                        var matches = regex.Matches(text);
                        if (matches.Count > 0)
                        {
                            var chapter = Chapters.Where(p => p.Link.Contains(order.AbsolutePath, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                            if (chapter == null)
                            {
                                chapter = GetLastEpubChapter(order);
                                if (chapter == null)
                                    return;
                            }
                            for (int i = 0; i < matches.Count; i++)
                            {
                                int index = matches[i].Index;
                                string value = matches[i].Value;
                                if (value.Length > 60)
                                    value = value.Substring(0, 60).Trim();
                                string display = GetDisplayText(text, index, value);
                                var item = new InsideSearchItem();
                                item.Chapter = chapter;
                                item.DisplayText = display.Trim();
                                item.SearchText = value;
                                item.AddonLength = index;
                                result.Add(item);
                            }
                        }
                    }));
                }
                await Task.WhenAll(tasks.ToArray());
            }
            else
            {
                var chapters = Chapters.Where(p => regex.IsMatch(p.Title));
                if (chapters.Count() > 0)
                {
                    foreach (var chapter in chapters)
                    {
                        var item = new InsideSearchItem();
                        item.Chapter = chapter;
                        item.DisplayText = chapter.Title;
                        item.SearchText = regex.Match(chapter.Title).Value;
                        item.AddonLength = 0;
                        result.Add(item);
                    }
                }
            }
            return result.OrderBy(p => p.Chapter.Index).ThenBy(p => p.AddonLength).ToList();
        }

        /// <summary>
        /// 获取章节语音
        /// </summary>
        /// <param name="chapter">章节</param>
        /// <param name="setCurrentChapter">是否将其设置为当前章节</param>
        /// <param name="synthesizer">语音合成器</param>
        /// <returns></returns>
        public async Task<MediaPlaybackItem> GetChapterVoiceAsync(Chapter chapter, bool setCurrentChapter = false, SpeechSynthesizer synthesizer = null)
        {
            if (Chapters.Count == 0)
                throw new InvalidCastException("Chapter not loaded");
            else if (chapter == null || !Chapters.Contains(chapter))
                throw new ArgumentException("The chapter is not in current book");

            // 将章节转化为语音合成流需要时间和额外的系统资源，不宜生成超量的文本
            string content = GetReadText(chapter);
            if (content.Length > SpeechMaxLength)
                throw new Exception("The chapter detail text length is too large");

            if (CurrentChapter != chapter)
                LoadChapter(chapter);

            // 控件原本是按需加载页面，但考虑到用户会对生成的语音流进行进度调整，所以在生成合成语音时会将当前章节未渲染的部分全部渲染
            _readerView.RenderAllOverflows();
            bool isTempSyn = false;
            if (synthesizer == null)
            {
                synthesizer = new SpeechSynthesizer();
                isTempSyn = true;
            }
            synthesizer.Options.IncludeSentenceBoundaryMetadata = true;
            synthesizer.Options.IncludeWordBoundaryMetadata = true;
            
            var stream = await synthesizer.SynthesizeTextToStreamAsync(content);
            _tempSpeechStream = stream;
            MediaSource source = MediaSource.CreateFromStream(stream, stream.ContentType);
            if (isTempSyn)
                synthesizer.Dispose();
            var playback = new MediaPlaybackItem(source);
            RegisterForWordBoundaryEvents(playback);
            return playback;
        }

        /// <summary>
        /// 检查当前阅读器的页标
        /// </summary>
        /// <param name="startPosition">起始位置（通常与SpeechCueChanged事件联用）</param>
        public void CheckCurrentReaderIndex(int? startPosition)
        {
            int index = _readerView.GetIndexFromStartOffset(startPosition ?? 0);
            if (index != _readerView.Index)
            {
                _readerView.Index = index;
            }
        }

        /// <summary>
        /// 获取当前已经加载的朗读流
        /// </summary>
        /// <returns></returns>
        public SpeechSynthesisStream GetCurrentSpeechStream()
        {
            return _tempSpeechStream;
        }
    }
}
