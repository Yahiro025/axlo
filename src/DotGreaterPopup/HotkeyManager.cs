using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DotGreaterPopup
{
    // Owns the Win+Shift+J global hotkey lifetime only: registers on
    // construction, unregisters on dispose. Presses surface as HotkeyPressed; the caller
    // routes that to popup.ShowAt(Cursor.Position).
    //
    // The P/Invoke pair lives here (not in NativeMethods) so this class
    // fully owns Register/Unregister pairing. Messages land on a private
    // invisible window, so no visible form handle is required.
    public sealed class HotkeyManager : IDisposable
    {
        public const int MOD_WIN = 0x0008;
        public const int MOD_SHIFT = 0x0004;
        public const int VK_J = 0x4A;

        private const int WM_HOTKEY = 0x0312;
        private const int HotkeyId = 1;

        public event Action HotkeyPressed;

        private readonly HotkeyWindow _window;
        private bool _disposed;

        public HotkeyManager()
        {
            _window = new HotkeyWindow(OnHotkeyMessage);

            if (!RegisterHotKey(_window.Handle, HotkeyId, MOD_WIN | MOD_SHIFT, VK_J))
            {
                int error = Marshal.GetLastWin32Error();
                _window.DestroyHandle();
                throw new InvalidOperationException(
                    "RegisterHotKey(Win+Shift+J) failed with Win32 error " + error + ".");
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            UnregisterHotKey(_window.Handle, HotkeyId);
            _window.DestroyHandle();
            _disposed = true;
        }

        private void OnHotkeyMessage()
        {
            Action handler = HotkeyPressed;
            if (handler != null)
            {
                handler();
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // Invisible top-level window that receives WM_HOTKEY for this process.
        private sealed class HotkeyWindow : NativeWindow
        {
            private readonly Action _onHotkey;

            public HotkeyWindow(Action onHotkey)
            {
                _onHotkey = onHotkey;
                CreateHandle(new CreateParams());
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_HOTKEY && m.WParam == (IntPtr)HotkeyId)
                {
                    _onHotkey();
                }

                base.WndProc(ref m);
            }
        }
    }
}
