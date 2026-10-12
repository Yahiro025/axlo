using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotGreaterPopup.Tests
{
    [TestClass]
    public class MenuAnchorTests
    {
        [TestMethod]
        public void Places_Right_Of_Menu_When_Room()
        {
            var p = PopupPosition.PlaceBesideMenu(
                new Point(100, 100), new Size(160, 72),
                new Rectangle(100, 100, 120, 200), new Rectangle(0, 0, 1920, 1040));
            Assert.AreEqual(new Point(228, 100), p);
        }

        [TestMethod]
        public void Places_Left_Of_Menu_When_No_Room_Right()
        {
            var p = PopupPosition.PlaceBesideMenu(
                new Point(1800, 200), new Size(160, 72),
                new Rectangle(1750, 100, 120, 300), new Rectangle(0, 0, 1920, 1040));
            Assert.AreEqual(new Point(1582, 200), p);
        }

        [TestMethod]
        public void Clamps_Y_Into_Work_Area()
        {
            var p = PopupPosition.PlaceBesideMenu(
                new Point(100, 1020), new Size(160, 72),
                new Rectangle(100, 800, 120, 200), new Rectangle(0, 0, 1920, 1040));
            Assert.AreEqual(new Point(228, 968), p);
        }
    }
}
