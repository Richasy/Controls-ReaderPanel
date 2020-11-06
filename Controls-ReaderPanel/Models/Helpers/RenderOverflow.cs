using System.Collections.Generic;
using Windows.UI.Xaml;

namespace Richasy.Controls.Reader.Models.Helpers
{
    public class RenderOverflow
    {
        public bool IsRendered { get; set; }
        public FrameworkElement Element { get; set; }
        public RenderOverflow()
        {

        }
        public RenderOverflow(bool isRendered, FrameworkElement ele)
        {
            IsRendered = isRendered;
            Element = ele;
        }

        public override bool Equals(object obj)
        {
            return obj is RenderOverflow overflow &&
                   EqualityComparer<FrameworkElement>.Default.Equals(Element, overflow.Element);
        }

        public override int GetHashCode()
        {
            return -703426257 + EqualityComparer<FrameworkElement>.Default.GetHashCode(Element);
        }
    }
}
