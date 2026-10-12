using System;
using System.Drawing;
using System.Windows.Forms;

namespace DotGreaterPopup
{
    // Borderless tool-window popup with one button per CharSet.Chars entry.
    //
    // Never steals focus: ShowWithoutActivation returns true and the extended
    // style carries WS_EX_NOACTIVATE (plus TOOLWINDOW and TOPMOST), so showing
    // the popup leaves the foreground window (for example Notepad) active and
    // clicking a button does not activate this form either.
    //
    // The form owns visibility state only. The caller passes Action<char>
    // onPick; a button click invokes it and then hides the popup. Typing
    // itself lives in CharTyper; this form never touches SendInput.
    public sealed class DotGreaterPopupForm : Form
    {
        // Extended styles applied in CreateParams below.
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_TOPMOST = 0x00000008;

        // Popup origin offset from the cursor, in pixels.
        private const int OffsetX = 16;
        private const int OffsetY = 20;

        // Buttons stay comfortably clickable (brief requires at least 48px).
        private const int ButtonWidth = 64;
        private const int ButtonHeight = 48;

        private readonly Action<char> _onPick;

        // One-shot re-pin: the low-level hook fires before the target app
        // shows its own context menu for the same click, so a freshly shown
        // popup can end up underneath it. Reasserting topmost ~250ms later
        // puts us back above once the native menu has settled. Runs on the
        // UI thread; NOACTIVATE keeps focus where it was.
        private readonly Timer _repinTimer;
        private Point _lastCursor;

        public DotGreaterPopupForm(Action<char> onPick)
        {
            if (onPick == null)
            {
                throw new ArgumentNullException("onPick");
            }

            _onPick = onPick;

            FormBorderStyle = FormBorderStyle.None;
            TopMost = true;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            KeyPreview = true;

            var panel = new FlowLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.FlowDirection = FlowDirection.LeftToRight;
            panel.WrapContents = false;
            panel.Padding = new Padding(8);

            // One button per charset entry. Loop the single CharSet.Chars
            // list; never hardcode '.' / '>' here.
            foreach (char c in CharSet.Chars)
            {
                char ch = c;
                var button = new Button();
                button.Text = ch.ToString();
                button.Font = new Font(button.Font.FontFamily, 16f, FontStyle.Bold);
                button.Size = new Size(ButtonWidth, ButtonHeight);
                button.MinimumSize = new Size(ButtonWidth, ButtonHeight);
                button.Margin = new Padding(4);
                button.Click += delegate(object sender, EventArgs e)
                {
                    _onPick(ch);
                    HidePopup();
                };
                panel.Controls.Add(button);
            }

            Controls.Add(panel);

            // Fixed client size so ShowAt can clamp before first show.
            // Width: padding 8*2 + per button (margin 4 + 64 + margin 4) * 2 = 160.
            // Height: padding 8*2 + (margin 4 + 48 + margin 4) = 72.
            ClientSize = new Size(160, 72);

            Deactivate += delegate(object sender, EventArgs e)
            {
                HidePopup();
            };

            _repinTimer = new Timer();
            _repinTimer.Interval = 250;
            _repinTimer.Tick += delegate(object sender, EventArgs e)
            {
                _repinTimer.Stop();
                if (IsDisposed || Disposing || !Visible)
                {
                    return;
                }
                TryAnchorToMenu();
                NativeMethods.SetWindowPos(
                    Handle,
                    NativeMethods.HWND_TOPMOST,
                    0, 0, 0, 0,
                    NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE);
            };
        }

        // Move beside the native context menu when one is actually showing
        // near the cursor. System menus are #32768 windows; browsers and
        // other custom menus leave no such window, so the cursor offset
        // from ShowAt stands. The proximity guard ignores stale menus
        // from other apps.
        private void TryAnchorToMenu()
        {
            IntPtr menu = NativeMethods.FindWindow("#32768", null);
            if (menu == IntPtr.Zero)
            {
                return;
            }
            NativeMethods.RECT rect;
            if (!NativeMethods.GetWindowRect(menu, out rect))
            {
                return;
            }
            var menuRect = Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
            Rectangle near = menuRect;
            near.Inflate(300, 300);
            if (!near.Contains(_lastCursor))
            {
                return;
            }
            Rectangle workArea = Screen.FromPoint(_lastCursor).WorkingArea;
            Location = PopupPosition.PlaceBesideMenu(_lastCursor, Size, menuRect, workArea);
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST;
                return cp;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                HidePopup();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void ShowAt(Point cursor)
        {
            _lastCursor = cursor;
            Point want = new Point(cursor.X + OffsetX, cursor.Y + OffsetY);
            Rectangle workArea = Screen.FromPoint(cursor).WorkingArea;
            Location = PopupPosition.Clamp(want, Size, workArea);
            Show();
            _repinTimer.Stop();
            _repinTimer.Start();
        }

        public void HidePopup()
        {
            // Idempotent: hiding an already-hidden form is a safe no-op.
            _repinTimer.Stop();
            Hide();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repinTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
