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

        // Position beside a measured native menu rect: right side preferred,
        // left side when the right overflows the work area. Vertical anchor
        // stays near the cursor, then both axes clamp into the work area.
        public static Point PlaceBesideMenu(
            Point cursor, Size popup, Rectangle menuRect, Rectangle workArea)
        {
            const int Gap = 8;
            int x = menuRect.Right + Gap;
            if (x + popup.Width > workArea.Right)
            {
                x = menuRect.Left - Gap - popup.Width;
            }
            return Clamp(new Point(x, cursor.Y), popup, workArea);
        }
    }
}
