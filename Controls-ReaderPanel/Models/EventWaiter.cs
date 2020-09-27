using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Richasy.Controls.Reader.Models
{
    public class EventWaiter
    {
        private DateTime _lastTime;

        public EventWaiter(double seconds)
        {
            Interval = TimeSpan.FromSeconds(seconds);
        }

        public EventWaiter(TimeSpan interval)
        {
            Interval = interval;
        }

        public EventWaiter()
        {
            Interval = TimeSpan.FromSeconds(0.1d);
        }

        public TimeSpan Interval { get; set; }

        public bool IsEnabled
        {
            get
            {
                if (DateTime.Now - _lastTime > Interval)
                {
                    _lastTime = DateTime.Now;
                    return true;
                }
                return false;
            }
        }

        public void Reset()
        {
            _lastTime = DateTime.Now;
        }
    }
}
