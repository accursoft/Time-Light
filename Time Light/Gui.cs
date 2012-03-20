using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Diagnostics;

using TimeLight.Properties;

namespace TimeLight
{
    class Gui : IDisposable
    {
        NotifyIcon tray;
        Timer timer;

        #region initialisation
        public Gui()
        {
            timer = new Timer(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), Settings.Default.Ledger));
            tray = new NotifyIcon() { Icon = Resources.Off };
            init();

            tray.MouseClick += (sender, e) => {
                if (e.Button == MouseButtons.Left)
                    if (timer.Timing)
                        Stop();
                    else
                        Start();
            };
            tray.Visible = true;
        }

        void init()
        {
            timer.LoadLedger();
            CreateContextMenu();
            
            XElement item = timer.Active;
            tray.Text = item == null ? Application.ProductName : Name(item);
        }
        #endregion

        #region menu
        void CreateContextMenu()
        {
            if (tray.ContextMenuStrip != null)
                tray.ContextMenuStrip.Dispose();

            //project tree
            tray.ContextMenuStrip = new ContextMenuStrip();
            tray.ContextMenuStrip.Items.AddRange(BuildChildMenu(timer.Ledger.Root).ToArray());
            tray.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            
            //view ledger
            tray.ContextMenuStrip.Items.Add(new ToolStripMenuItem(Settings.Default.ViewLedger, null, delegate { Process.Start(Settings.Default.Viewer, timer.File); }));

            //reload ledger
            tray.ContextMenuStrip.Items.Add(new ToolStripMenuItem(Settings.Default.LoadLedger, null, delegate {
                if (timer.Timing)
                {
                    MessageBox.Show(Settings.Default.CannotLoad, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                init();
            }));

            //exit
            tray.ContextMenuStrip.Items.Add(new ToolStripMenuItem(Settings.Default.Exit, null, delegate {
                timer.Stop();
                Application.Exit();
            }));
        }

        IEnumerable<ToolStripMenuItem> BuildChildMenu(XElement element)
        {
            var leaves = element.Elements(XmlFormat.Leaf).Select(leaf =>
            {
                ToolStripMenuItem item = new ToolStripMenuItem(Name(leaf));
                item.Click += delegate { Select(leaf); };
                return item;
            });
            var nodes = element.Elements(XmlFormat.Node).Select(node => new ToolStripMenuItem(Name(node), null, BuildChildMenu(node).ToArray()));
            return nodes.Concat(leaves);
        }

        void Select(XElement item)
        {
            if (timer.Active == item)
            {
                if (timer.Timing)
                    Stop();
                else
                    Start();
                return;
            }

            if (timer.Timing) Stop();

            timer.Active = item;
            tray.Text = Name(item);
            Start();
        }
        #endregion

        #region start/stop
        void Start()
        {
            if (timer.Active == null)
            {
                MessageBox.Show(Settings.Default.NoActiveItem, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            timer.Start();
            tray.Icon = Resources.On;
        }

        void Stop()
        {
            timer.Stop();
            tray.Icon = Resources.Off;
        }
        #endregion

        /// <summary>Extract an element's name attribute</summary>
        static string Name(XElement element)
        {
            return element.Attribute(XmlFormat.Name).Value;
        }

        public void Dispose()
        {
            if (tray == null) return;

            tray.Dispose();
            tray = null;
        }
    }
}