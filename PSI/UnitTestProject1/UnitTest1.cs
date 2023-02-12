using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using TD2;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            int i = new Random().Next();
            byte[] binaire = Utils.ToBinary(i);
            int entier = Utils.ToInt(binaire);
            Assert.AreEqual(entier, i);
        }
    }
}
