using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baxter.Text
{
    public class Svm
    {
        public void Load()
        {
            const string dataFilePath = @"D:\sunnyData.csv";
            var dataTable = DataTable.New.ReadCsv(dataFilePath);
            List<string> x = dataTable.Rows.Select(row => row["Text"]).ToList();
            double[] y = dataTable.Rows.Select(row => double.Parse(row["IsSunny"]))
                                       .ToArray();
        }
    }
}