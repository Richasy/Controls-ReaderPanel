using System;
using Windows.Devices.Input;
using Windows.Foundation;

namespace Richasy.Controls.Reader.Models
{
    public class PositionEventArgs : EventArgs
    {
        public PointerDeviceType DeviceType { get; set; }
        public Point Position { get; set; }
        internal PositionEventArgs()
        {

        }
        internal PositionEventArgs(Point point, PointerDeviceType type)
        {
            Position = point;
            DeviceType = type;
        }
    }
}
