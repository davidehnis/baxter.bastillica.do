using DataAccess;
using libsvm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Baxter.Vector.Machine;
using KernelType = libsvm.KernelType;

namespace Baxter.Text.Tests
{
    [TestClass]
    public class GeneralTests
    {
        [TestMethod]
        public void CompareKernelsVerifyEquality()
        {
            // Arrange
            const string path = @"..\..\Artifacts\sunnyData.txt";
            const string userInput = "sunny sunny rainy rainy rainy";

            // Act
            var baxter = Vector.Machine.KernelHelper.LinearKernel();
            var svm = libsvm.KernelHelper.LinearKernel();

            // Assert
            Assert.AreEqual(svm.Degree, baxter.Degree);
            Assert.AreEqual(svm.Gamma, baxter.Gamma);
            Assert.AreEqual(svm.KernelType, KernelType.LINEAR);
            Assert.AreEqual(baxter.KernelType, Vector.Machine.KernelType.Linear);
            Assert.AreEqual(svm.R, baxter.R);
        }

        [TestMethod]
        public void CompareModelsVerifyEquality()
        {
            // Arrange
            const string path = @"..\..\Artifacts\sunnyData.txt";

            // Act
            var baxter = BuildBaxterModel(path);
            var svm = BuildSvmModel(path);

            // Assert
            Assert.AreEqual(svm.l, baxter.L);
            Assert.AreEqual(svm.nr_class, baxter.NrClass);
        }

        [TestMethod]
        public void ComparePredictionsVerifyEquality()
        {
            // Arrange
            const string path = @"..\..\Artifacts\sunnyData.txt";
            const string userInput = "sunny sunny rainy rainy rainy";

            // Act
            var baxter = BaxterPrediction(path, userInput);
            var svm = SvmPrediction(path, userInput);

            // Assert
            Assert.AreEqual(svm, baxter);
        }

        [TestMethod]
        public void CompareTextClassifiersProblemsVerifyEquality()
        {
            // Arrange
            const string path = @"..\..\Artifacts\sunnyData.txt";

            // Act
            var baxter = BuildBaxterProblem(path);
            var svm = BuildSvmProblem(path);

            // Assert
            Assert.AreEqual(svm.l, baxter.L);
            Assert.AreEqual(svm.x.Length, baxter.X.Length);
            Assert.IsTrue(svm.x.Any());
            Assert.IsTrue(baxter.X.Any());
            Assert.AreEqual(svm.x[0].Length, baxter.X[0].Length);
            Assert.AreEqual(svm.x[0][0].value, baxter.X[0][0].Value);
            Assert.AreEqual(svm.x[1][0].value, baxter.X[1][0].Value);
            Assert.AreEqual(svm.x[2][0].value, baxter.X[2][0].Value);
            Assert.AreEqual(svm.x[3][0].value, baxter.X[3][0].Value);
        }

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

        private double BaxterPrediction(string trainingDataFilePath, string input)
        {
            var problem = BuildBaxterProblem(trainingDataFilePath);
            var vocabulary = BuildBaxterVocabulary(trainingDataFilePath);
            const int C = 1;
            var model = new Classifier(problem, Vector.Machine.KernelHelper.LinearKernel(), C);
            var newX = TextClassificationBuilder.CreateNode(input, vocabulary);

            return model.Predict(newX);
        }

        private Model BuildBaxterModel(string trainingDataFilePath)
        {
            var problem = BuildBaxterProblem(trainingDataFilePath);
            const int C = 1;
            var model = new Classifier(problem, Vector.Machine.KernelHelper.LinearKernel(), C);
            return model.Model;
        }

        private Problem BuildBaxterProblem(string trainingDataFilePath)
        {
            var dataTable = DataTable.New.ReadCsv(trainingDataFilePath);
            var x = dataTable.Rows.Select(row => row["Text"]).ToList();
            var y = dataTable.Rows.Select(row => double.Parse(row["IsSunny"])).ToArray();
            var vocabulary = x.SelectMany(GetWords).Distinct().OrderBy(word => word).ToList();
            var problemBuilder = new TextClassificationBuilder();
            return problemBuilder.CreateProblem(x, y, vocabulary.ToList());
        }

        private List<string> BuildBaxterVocabulary(string trainingDataFilePath)
        {
            var dataTable = DataTable.New.ReadCsv(trainingDataFilePath);
            var x = dataTable.Rows.Select(row => row["Text"]).ToList();
            var y = dataTable.Rows.Select(row => double.Parse(row["IsSunny"]))
                .ToArray();
            return x.SelectMany(GetWords).Distinct().OrderBy(word => word).ToList();
        }

        private svm_model BuildSvmModel(string trainingDataFilePath)
        {
            var problem = BuildSvmProblem(trainingDataFilePath);

            const int C = 1;
            var svc = new C_SVC(problem, libsvm.KernelHelper.LinearKernel(), C);
            return svc.model;
        }

        private svm_problem BuildSvmProblem(string trainingDataFilePath)
        {
            var dataTable = DataTable.New.ReadCsv(trainingDataFilePath);
            var x = dataTable.Rows.Select(row => row["Text"]).ToList();
            var y = dataTable.Rows.Select(row => double.Parse(row["IsSunny"])).ToArray();
            var vocabulary = x.SelectMany(GetWords).Distinct().OrderBy(word => word).ToList();
            var problemBuilder = new TextClassificationProblemBuilder();

            return problemBuilder.CreateProblem(x, y, vocabulary.ToList());
        }

        private double SvmPrediction(string trainingDataFilePath, string input)
        {
            var problem = BuildSvmProblem(trainingDataFilePath);
            var vocabulary = BuildBaxterVocabulary(trainingDataFilePath);
            const int C = 1;
            var model = new C_SVC(problem, libsvm.KernelHelper.LinearKernel(), C);
            var newX = TextClassificationProblemBuilder.CreateNode(input, vocabulary);

            return model.Predict(newX);
        }
    }
}