using System;
using Windows.Foundation;

namespace Richasy.Controls.Reader.Models
{
    public class PositionEventArgs:EventArgs
    {
        public Point Position { get; set; }
        internal PositionEventArgs()
        {

        }
        internal PositionEventArgs(Point point)
        {
            Position = point;
        }
    }
}
