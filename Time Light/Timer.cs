using System;
using System.Linq;
using System.Xml.Linq;
using System.Diagnostics.Contracts;
using System.Diagnostics;
using System.Xml;

namespace TimeLight
{
    class Timer
    {
        DateTime start;
        static TraceSource tracer = new TraceSource("Log");

        public string File { get; private set; }
        public bool Timing { get; private set; }
        public XDocument Ledger { get; private set; }

        XElement active;
        public XElement Active {
            get { return active; }
            set {
                if (active != null)
                    active.Attribute(XmlFormat.Default).Remove();
                if ((active = value) != null)
                    active.SetAttributeValue(XmlFormat.Default, XmlFormat.Default);
            }
        }

        #region Load
        public Timer(string file)
        {
            File = file;
        }

        public void LoadLedger()
        {
            Contract.Requires(!Timing);

            Active = null;

            Ledger = XDocument.Load(File);

            //add missing total attributes
            foreach (var node in Ledger.Descendants(XmlFormat.Node))
                if (node.Attribute(XmlFormat.Total) == null)
                    node.SetAttributeValue(XmlFormat.Total, TimeSpan.Zero);

            //add missing leaf values, search for default
            foreach (var node in Ledger.Descendants(XmlFormat.Leaf))
            {
                if (string.IsNullOrWhiteSpace(node.Value)) 
                    node.SetValue(TimeSpan.Zero);

                if (node.Attribute(XmlFormat.Default) != null)
                    active = node;
            }
        }
        #endregion

        #region Timer
        public void Start()
        {
            Contract.Requires(Active != null);

            start = DateTime.Now;
            Timing = true;

            tracer.TraceInformation("Started " + Active.ToString());
        }

        public void Stop()
        {
            if (!Timing) return;

            Active.SetValue(XmlConvert.ToTimeSpan(Active.Value).Add(DateTime.Now - start));

            UpdateTotals(Active.Parent);

            Ledger.Save(File);
            Timing = false;

            tracer.TraceInformation("Stopped " + Active.ToString());
        }

        static void UpdateTotals(XElement node)
        {
            if (node == null || node.Name != XmlFormat.Node) return;

            TimeSpan total = node.Elements().Aggregate(TimeSpan.Zero, (subtotal, element) => subtotal.Add(XmlConvert.ToTimeSpan(element.Name == XmlFormat.Node ? element.Attribute(XmlFormat.Total).Value : element.Value)));

            node.SetAttributeValue(XmlFormat.Total, total);

            var billed = node.Attribute(XmlFormat.Billed);
            if (billed != null)
                node.SetAttributeValue(XmlFormat.Unbilled, total.TotalHours - XmlConvert.ToDouble(billed.Value));

            UpdateTotals(node.Parent);
        }
        #endregion
    }
}