using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

namespace DotGreaterPopup
{
    // System-wide double-right-click detector over a WH_MOUSE_LL hook.
    //
    // Pass-through only: the hook procedure never swallows input. Every path
    // returns CallNextHookEx, so the native context menu keeps working.
    //
    // State owned here is the hook lifetime plus the last right-button-up
    // (point + timestamp). All timing/distance math delegates to
    // DoubleClickDecision.ShouldFire. The event is marshalled to the thread
    // that called Install() via its SynchronizationContext (the UI thread in
    // the real app). No UI references in this file.
    //
    // Manual smoke check (no headless assert possible — WH_MOUSE_LL needs a
    // desktop with a message loop):
    //   1. Build the exe, run it, right-click ONCE in Notepad.
    //      Expected: native menu only, DoubleRightClicked never fires.
    //   2. Double-right-click fast at one spot.
    //      Expected: exactly one DoubleRightClicked with the cursor point.
    public sealed class DoubleRightClickTrigger : IDisposable
    {
        public event Action<Point> DoubleRightClicked;

        // Rooted so the GC never moves/collects the callback while the OS
        // holds its function pointer (see SetWindowsHookEx remarks).
        private NativeMethods.LowLevelMouseProc _hookProc;
        private IntPtr _hookHandle = IntPtr.Zero;
        private SynchronizationContext _uiContext;
        private bool _hasLastUp;
        private Point _lastPoint;
        private long _lastMs;
        private bool _disposed;

        public void Install()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("DoubleRightClickTrigger");
            }

            if (_hookHandle != IntPtr.Zero)
            {
                return;
            }

            _hookProc = HookProc;
            _uiContext = SynchronizationContext.Current;
            IntPtr module = Marshal.GetHINSTANCE(typeof(DoubleRightClickTrigger).Module);
            _hookHandle = NativeMethods.SetWindowsHookEx(
                NativeMethods.WH_MOUSE_LL, _hookProc, module, 0);

            if (_hookHandle == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                _hookProc = null;
                _uiContext = null;
                throw new InvalidOperationException(
                    "SetWindowsHookEx(WH_MOUSE_LL) failed with Win32 error " + error + ".");
            }
        }

        public void Dispose()
        {
            if (_hookHandle != IntPtr.Zero)
            {
                NativeMethods.UnhookWindowsHookEx(_hookHandle);
                _hookHandle = IntPtr.Zero;
            }

            _hookProc = null;
            _uiContext = null;
            _disposed = true;
        }

        private IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            // HC_ACTION (nCode >= 0) carries a mouse message in wParam.
            // WM_RBUTTONUP only: downs and moves never touch click state.
            if (nCode >= 0 && wParam == (IntPtr)NativeMethods.WM_RBUTTONUP)
            {
                Point point;
                if (NativeMethods.GetCursorPos(out point))
                {
                    // int wraps ~every 24.9 days; ShouldFire treats
                    // curMs < lastMs as "no fire" and state still advances.
                    long nowMs = Environment.TickCount;
                    int doubleMs = (int)NativeMethods.GetDoubleClickTime();
                    int dx = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXDOUBLECLK);
                    int dy = NativeMethods.GetSystemMetrics(NativeMethods.SM_CYDOUBLECLK);

                    // ShouldFire takes one square size; Max covers the system
                    // rect so no OS-valid double-click is ever missed.
                    if (_hasLastUp && DoubleClickDecision.ShouldFire(
                        _lastPoint, _lastMs, point, nowMs, doubleMs, Math.Max(dx, dy)))
                    {
                        Raise(point);
                    }

                    // First-ever click only stores state (_hasLastUp was false),
                    // so it can never fire.
                    _lastPoint = point;
                    _lastMs = nowMs;
                    _hasLastUp = true;
                }
            }

            // Pass-through always: never swallow right-clicks.
            return NativeMethods.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        private void Raise(Point point)
        {
            Action<Point> handler = DoubleRightClicked;
            if (handler == null)
            {
                return;
            }

            SynchronizationContext ctx = _uiContext;
            if (ctx == null)
            {
                handler(point);
            }
            else
            {
                ctx.Post(delegate(object state) { handler((Point)state); }, point);
            }
        }
    }
}
