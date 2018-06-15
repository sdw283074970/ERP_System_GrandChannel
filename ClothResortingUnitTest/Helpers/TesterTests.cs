using System;
using System.Text;
using System.Collections.Generic;
using NUnit.Framework;
using ClothResorting.Helpers;

namespace ClothResortingUnitTest.Helpers
{

    [TestFixture]
    public class TesterTests
    {
        [Test]
        [TestCase("12-15", 12)]
        [TestCase("122", 122)]
        public void GetFrom_WhenInputString_ReturnCorrectNumber(string cartonNumberRange, int expect)
        {
            var test = new Tester();

            var result = test.GetFrom(cartonNumberRange);

            Assert.That(result, Is.EqualTo(expect));
        }

        [Test]
        [TestCase("12-15", 15)]
        [TestCase("122", 122)]
        public void GetTo_WhenInputString_ReturnCorrectNumber(string cartonNumberRange, int expect)
        {
            var test = new Tester();

            var result = test.GetTo(cartonNumberRange);

            Assert.That(result, Is.EqualTo(expect));
        }
    }
}
