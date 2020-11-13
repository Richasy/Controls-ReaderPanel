using System;

namespace Richasy.Controls.Reader.Models
{
    public class ChapterDetail
    {
        public int Index { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public ChapterDetail()
        {

        }

        public ChapterDetail(int index, string title, string content)
        {
            Index = index;
            Title = title;
            Content = content;
        }

        public override bool Equals(object obj)
        {
            return obj is ChapterDetail detail &&
                   Index == detail.Index;
        }

        public override int GetHashCode()
        {
            return -2134847229 + Index.GetHashCode();
        }

        public string GetReadContent()
        {
            string bookContent = Title + "\n" + Content;
            bookContent = bookContent.Replace("\r", "\n");
            bookContent = bookContent.Replace("\n\n", "\n");
            bookContent = bookContent.Replace("\n", Environment.NewLine);
            return bookContent;
        }
    }
}
