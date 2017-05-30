using DataAccess;
using libsvm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Baxter.Vector.Machine;

namespace Baxter.Text.Tests
{
    [TestClass]
    public class GeneralTests
    {
        [TestMethod]
        public void DataReader003OpensAndReadsContentsOfData()
        {
            // Arrange
            const string path = @"..\..\Artifacts\sunnyData.txt";
            var dataTable = DataTable.New.ReadCsv(path);
            List<string> x = dataTable.Rows.Select(row => row["Text"]).ToList();

            // Act
            double[] y = dataTable.Rows.Select(row => double.Parse(row["IsSunny"]))
                                       .ToArray();

            // Assert
            Assert.IsTrue(y.Any());
        }

        [TestMethod]
        public void DataReader004GeneratesVocabulary()
        {
            // Arrange
            const string path = @"..\..\Artifacts\sunnyData.txt";
            var dataTable = DataTable.New.ReadCsv(path);
            var x = dataTable.Rows.Select(row => row["Text"]).ToList();
            var y = dataTable.Rows.Select(row => double.Parse(row["IsSunny"]))
                .ToArray();

            // Act
            var vocabulary = x.SelectMany(GetWords).Distinct().OrderBy(word => word).ToList();

            // Assert
            Assert.IsTrue(vocabulary.Any());
        }

        [TestMethod]
        public void DataReader005GeneratesTextClassification()
        {
            // Arrange
            const string path = @"..\..\Artifacts\sunnyData.txt";
            var dataTable = DataTable.New.ReadCsv(path);
            var x = dataTable.Rows.Select(row => row["Text"]).ToList();
            var y = dataTable.Rows.Select(row => double.Parse(row["IsSunny"]))
                .ToArray();
            var vocabulary = x.SelectMany(GetWords).Distinct().OrderBy(word => word).ToList();
            var problemBuilder = new TextClassificationProblemBuilder();

            // Act
            var problem = problemBuilder.CreateProblem(x, y, vocabulary.ToList());

            // Assert
            Assert.IsNotNull(problem);
        }

        [TestMethod]
        public void DataReader006TrainsTextClassification()
        {
            // Arrange
            const string path = @"..\..\Artifacts\sunnyData.txt";
            var dataTable = DataTable.New.ReadCsv(path);
            var x = dataTable.Rows.Select(row => row["Text"]).ToList();
            var y = dataTable.Rows.Select(row => double.Parse(row["IsSunny"]))
                .ToArray();
            var vocabulary = x.SelectMany(GetWords).Distinct().OrderBy(word => word).ToList();
            var problemBuilder = new TextClassificationProblemBuilder();
            var problem = problemBuilder.CreateProblem(x, y, vocabulary.ToList());

            // Act
            const int C = 1;
            var model = new C_SVC(problem, libsvm.KernelHelper.LinearKernel(), C);

            // Assert
            Assert.IsNotNull(problem);
        }

        [TestMethod]
        public void DataReader007PredictsTextClassification()
        {
            // Arrange
            const string path = @"..\..\Artifacts\sunnyData.txt";
            var dataTable = DataTable.New.ReadCsv(path);
            var x = dataTable.Rows.Select(row => row["Text"]).ToList();
            var y = dataTable.Rows.Select(row => double.Parse(row["IsSunny"]))
                .ToArray();
            var vocabulary = x.SelectMany(GetWords).Distinct().OrderBy(word => word).ToList();
            var problemBuilder = new TextClassificationProblemBuilder();
            var problem = problemBuilder.CreateProblem(x, y, vocabulary.ToList());
            const int C = 1;
            var model = new C_SVC(problem, libsvm.KernelHelper.LinearKernel(), C);

            // Act
            string userInput = "sunny sunny rainy rainy rainy";
            var predictionDictionary = new Dictionary<int, string> { { -1, "Rainy" }, { 1, "Sunny" } };

            var newX = TextClassificationProblemBuilder.CreateNode(userInput, vocabulary);

            var predictedY = model.Predict(newX);
            var answer = predictionDictionary[(int)predictedY];

            // Assert
            Assert.AreEqual("Rainy", answer);
        }

        [TestMethod]
        public void DataReader007PredictsTextClassificationWithNewClasses()
        {
            // Arrange
            const string path = @"..\..\Artifacts\sunnyData.txt";
            var dataTable = DataTable.New.ReadCsv(path);
            var x = dataTable.Rows.Select(row => row["Text"]).ToList();
            var y = dataTable.Rows.Select(row => double.Parse(row["IsSunny"]))
                .ToArray();
            var vocabulary = x.SelectMany(GetWords).Distinct().OrderBy(word => word).ToList();
            var problemBuilder = new TextClassificationBuilder();
            var problem = problemBuilder.CreateProblem(x, y, vocabulary.ToList());
            const int C = 1;
            var model = new Classifier(problem, Vector.Machine.KernelHelper.LinearKernel(), C);

            // Act
            string userInput = "sunny sunny rainy rainy rainy";
            var predictionDictionary = new Dictionary<int, string> { { -1, "Rainy" }, { 1, "Sunny" } };

            var newX = TextClassificationBuilder.CreateNode(userInput, vocabulary);

            var predictedY = model.Predict(newX);
            var answer = predictionDictionary[(int)predictedY];

            // Assert
            Assert.AreEqual("Rainy", answer);
        }

        private static IEnumerable<string> GetWords(string x)
        {
            return x.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}