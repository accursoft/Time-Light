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
        FileSystemWatcher watcher;

        public View()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), Settings.Default.Ledger);

            if (!File.Exists(path)) {
                File.WriteAllText(path, Settings.Default.NewLedger);
                MessageBox.Show(string.Format(Settings.Default.CreatedLedger, path), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            //create model and controller
            ledger = new Ledger(path);
            timer = new Timer();
            controller = new Controller(ledger, timer);
            tray = new NotifyIcon() { Icon = Resources.Off };

            //subscribe to model
            timer.TimingChanged += timing => { tray.Icon = timing ? Resources.On : Resources.Off; };
            ledger.ActiveChanged += active => { tray.Text = active == null ? Application.ProductName : active; };
            ledger.LedgerChanged += CreateContextMenu;

            controller.LoadLedger();

            //watch for changes to the ledger
            watcher = new FileSystemWatcher(Path.GetDirectoryName(ledger.File), Path.GetFileName(ledger.File)) { NotifyFilter = NotifyFilters.LastWrite };

            watcher.Changed += (sender, e) => {
                BypassWatcher(delegate {
                    if (timer.Timing)
                        MessageBox.Show(Settings.Default.ChangedWhileTiming, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else {
                        try {
                            if (tray.ContextMenuStrip.InvokeRequired)
                                tray.ContextMenuStrip.Invoke(new Action(controller.LoadLedger));
                            else
                                controller.LoadLedger();
                        }
                        catch (Exception ex) {
                            Program.Handle(ex);
                            MessageBox.Show(Settings.Default.CannotLoad, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        tray.BalloonTipText = Settings.Default.Reloaded;
                        tray.ShowBalloonTip(1000);
                    }
                });
            };

            watcher.EnableRaisingEvents = true;

            //subscribe to mouse event
            tray.MouseClick += (sender, e) => {
                if (e.Button == MouseButtons.Left)
                    if (timer.Timing)
                        BypassWatcher(controller.Stop);
                    else {
                        if (ledger.Active == null)
                            MessageBox.Show(Settings.Default.NoActiveItem, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        else
                            controller.Start();
                    }
            };
            tray.Visible = true;
        }

        void BypassWatcher(Action action)
        {
            watcher.EnableRaisingEvents = false;
            action();
            watcher.EnableRaisingEvents = true;
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

            //exit
            tray.ContextMenuStrip.Items.Add(new ToolStripMenuItem(Settings.Default.Exit, null, delegate {
                if (timer.Timing)
                    BypassWatcher(controller.Stop);
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
            if (tray != null) {
                tray.Dispose();
                tray = null;
            }
            if (watcher != null) {
                watcher.Dispose();
                watcher = null;
            }
        }
    }
}