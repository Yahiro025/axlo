using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace DotGreaterPopup
{
    // Owns the HKCU Run entry only. Never touches HKLM, needs no admin.
    //
    // Value name is fixed ("DotGreaterPopup"); the value data is the quoted
    // path of this exe so Startup launches it after logon. Reads/writes go
    // through IsEnabled / SetEnabled so callers never touch the registry.
    public sealed class AutostartManager
    {
        public const string ValueName = "DotGreaterPopup";

        private const string RunKeyPath =
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        public bool IsEnabled
        {
            get
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false))
                {
                    return key != null && key.GetValue(ValueName) != null;
                }
            }
        }

        public void SetEnabled(bool enabled)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true))
            {
                if (key == null)
                {
                    throw new InvalidOperationException(
                        "HKCU Run key not found: " + RunKeyPath + ".");
                }

                if (enabled)
                {
                    key.SetValue(ValueName, "\"" + Application.ExecutablePath + "\"");
                }
                else
                {
                    key.DeleteValue(ValueName, false);
                }
            }
        }
    }
}
