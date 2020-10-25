using Richasy.Controls.Reader.Enums;
using Richasy.Controls.Reader.Models;
using Richasy.Controls.Reader.Models.Epub;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Richasy.Controls.Reader.Views
{
    public partial class EpubView : TxtView
    {
        private HtmlHelper helper;
        public EpubBook Book { get; set; }
        public EpubView()
        {
        }

        public new void SetContent(string content, ReaderStartMode mode = ReaderStartMode.First, int startLength = 0)
        {
            
        }
    }
}
