using System;
using System.Drawing;

namespace DotGreaterPopup
{
    public static class PopupPosition
    {
        public static Point Clamp(Point want, Size popup, Rectangle workArea)
        {
            int x = Math.Max(workArea.Left, Math.Min(want.X, workArea.Right - popup.Width));
            int y = Math.Max(workArea.Top, Math.Min(want.Y, workArea.Bottom - popup.Height));
            return new Point(x, y);
        }
    }
}
