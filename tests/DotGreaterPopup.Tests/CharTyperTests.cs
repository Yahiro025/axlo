using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotGreaterPopup.Tests
{
    [TestClass]
    public class CharTyperTests
    {
        [TestMethod] [ExpectedException(typeof(ArgumentException))]
        public void Rejects_Unknown_Char() => new CharTyper().Validate('a');

        [TestMethod]
        public void InputStruct_MatchesNativeSize()
        {
            int expected = Environment.Is64BitProcess ? 40 : 28;
            Assert.AreEqual(expected, Marshal.SizeOf(typeof(NativeMethods.INPUT)));
        }
    }
}
