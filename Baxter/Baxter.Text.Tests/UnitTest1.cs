using System;
using System.Collections.Generic;
using Gustav.Domain;
using javax.swing.text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Baxter.Text.Tests
{
    [TestClass]
    public class ParsingTests
    {
        [TestMethod]
        public void ComparingContentsToDetermineTypes()
        {
            // Arrange
            const string compare = @"sally drives with the roof down on sunny days.";
            //var content = AbstractDocument.Content(compare);

            // Act
            var distance = Reporting.Distance(compare);

            // Assert
            Assert.IsNotNull(distance);
        }

        [TestMethod]
        public void ComparingContentsToDetermineTypesSame()
        {
            // Arrange
            var content = ComparisonContentSame();

            // Act
            var distance = Reporting.Distance(content);

            // Assert
            Assert.IsNotNull(distance);
        }

        [TestMethod]
        public void ComparingContentsToDetermineTypesSlightly()
        {
            // Arrange
            var content = ComparisonContentSlightly();

            // Act
            var distance = Reporting.Distance(content);

            // Assert
            Assert.IsNotNull(distance);
        }

        [TestMethod]
#if (!LOCALDEVBUILD)
        [Ignore]
#endif
        public void FindSampleUsingFuzzyLibraryAsStringWithContent()
        {
            // Arrange
            var content = ComparisonContentSame();

            // Act
            var value = TextReviewer.Parse(content, "Melody*");

            // Assert
            Assert.IsTrue(value > 0);
        }

        private string ComparisonContentSame()
        {
            return @"Month End Reporting Worksheet

Month__________________________________________		Chambers>DDA>PSD

Detail Report: bob's your uncle

Main _______________
MRHA______________
Sec__________________
Add_________________ (Reinstatements)

Total________________
CC___________________
Email_______________

_____Term Report ___					Active Affiliates_________________
_____CC Application Report____
_____New Member Report___
_____Email Reports>Rachel
_____New Member Report>Darcy >>>Dora _____
_____Foundation Report >Cheryl Foundation Contributions> Cheryl ________
_____Buy Nearby Contributors mailing list to Rachel
_____Salesforce New Mbrs   _______Cancels >Dave
_____Micro Member list >Cheryl>Melody
*****************************************************************************
New Invoices       Members_____________ Invoice Total_____________________
Name - Melody Description - Works too much
_____Second Notices                         Members____________Total______________

_____Debit Bankcard accounts       Members____________Total______________
Welcome Letters
_____BES	_____CCS
_____MP	_____KJ
_____MD	_____IA

*****Add YTD Daily Membership Report to Month End Reports****

_____NSRA Remittance

_____ New Member report for each Rep
______Territory list for each Rep
";
        }

        private string ComparisonContentSlightly()
        {
            return @"Month End Reporting Worksheet

Month__________________________________________		Chambers>DDA>PSD

Detail Report:

Main _______________

Total________________
CC___________________
Email_______________

_____Term Report ___					Active Affiliates_________________
_____CC Application Report____
_____New Member Report___

_____Salesforce New Mbrs   _______Cancels >Dave
_____Micro Member list >Cheryl>Melody
*****************************************************************************
New Invoices       Members_____________ Invoice Total_____________________

_____Second Notices                         Members____________Total______________

_____Debit Bankcard accounts       Members____________Total______________
Welcome Letters
_____BES	_____CCS
_____MP	_____KJ
_____MD	_____IA

*****Add YTD Daily Membership Report to Month End Reports****

_____NSRA Remittance

_____ New Member report for each Rep
______Territory list for each Rep
";
        }

        private List<SampleData> TestDataContent()
        {
            var results = new List<SampleData>
            {
                new SampleData {Description = "testing in the forest", Id = "1", Name = "Bob"},
                new SampleData {Description = "testing in the forest", Id = "1", Name = "Rick"},
                new SampleData {Description = "testing in the forest", Id = "1", Name = "Richard"},
                new SampleData {Description = "testing in the forest", Id = "1", Name = "Melody Crenshaw-Hillsworth"}
            };

            return results;
        }
    }
}