using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Threading.Tasks;
using System;

namespace Baxter.Agents.Automaton
{
    /// <summary>simplified scripting wrapper</summary>
    public static class Scripter
    {
        #region Public Methods
        public static ImmutableArray<Diagnostic> Compile(Script script)
        {
            var cSharpScript =
                CSharpScript.Create<int>(script.Code, globalsType: typeof(object));

            return cSharpScript.Compile(); //var results = cSharpScript.Compile();
        }

        public static Script CreateScript(string code)
        {
            return new
                Script(Guid.NewGuid(), code.Substring(0, code.Length >= 5 ? 5 : code.Length), code, DateTime.UtcNow);
        }

        public static ReturnType Run<ReturnType>(Script script)
        {
            var cSharpScript =
                CSharpScript.Create<ReturnType>(script.Code, globalsType: typeof(object));
            var results = cSharpScript.Compile();

            var task = Task.Factory.StartNew(async () => (await cSharpScript.RunAsync(new object())).ReturnValue);
            Task.WaitAll(task);
            return task.Result.Result;
        }
        #endregion Public Methods
    }
}