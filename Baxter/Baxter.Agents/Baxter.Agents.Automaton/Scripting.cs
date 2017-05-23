using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.CodeAnalysis.CSharp;

namespace Baxter.Agents.Automaton
{
    public class CSharpScriptEngine
    {
    }

    //scripting engine
    public class Scripting
    {
        //private static ScriptState<object> scriptState = null;

        //public static object Execute(string code)
        //{
        //    scriptState = scriptState == null ? CSharpScript.RunAsync(code).Result : scriptState.ContinueWithAsync(code).Result;
        //    if (scriptState.ReturnValue != null && !string.IsNullOrEmpty(scriptState.ReturnValue.ToString()))
        //        return scriptState.ReturnValue;
        //    return null;
        //}
    }
}