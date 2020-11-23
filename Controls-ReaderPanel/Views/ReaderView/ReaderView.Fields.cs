using Richasy.Controls.Reader.Enums;
using Richasy.Controls.Reader.Models;
using Richasy.Controls.Reader.Models.Epub;

namespace Richasy.Controls.Reader.Views
{
    public partial class ReaderView
    {
        private HtmlHelper helper;
        public EpubBook Book { get; set; }
        public ReaderType ReaderType { get; set; }
    }
}
