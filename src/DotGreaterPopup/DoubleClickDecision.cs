using System;
using System.Drawing;

namespace DotGreaterPopup
{
    public static class DoubleClickDecision
    {
        public static bool ShouldFire(Point lastPt, long lastMs, Point curPt, long curMs, int dblTimeMs, int dblSizePx)
        {
            if (curMs < lastMs)
            {
                return false;
            }

            if (curMs - lastMs > dblTimeMs)
            {
                return false;
            }

            return Math.Abs(curPt.X - lastPt.X) <= dblSizePx
                && Math.Abs(curPt.Y - lastPt.Y) <= dblSizePx;
        }
    }
}
