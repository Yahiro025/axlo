using System;
using System.Runtime.InteropServices;

namespace DotGreaterPopup
{
    public sealed class CharTyper
    {
        public void Validate(char c)
        {
            if (Array.IndexOf(CharSet.Chars, c) < 0)
            {
                throw new ArgumentException("Character '" + c + "' is not in CharSet.", "c");
            }
        }

        public void Insert(char c)
        {
            Validate(c);

            var inputs = new NativeMethods.INPUT[2];
            inputs[0] = MakeUnicodeInput(c, 0);
            inputs[1] = MakeUnicodeInput(c, NativeMethods.KEYEVENTF_KEYUP);
            uint sent = NativeMethods.SendInput(
                (uint)inputs.Length, inputs, Marshal.SizeOf(typeof(NativeMethods.INPUT)));
            if (sent != inputs.Length)
            {
                throw new InvalidOperationException(
                    "SendInput injected " + sent + " of " + inputs.Length
                    + " inputs (Win32 error " + Marshal.GetLastWin32Error()
                    + "). The target window may be elevated.");
            }
        }

        private static NativeMethods.INPUT MakeUnicodeInput(char c, uint extraFlags)
        {
            return new NativeMethods.INPUT
            {
                Type = NativeMethods.INPUT_KEYBOARD,
                U = new NativeMethods.InputUnion
                {
                    Ki = new NativeMethods.KEYBDINPUT
                    {
                        WVk = 0,
                        WScan = c,
                        DwFlags = NativeMethods.KEYEVENTF_UNICODE | extraFlags,
                        Time = 0,
                        DwExtraInfo = IntPtr.Zero,
                    },
                },
            };
        }
    }
}
