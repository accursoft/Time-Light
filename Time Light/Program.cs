using System;
using System.Windows.Forms;

namespace TimeLight
{
    static class Program
    {
        static void Main()
        {
            Application.EnableVisualStyles();
            using (new Gui())
            {
                Application.Run();
            }
        }
    }

    internal static class XmlFormat
    {
        public const string Node = "node";
        public const string Leaf = "leaf";
        public const string Total = "total";
        public const string Name = "name";
        public const string Billed = "billed";
        public const string Unbilled = "unbilled";
        public const string Default = "default";
    }
}