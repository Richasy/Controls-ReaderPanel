using HtmlAgilityPack;
using Richasy.Controls.Reader.Models;
using Richasy.Controls.Reader.Models.Epub;
using Richasy.Controls.Reader.Models.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;

namespace Richasy.Controls.Reader
{
    public partial class ReaderPanel
    {
        #region Internal Methods
        private bool IsTitle(string line)
        {
            var numberRegex = new Regex("^[第]?[0-9零一二三四五六七八九十百千]+$");
            string title = "";
            bool result = false;
            result = ChapterDivisionRegex.IsMatch(line);
            title = ChapterDivisionRegex.Match(line).Value;

            if (result)
            {
                if (title.Length > MAX_CHAPTER_LENGTH)
                    return false;
                int temp_index = line.IndexOf(title);
                if (temp_index > 0)
                {
                    string prev = line.Substring(0, temp_index);
                    // 此举是为了检测是否有类似章节名的内容
                    for (int i = 0; i < prev.Length; i++)
                    {
                        if (prev[i] >= 0x4e00 && prev[i] <= 0x9fbb)
                        {
                            return false;
                        }
                    }
                }
                //int index = 0;
                //for (int i = 0; i < title.Length; i++)
                //{
                //    if (ChapterEndKey.Any(p => p == title[i].ToString()))
                //    {
                //        index = i;
                //        break;
                //    }
                //}
                //if (index <= temp_index)
                //    index = line.Length;
                //string chapter = title.Substring(temp_index, index - 1).Trim();
                //if (!numberRegex.IsMatch(chapter))
                //{
                //    result = false;
                //}
            }
            return result;
        }

        private bool IsExtra(string line)
        {
            if (line.Length > MAX_CHAPTER_LENGTH)
                return false;

            for (int i = 0; i < ChapterExtraKey.Length; i++)
            {
                if (line.StartsWith(ChapterExtraKey[i]))
                {
                    return true;
                }
            }
            return false;
        }

        private Chapter GetLastEpubChapter(EpubTextFile chapter)
        {
            var orders = _epubContent.SpecialResources.HtmlInReadingOrder;
            Chapter lastChapter = null;
            foreach (var header in Chapters)
            {
                var corr = orders.Where(p => p.AbsolutePath == header.Link).FirstOrDefault();
                if (corr != null)
                {
                    int index = orders.IndexOf(corr);
                    int currentIndex = orders.IndexOf(chapter);
                    if (currentIndex >= index)
                        lastChapter = header;
                    else
                        break;
                }
            }
            return lastChapter;
        }
        #endregion

        #region Chapter Methods
        /// <summary>
        /// 用于对TXT文件进行分章
        /// </summary>
        /// <param name="book">TXT文件</param>
        /// <returns></returns>
        private async Task<List<Chapter>> GetTxtChapters(StorageFile book)
        {
            if (book == null)
                throw new ArgumentNullException();
            int count = 0;
            List<Chapter> chapters = new List<Chapter>();
            var total = await GetTxtContent(book);
            int number = 1;
            Chapter chapter = new Chapter();
            chapter.Title = book.DisplayName.Replace(".txt", "", StringComparison.OrdinalIgnoreCase);
            chapter.Index = 0;
            chapter.StartLength = 0;
            chapters.Add(chapter);
            StringBuilder builder = new StringBuilder();
            int parseLength = 0;
            foreach (var line in total.Split(Environment.NewLine))
            {
                if (string.IsNullOrEmpty(line.Trim()))
                {
                    parseLength += line.Length + Environment.NewLine.Length;
                    continue;
                }
                if (number >= 1)
                {
                    string title = "";
                    if (IsExtra(line))
                        title = line;
                    else if (IsTitle(line))
                        title = ChapterDivisionRegex.Match(line).Value;
                    if (!string.IsNullOrEmpty(title))
                    {
                        count++;
                        parseLength += builder.ToString().Length;
                        builder.Clear();
                        chapter = new Chapter(count, title, parseLength);
                        chapters.Add(chapter);
                        number = 0;
                    }
                }

                builder.Append(line);
                parseLength += Environment.NewLine.Length;
                number++;
                if (number >= MAX_TXT_PARSE_LINES)
                {
                    //为了避免某个文档一直没有匹配到新章节而不停的向StringBuilder中添加内容,导致内存溢出，这里对StringBuilder的大小进行了一定的限制
                    //即解析的行数达到一定的数目之后，即使没有匹配到新章节也将StringBuilder清空，同时更新parseLength。
                    //注意：这个数目的设定会影响到解析的时间，请谨慎设置!!!!
                    parseLength += builder.ToString().Length;
                    builder.Clear();
                    number = 1;
                }
            }

            return chapters;
        }

        /// <summary>
        /// 转化Epub书籍的章节信息
        /// </summary>
        /// <param name="book">Epub书籍</param>
        /// <returns></returns>
        private List<Chapter> GetEpubChapters(EpubBook book)
        {
            if (book == null)
                throw new ArgumentNullException();
            var headers = book.TableOfContents;
            var chapters = new List<Chapter>();
            AddEpubChapter(headers, chapters, new HtmlDocument());
            int index = 0;
            foreach (var chapter in chapters)
            {
                chapter.Index = index;
                index++;
            }
            return chapters;

            void AddEpubChapter(IList<EpubChapter> headerList, List<Chapter> titles, HtmlDocument document, int level = 1)
            {
                for (int i = 0; i < headerList.Count; i++)
                {
                    var file = book.Resources.Html.Where(p => p.AbsolutePath.Equals(headerList[i].AbsolutePath, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    string plainText = file.TextContent;
                    document.LoadHtml(HtmlHelper.GetBodyString(plainText));
                    plainText = document.DocumentNode.SelectSingleNode("//body").InnerHtml;
                    int basicStart = 0;
                    int readOrderIndex = book.SpecialResources.HtmlInReadingOrder.IndexOf(book.SpecialResources.HtmlInReadingOrder.FirstOrDefault(p => p.AbsolutePath.Equals(headerList[i].AbsolutePath)));
                    if (readOrderIndex > 0)
                    {
                        for (int j = 0; j < readOrderIndex; j++)
                        {
                            basicStart += book.SpecialResources.HtmlInReadingOrder[j].TextContent.Length;
                        }
                    }
                    int hashIndex = 0;
                    if (file != null)
                    {
                        if (!string.IsNullOrEmpty(headerList[i].HashLocation) && plainText.Contains("id=\"" + headerList[i].HashLocation + "\""))
                        {
                            var last = titles.Where(p => p.LinkEqual(headerList[i].AbsolutePath)).LastOrDefault();
                            int tempHashIndex = last?.HashIndex ?? 0;
                            hashIndex = plainText.IndexOf("id=\"" + headerList[i].HashLocation + "\"");
                        }
                    }
                    var info = new Chapter(headerList[i], i, basicStart, level, hashIndex);
                    titles.Add(info);
                    if (headerList[i].SubChapters != null && headerList[i].SubChapters.Count > 0)
                    {
                        AddEpubChapter(headerList[i].SubChapters, titles, document, level + 1);
                    }
                }
            }
        }
        #endregion

        #region Other Methods
        private async Task<string> GetTxtContent(StorageFile book)
        {
            string bookContent = "";
            using (var stream = await book.OpenStreamForReadAsync())
            {
                var encoding = EncodingHelper.DetectFileEncoding(stream, Encoding.UTF8);
                stream.Seek(0, SeekOrigin.Begin);
                var _bookReader = new StreamReader(stream, encoding);
                bookContent = await _bookReader.ReadToEndAsync();
                bookContent = bookContent.Replace("\r", "\n");
                bookContent = bookContent.Replace("\n\n", "\n");
                bookContent = bookContent.Replace("\n", Environment.NewLine);
            }
            return bookContent;
        }

        private void SetProgress(Chapter chapter, int addonLength = 0)
        {
            if (Chapters == null || Chapters.Count == 0 || !Chapters.Any(p => p.Equals(chapter)))
                throw new ArgumentOutOfRangeException("The chapter list don't have this chapter");
            if (chapter != CurrentChapter)
                ChapterChanged?.Invoke(this, chapter);
            CurrentChapter = Chapters.Where(p => p.Equals(chapter)).FirstOrDefault();
            if (ReaderType == Enums.ReaderType.Txt)
            {
                int nextIndex = CurrentChapter.Index + 1;
                string content = "";
                if (nextIndex >= Chapters.Count)
                    content = _txtContent.Substring(CurrentChapter.StartLength);
                else
                    content = _txtContent.Substring(CurrentChapter.StartLength, Chapters[nextIndex].StartLength - CurrentChapter.StartLength);
                _txtView.SetContent(content, Enums.ReaderStartMode.First, addonLength);
            }
            else if (ReaderType == Enums.ReaderType.Custom)
            {
                var detail = CustomChapterDetailList.Where(p => p.Index == CurrentChapter.Index).FirstOrDefault();
                if (detail != null)
                    _txtView.SetContent(detail.Content, Enums.ReaderStartMode.First, addonLength);
                else
                    CustomContentRequest?.Invoke(this, new CustomRequestEventArgs(Enums.ReaderStartMode.First, CurrentChapter, addonLength));
            }
            else
            {
                var info = _epubContent.SpecialResources.HtmlInReadingOrder.Where(p => p.AbsolutePath.Equals(chapter.Link, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                string content = string.Empty;
                if (info != null)
                {
                    content = info.TextContent;
                    _tempEpubChapterIndex = _epubContent.SpecialResources.HtmlInReadingOrder.IndexOf(info);
                }
                else
                    content = chapter.Title;
                _epubView.SetContent(content, Enums.ReaderStartMode.First, addonLength);
            }
            RaiseProgressChanged(addonLength);
        }
        #endregion
    }
}
