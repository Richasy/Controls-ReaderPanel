using System;
using Windows.UI.Xaml;

namespace Richasy.Controls.Reader.Models
{
    public class EventDelayer
    {
        private DispatcherTimer _timer;

        public bool IsPause { get; set; }

        public EventDelayer(double seconds) : this(TimeSpan.FromSeconds(seconds))
        {
        }

        public EventDelayer(TimeSpan interval)
        {
            _timer = new DispatcherTimer();
            _timer.Tick += _timer_Tick;
            Interval = interval;
        }

        public EventDelayer() : this(0.2)
        {
        }

        public TimeSpan Interval
        {
            get => _timer.Interval;
            set => _timer.Interval = value;
        }

        public bool ResetWhenDelayed { get; set; }

        public void Delay()
        {
            if (!_timer.IsEnabled)
            {
                _timer.Start();
            }
            else
            {
                if (ResetWhenDelayed)
                {
                    _timer.Stop();
                    _timer.Start();
                }
            }
        }


        private void _timer_Tick(object sender, object e)
        {
            if (_timer.IsEnabled)
            {
                _timer.Stop();
            }
            OnArrived();
        }

        public event EventHandler Arrived;
        protected void OnArrived()
        {
            if(!IsPause)
                Arrived?.Invoke(this, EventArgs.Empty);
        }

    }
}
