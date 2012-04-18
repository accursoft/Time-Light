using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

using TimeLight.Properties;
using System.IO;

namespace TimeLight
{
    static class Program
    {
        static void Main()
        {
            Application.EnableVisualStyles();

            AppDomain.CurrentDomain.UnhandledException += (s, e) => { Handle((Exception)e.ExceptionObject); };
            try {
                using (new View()) Application.Run();
            }
            catch (Exception e) {
                Handle(e);
            }
        }

        static void Handle(Exception e)
        {
            //create error report
            StringBuilder s = new StringBuilder();
            Report(e, s);
            string report = Settings.Default.Error + s.ToString();
            Trace.TraceError(report);

            //open it in an editor
            string file = Path.GetTempFileName();
            File.WriteAllText(file, report);
            Process.Start(Settings.Default.Viewer, file);
        }

        static void Report(Exception e, StringBuilder s)
        {
            s.AppendLine(e.ToString());
            var e1 = e.InnerException;
            if (e1 != null)
                Report(e1, s);
        }
    }
}