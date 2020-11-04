using Richasy.Controls.Reader.Models;
using System;
using Windows.UI.Xaml;

namespace Richasy.Controls.Reader.Views
{
    public partial class EpubView
    {
        public event EventHandler<LinkEventArgs> LinkTapped;
        public event EventHandler<ImageEventArgs> ImageTapped;
    }
}
