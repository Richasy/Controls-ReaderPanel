using System;

namespace Richasy.Controls.Reader.Models
{
    public class LinkEventArgs : EventArgs
    {
        public string FileName { get; set; }
        public string Id { get; set; }
        public string Link { get; set; }
        public LinkEventArgs() { }
        public LinkEventArgs(string fileName, string id)
        {
            FileName = fileName;
            Id = id;
        }
    }
}
