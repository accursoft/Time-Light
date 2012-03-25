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
    class View : IDisposable
    {
        NotifyIcon tray;
        Ledger ledger;
        Timer timer;
        Controller controller;

        public View()
        {
            ledger = new Ledger(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), Settings.Default.Ledger));
            timer = new Timer();
            controller = new Controller(ledger, timer);
            tray = new NotifyIcon() { Icon = Resources.Off };

            timer.TimingChanged += timing => { tray.Icon = timing ? Resources.On : Resources.Off; };
            ledger.ActiveChanged += active => { tray.Text = active == null ? Application.ProductName : active; };
            ledger.LedgerChanged += CreateContextMenu;

            controller.LoadLedger();

            tray.MouseClick += (sender, e) => {
                if (e.Button == MouseButtons.Left)
                    if (timer.Timing)
                        controller.Stop();
                    else {
                        if (ledger.Active == null)
                            MessageBox.Show(Settings.Default.NoActiveItem, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        else
                            controller.Start();
                    }
            };
            tray.Visible = true;
        }

        #region menu
        void CreateContextMenu()
        {
            if (tray.ContextMenuStrip != null)
                tray.ContextMenuStrip.Dispose();

            //project tree
            tray.ContextMenuStrip = new ContextMenuStrip();
            tray.ContextMenuStrip.Items.AddRange(BuildChildMenu(ledger.Xml.Root).ToArray());
            tray.ContextMenuStrip.Items.Add(new ToolStripSeparator());

            //view ledger
            tray.ContextMenuStrip.Items.Add(new ToolStripMenuItem(Settings.Default.ViewLedger, null, delegate { Process.Start(Settings.Default.Viewer, ledger.File); }));

            //reload ledger
            tray.ContextMenuStrip.Items.Add(new ToolStripMenuItem(Settings.Default.LoadLedger, null, delegate {
                if (timer.Timing)
                    MessageBox.Show(Settings.Default.CannotLoad, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    controller.LoadLedger();
            }));

            //exit
            tray.ContextMenuStrip.Items.Add(new ToolStripMenuItem(Settings.Default.Exit, null, delegate {
                if (timer.Timing)
                    controller.Stop();
                Application.Exit();
            }));
        }

        IEnumerable<ToolStripMenuItem> BuildChildMenu(XElement element)
        {
            var leaves = element.Elements(XmlFormat.Leaf).Select(leaf => {
                ToolStripMenuItem item = new ToolStripMenuItem(XmlFormat.Name(leaf));
                item.Click += delegate { controller.Select(leaf); };
                return item;
            });
            var nodes = element.Elements(XmlFormat.Node).Select(node => new ToolStripMenuItem(XmlFormat.Name(node), null, BuildChildMenu(node).ToArray()));
            return nodes.Concat(leaves);
        }
        #endregion

        public void Dispose()
        {
            if (tray == null) return;

            tray.Dispose();
            tray = null;
        }
    }
}