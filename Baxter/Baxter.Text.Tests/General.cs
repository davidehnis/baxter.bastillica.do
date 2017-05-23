using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Baxter.Text.Tests
{
    [TestClass]
    public class GeneralTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            const string path = @"..\..\Artifacts\document-term-matric.txt";
            var dataTable = DataTable.New.ReadCsv(path);
            List<string> x = dataTable.Rows.Select(row => row["Text"]).ToList();
            double[] y = dataTable.Rows.Select(row => double.Parse(row["IsSunny"]))
                                       .ToArray();
        }
    }
}