using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Baxter.Agents.Automaton.Tests
{
    [TestClass]
    public class InitialTests
    {
        #region Public Methods
        [TestMethod]
        public void CreatedComplexScriptObjectScriptReturnsValidValue()
        {
            // Arrange
            var code = @"
using System;
using System.Collections.Generic;

var list = new List<int>();
for( int i = 0; i < 100; i++)
{
    list.Add(i);
}

return list;
";
            var script = Scripter.CreateScript(code);

            // Act
            var value = Scripter.Run<List<int>>(script);

            // Assert
            Assert.IsNotNull(value);
            Assert.IsTrue(value.Any());
            Assert.IsTrue(value.Count == 100);
        }
        [TestMethod]
        public void CreatedSimpleScriptObjectScriptReturnsValidValue()
        {
            // Arrange
            var script = Scripter.CreateScript("4+2");

            // Act
            var value = Scripter.Run<int>(script);

            // Assert
            Assert.IsTrue(value == 6);
        }
        [TestMethod]
        public void CreateHardCodedScriptReturnsValidValue()
        {
            // Arrange
            var script = CSharpScript.Create<int>("4+2", globalsType: typeof(object));
            var results = script.Compile();

            // Act
            var task = Task.Factory.StartNew(async () => (await script.RunAsync(new object())).ReturnValue);
            Task.WaitAll(task);
            var value = task.Result.Result;

            // Assert
            Assert.IsTrue(value == 6);
        }
        #endregion Public Methods

        #region Protected Methods
        protected void code()
        {
            var script = @"return 4+2;";
            // Act
            SyntaxTree tree = CSharpSyntaxTree.ParseText(script);
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var compilation = CSharpCompilation.Create("Hi")
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).AssemblyQualifiedName))
                .AddSyntaxTrees(tree);

            //compilation.
        }
        #endregion Protected Methods

        #region Private Methods
        private Script CreateScript(string code)
        {
            return new
                Script(Guid.NewGuid(), code.Substring(0, code.Length >= 5 ? 5 : code.Length), code, DateTime.UtcNow);
        }
        #endregion Private Methods
    }
}