using System;
using System.Diagnostics;
using System.Xml.Linq;

namespace TimeLight
{
    class Controller
    {
        readonly Ledger ledger;
        readonly Timer timer;

        static TraceSource tracer = new TraceSource("Log");

        public Controller(Ledger ledger, Timer timer)
        {
            this.ledger = ledger;
            this.timer = timer;
        }

        public void Start()
        {
            if (ledger.Active == null) throw new InvalidOperationException("There is no active item");

            timer.Start();

            tracer.TraceInformation("Started " + ledger.Active);
        }

        public void Stop()
        {
            ledger.RecordTime(timer.Stop());

            tracer.TraceInformation("Stopped " + ledger.Active);
        }

        public void Select(XElement item)
        {
            if (ledger.Active == item) {
                if (timer.Timing)
                    Stop();
                else
                    Start();
                return;
            }

            if (timer.Timing) Stop();

            ledger.Active = item;
            Start();
        }

        public void LoadLedger()
        {
            if (timer.Timing) throw new InvalidOperationException("Cannot load a new ledger while timing");

            ledger.LoadLedger();

            tracer.TraceInformation("Loaded " + ledger.File);
        }
    }
}