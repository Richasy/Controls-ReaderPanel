using Richasy.Controls.Reader.Models;
using System;

namespace Richasy.Controls.Reader.Views
{
    public partial class ReaderView
    {
        public event EventHandler<LinkEventArgs> LinkTapped;
        public event EventHandler<ImageEventArgs> ImageTapped;
    }
}
