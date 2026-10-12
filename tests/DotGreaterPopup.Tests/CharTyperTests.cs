using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotGreaterPopup.Tests
{
    [TestClass]
    public class CharTyperTests
    {
        [TestMethod] [ExpectedException(typeof(ArgumentException))]
        public void Rejects_Unknown_Char() => new CharTyper().Validate('a');
    }
}
