using Richasy.Controls.Reader.Models.Epub;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Richasy.Controls.Reader.Models
{
    public class Chapter
    {
        /// <summary>
        /// 章节序列号
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 起始字数
        /// </summary>
        public int StartLength { get; set; }
        /// <summary>
        /// 链接
        /// </summary>
        public string Link { get; set; }
        /// <summary>
        /// 哈希
        /// </summary>
        public string Hash { get; set; }
        /// <summary>
        /// 层级
        /// </summary>
        public int Level { get; set; }
        /// <summary>
        /// 哈希标识
        /// </summary>
        public int HashIndex { get; set; }
        public Chapter()
        {

        }

        public Chapter(int index, string title, int startLength)
        {
            Index = index;
            Title = title;
            StartLength = startLength;
        }
        public Chapter(int index, string title, string link)
        {
            Index = index;
            Title = title;
            Link = link;
        }

        public Chapter(EpubChapter item, int index, int startLength, int level = 0, int hashIndex = 0)
        {
            Index = index;
            Title = item.Title;
            Link = item.AbsolutePath;
            StartLength = startLength;
            Hash = item.HashLocation;
            Level = level;
            HashIndex = hashIndex;
        }

        public override string ToString()
        {
            return "[Index = " + Index + ",Title = " + Title + ",StartLength = " + StartLength + "]";
        }

        public bool LinkEqual(string link)
        {
            var regex = new Regex(@"#(.*)");
            string thisLink = regex.Replace(Link, "");
            link = regex.Replace(link, "");
            return thisLink == link;
        }

        public override bool Equals(object obj)
        {
            return obj is Chapter info &&
                   Index == info.Index;
        }

        public override int GetHashCode()
        {
            return -1590218067 + EqualityComparer<int>.Default.GetHashCode(Index);
        }
    }
}
