using System;

namespace Richasy.Controls.Reader.Models
{
    public class ImageEventArgs : EventArgs
    {
        public string Base64 { get; set; }
        public string Tip { get; set; }
        public ImageEventArgs() { }
        public ImageEventArgs(string base64, string tip = "")
        {
            Base64 = base64;
            Tip = tip;
        }
    }
}
