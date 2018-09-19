using ClothResorting.Helpers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClothResortingUnitTest.Helpers
{
    [TestFixture]
    class InventoryFeeCalculatorTests
    {
        [Test]
        [TestCase("2/3/2018", "2/8/2018", "02032018", "02082018", 1)]
        [TestCase("2/3/2018", "2/4/2018", "01032018", "02082018", 1)]
        [TestCase("2/3/2018", "2/12/2018", "02032018", "02122018", 2)]
        [TestCase("9/15/2018", null, "02032018", "02122019", 1)]
        public void CalculateNunmberOfWeek_WhenBillingDateIsTheSameAsInAndOutboundDate_ReturnWeeks(string inboundDate, string outboundDate, string lastBillingDate, string currentBillingDate, int expect)
        {
            var calculator = new InventoryFeeCalculator();

            var result = calculator.CalculateNunmberOfWeek(inboundDate, outboundDate, lastBillingDate, currentBillingDate);

            Assert.That(result, Is.EqualTo(expect));
        }
    }
}
