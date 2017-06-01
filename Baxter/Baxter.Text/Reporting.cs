using System;
using System.Linq;

namespace Baxter.Text
{
    /// <summary>Month End Reporting Worksheet</summary>
    public class Reporting
    {
        private const int _tolerationMax = 650;
        public string ActiveAffiliates { get; set; }

        public string NewInvoicesMembers { get; set; }

        public string NewInvoicesTotal { get; set; }

        public string SecondNoticesMembers { get; set; }

        public string SecondNoticesTotal { get; set; }

        public static int Distance(string compared)
        {
            return LevenshteinDistance.Compute(compared, Standard());
        }

        public static string ExtractValue(string content, string find, string end)
        {
            var startTag = find;
            var startIndex = content.IndexOf(startTag, StringComparison.Ordinal) + startTag.Length;
            var endIndex = content.IndexOf(end, startIndex, StringComparison.Ordinal);
            return RemoveNoise(content.Substring(startIndex, endIndex - startIndex));
        }

        public static bool Is(string content)
        {
            return Distance(content) <= _tolerationMax;
        }

        public static string[] Noise()
        {
            return new[]
            {
                @"_"
            };
        }

        public static string RemoveNoise(string content)
        {
            return Noise().Aggregate(content, (current, noise) => current.Replace(noise, "").Trim());
        }

        private static string Standard()
        {
            return @"Month End Reporting Worksheet

Month__________________________________________		Chambers>DDA>PSD

Detail Report:

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
    }
}