using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace DotGreaterPopup
{
    // Thin composition root: single-instance Mutex, then trigger + typer +
    // popup wired to the same ShowAt, a Win+J fallback hotkey, and a tray
    // icon (Show now / Start with Windows / Exit).
    //
    // Runs under an ApplicationContext (not Application.Run(form)) so the
    // popup starts hidden while a live message loop keeps the low-level
    // hook and the hotkey window working. trigger.Install() is called on
    // this UI thread so hook events marshal back here.
    internal static class Program
    {
        private const string MutexName = @"Local\DotGreaterPopup";

        [STAThread]
        private static void Main()
        {
            bool createdNew;
            using (var mutex = new Mutex(true, MutexName, out createdNew))
            {
                if (!createdNew)
                {
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                using (var context = new TrayContext())
                {
                    Application.Run(context);
                }
            }
        }

        private sealed class TrayContext : ApplicationContext
        {
            private readonly DoubleRightClickTrigger _trigger = new DoubleRightClickTrigger();
            private readonly CharTyper _typer = new CharTyper();
            private readonly AutostartManager _autostart = new AutostartManager();
            private readonly DotGreaterPopupForm _popup;
            private readonly HotkeyManager _hotkey;
            private readonly NotifyIcon _icon;
            private readonly ToolStripMenuItem _autostartItem;
            private bool _exiting;

            public TrayContext()
            {
                // Declared before wiring: onPick closes the popup first so
                // the char lands in the window under the cursor, then types.
                // HidePopup is idempotent (the form hides itself too).
                DotGreaterPopupForm popup = null;
                popup = new DotGreaterPopupForm(delegate(char c)
                {
                    popup.HidePopup();
                    _typer.Insert(c);
                });
                _popup = popup;

                _trigger.DoubleRightClicked += delegate(Point pt)
                {
                    _popup.ShowAt(pt);
                };
                _trigger.Install();

                // Fallback trigger: Win+J opens the same popup at the cursor.
                // If registration fails, the app still runs on double-click.
                HotkeyManager hotkey = null;
                try
                {
                    hotkey = new HotkeyManager();
                    hotkey.HotkeyPressed += delegate
                    {
                        _popup.ShowAt(Cursor.Position);
                    };
                }
                catch (InvalidOperationException)
                {
                    hotkey = null;
                }
                _hotkey = hotkey;

                var menu = new ContextMenuStrip();

                var showItem = new ToolStripMenuItem("Show now");
                showItem.Click += delegate
                {
                    _popup.ShowAt(Cursor.Position);
                };

                _autostartItem = new ToolStripMenuItem("Start with Windows");
                _autostartItem.CheckOnClick = true;
                _autostartItem.Checked = _autostart.IsEnabled;
                _autostartItem.Click += delegate
                {
                    try
                    {
                        _autostart.SetEnabled(_autostartItem.Checked);
                    }
                    catch (Exception ex)
                    {
                        _autostartItem.Checked = _autostart.IsEnabled;
                        MessageBox.Show(
                            "Could not update autostart: " + ex.Message,
                            "DotGreaterPopup",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                };

                var exitItem = new ToolStripMenuItem("Exit");
                exitItem.Click += delegate { Exit(); };

                menu.Items.Add(showItem);
                menu.Items.Add(_autostartItem);
                menu.Items.Add(new ToolStripSeparator());
                menu.Items.Add(exitItem);

                // Refresh the check mark each time the menu opens, in case
                // the Run value changed outside this session.
                menu.Opening += delegate
                {
                    _autostartItem.Checked = _autostart.IsEnabled;
                };

                _icon = new NotifyIcon();
                _icon.Icon = SystemIcons.Application;
                _icon.Text = "DotGreaterPopup";
                _icon.ContextMenuStrip = menu;
                _icon.Visible = true;

                if (_hotkey == null)
                {
                    _icon.ShowBalloonTip(
                        5000,
                        "DotGreaterPopup",
                        "Win+J fallback unavailable; double-right-click still works.",
                        ToolTipIcon.Warning);
                }
            }

            private void Exit()
            {
                if (_exiting)
                {
                    return;
                }
                _exiting = true;

                // Ordered teardown: icon first (off the tray), then hook,
                // then hotkey, then the popup.
                _icon.Visible = false;
                _icon.Dispose();
                _trigger.Dispose();
                if (_hotkey != null)
                {
                    _hotkey.Dispose();
                }
                _popup.Dispose();

                ExitThread();
            }
        }
    }
}
