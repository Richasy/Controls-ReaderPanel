using Richasy.Controls.Reader.Enums;
using Richasy.Controls.Reader.Models;
using Windows.Storage;

namespace Richasy.Controls.Reader.Views
{
    public partial class EpubView : TxtView
    {
        private HtmlHelper helper;
        public EpubView()
        {
        }

        public new void SetContent(string content, ReaderStartMode mode = ReaderStartMode.First, int startLength = 0)
        {

        }
    }
}
