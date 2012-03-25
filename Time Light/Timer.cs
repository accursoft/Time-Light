using System;

namespace TimeLight
{
    class Timer
    {
        DateTime start;

        bool timing;
        public bool Timing {
            get { return timing; }
            private set {
                timing = value;
                TimingChanged(value);
            }
        }
        public event Action<bool> TimingChanged = delegate { };

        public void Start()
        {
            start = DateTime.Now;
            Timing = true;
        }

        public TimeSpan Stop()
        {
            if (!Timing) throw new InvalidOperationException("The timer is not running.");

            Timing = false;
            return DateTime.Now - start;
        }
    }
}