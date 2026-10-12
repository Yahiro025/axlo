using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: InternalsVisibleTo("DotGreaterPopup.Tests")]

namespace DotGreaterPopup
{
    // Shared Win32 declarations. Task 2 owns the SendInput block below;
    // Task 3 appends the low-level mouse-hook declarations.
    internal static class NativeMethods
    {
        public const int INPUT_KEYBOARD = 1;
        public const uint KEYEVENTF_KEYUP = 0x0002;
        public const uint KEYEVENTF_UNICODE = 0x0004;

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public uint Type;
            public InputUnion U;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT Mi;
            [FieldOffset(0)]
            public KEYBDINPUT Ki;
        }

        // Present so the union spans the native size (MOUSEINPUT is the
        // largest member). Without it Marshal.SizeOf(INPUT) is 32, but
        // SendInput requires the native 40 on x64 and fails the call.
        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int Dx;
            public int Dy;
            public uint MouseData;
            public uint DwFlags;
            public uint Time;
            public IntPtr DwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public ushort WVk;
            public ushort WScan;
            public uint DwFlags;
            public uint Time;
            public IntPtr DwExtraInfo;
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        // ---- Task 3: low-level mouse hook (pass-through) ----
        public const int WH_MOUSE_LL = 14;
        public const int WM_RBUTTONUP = 0x0205;
        public const int SM_CXDOUBLECLK = 36;
        public const int SM_CYDOUBLECLK = 37;

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate IntPtr LowLevelHookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(
            int idHook, LowLevelHookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(
            IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern uint GetDoubleClickTime();

        // ---- Popup z-order re-pin (stays above native context menus) ----
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOACTIVATE = 0x0010;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(
            IntPtr hWnd, IntPtr hWndInsertAfter,
            int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);

        // ---- Task: Esc-to-close watcher (pass-through keyboard hook) ----
        public const int WH_KEYBOARD_LL = 13;
        public const int WM_KEYDOWN = 0x0100;
        public const int VK_ESCAPE = 0x1B;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out Point pt);
    }
}
