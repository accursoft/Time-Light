using System;
using System.Linq;
using System.Xml.Linq;
using System.Xml;

namespace TimeLight
{
    class Ledger
    {
        public string File { get; private set; }

        public XDocument Xml { get; private set; }
        public event Action LedgerChanged = delegate { };

        XElement active;
        public XElement Active
        {
            get { return active; }
            set
            {
                if (active != null)
                    active.Attribute(XmlFormat.Default).Remove();

                if ((active = value) != null)
                    active.SetAttributeValue(XmlFormat.Default, XmlFormat.Default);

                ActiveChanged(active == null ? null : XmlFormat.Name(active));
            }
        }
        public event Action<string> ActiveChanged = delegate { };

        public Ledger(string file)
        {
            File = file;
        }

        public void LoadLedger()
        {
            Active = null;

            Xml = XDocument.Load(File);

            //add missing total attributes
            foreach (var node in Xml.Descendants(XmlFormat.Node))
                if (node.Attribute(XmlFormat.Total) == null)
                    node.SetAttributeValue(XmlFormat.Total, TimeSpan.Zero);

            //add missing leaf values, search for default
            foreach (var node in Xml.Descendants(XmlFormat.Leaf)) {
                if (string.IsNullOrWhiteSpace(node.Value))
                    node.SetValue(TimeSpan.Zero);

                if (node.Attribute(XmlFormat.Default) != null) {
                    active = node;
                    ActiveChanged(XmlFormat.Name(node));
                }
            }

            LedgerChanged();
        }

        public void RecordTime(TimeSpan time)
        {
            Active.SetValue(XmlConvert.ToTimeSpan(Active.Value).Add(time));
            UpdateTotals(Active.Parent);
            Xml.Save(File);
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
    }
}