using System.Windows.Forms;

namespace TimeLight
{
    static class Program
    {
        static void Main()
        {
            Application.EnableVisualStyles();
            using (new View()) Application.Run();
        }
    }
}