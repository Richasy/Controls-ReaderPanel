using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Richasy.Controls.Reader.Models
{
    public class InsideSearchItem
    {
        public Chapter Chapter { get; set; }
        public int Index { get; set; }
        public string SearchText { get; set; }
        public string DisplayText { get; set; }
    }
}
