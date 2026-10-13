using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace DotGreaterPopup
{
    // Pass-through low-level keyboard watcher that reports plain Esc presses.
    // Same shape as DoubleRightClickTrigger: owns hook lifetime plus install
    // state only, raises an event marshaled to the UI thread, and always
    // calls CallNextHookEx so Esc keeps working everywhere else. The popup
    // itself can never see Esc because WS_EX_NOACTIVATE keeps focus away.
    public sealed class EscapeKeyWatcher : IDisposable
    {
        public event Action EscapePressed;

        private NativeMethods.LowLevelHookProc _hookProc;
        private IntPtr _hookId = IntPtr.Zero;
        private SynchronizationContext _uiContext;
        private bool _installed;
        private bool _disposed;

        public void Install()
        {
            if (_installed)
            {
                return;
            }

            _uiContext = SynchronizationContext.Current;
            _hookProc = HookCallback;
            _hookId = NativeMethods.SetWindowsHookEx(
                NativeMethods.WH_KEYBOARD_LL,
                _hookProc,
                Marshal.GetHINSTANCE(typeof(EscapeKeyWatcher).Module),
                0);
            if (_hookId == IntPtr.Zero)
            {
                _hookProc = null;
                throw new InvalidOperationException(
                    "SetWindowsHookEx(WH_KEYBOARD_LL) failed with Win32 error "
                    + Marshal.GetLastWin32Error() + ".");
            }
            _installed = true;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;

            if (_installed)
            {
                NativeMethods.UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
                _installed = false;
            }
            _hookProc = null;
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)NativeMethods.WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (vkCode == NativeMethods.VK_ESCAPE)
                {
                    Action handler = EscapePressed;
                    if (handler != null)
                    {
                        SynchronizationContext ui = _uiContext;
                        if (ui != null)
                        {
                            ui.Post(delegate(object state) { handler(); }, null);
                        }
                        else
                        {
                            handler();
                        }
                    }
                }
            }

            return NativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }
    }
}
