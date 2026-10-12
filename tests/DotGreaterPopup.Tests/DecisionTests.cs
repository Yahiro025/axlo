using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotGreaterPopup.Tests
{
    [TestClass]
    public class DecisionTests
    {
        [TestMethod]
        public void Fires_When_Fast_And_Close() =>
            Assert.IsTrue(DoubleClickDecision.ShouldFire(new Point(0, 0), 0, new Point(2, 2), 200, 500, 8));

        [TestMethod]
        public void Rejects_When_Slow() =>
            Assert.IsFalse(DoubleClickDecision.ShouldFire(new Point(0, 0), 0, new Point(2, 2), 900, 500, 8));

        [TestMethod]
        public void Rejects_When_Far() =>
            Assert.IsFalse(DoubleClickDecision.ShouldFire(new Point(0, 0), 0, new Point(100, 100), 200, 500, 8));

        [TestMethod]
        public void Charset_Has_Dot_And_Greater() =>
            CollectionAssert.AreEqual(new[] { '.', '>' }, CharSet.Chars);

        [TestMethod]
        public void Clamp_Keeps_Popup_Onscreen()
        {
            var p = PopupPosition.Clamp(new Point(1900, 1000), new Size(160, 80), new Rectangle(0, 0, 1920, 1040));
            Assert.IsTrue(p.X + 160 <= 1920 && p.Y + 80 <= 1040);
        }
    }
}
