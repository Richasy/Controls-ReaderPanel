using Richasy.Controls.Reader.Enums;
using Richasy.Controls.Reader.Models;
using Richasy.Controls.Reader.Models.Epub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Richasy.Controls.Reader.Views
{
    public partial class ReaderView
    {
        private HtmlHelper helper;
        public EpubBook Book { get; set; }
        public ReaderType ReaderType { get; set; }
    }
}
