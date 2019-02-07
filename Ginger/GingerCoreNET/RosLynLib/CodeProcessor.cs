#region License
/*
Copyright © 2014-2018 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.RosLynLib;
using Amdocs.Ginger.Repository;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
namespace GingerCoreNET.RosLynLib
{
    public class CodeProcessor
    {
      static  Regex Pattern = new Regex("{CS(\\s)*Exp(\\s)*=(\\s)*([a-zA-Z]|\\d)*\\((\")*([^\\)}\\({])*(\")*\\)}", RegexOptions.Compiled);
        public object EvalExpression(string expression)
        {
           
            string code = expression.Replace("{CS Eval(", "").Trim().Replace(")}", "");                      
            Stopwatch st = Stopwatch.StartNew();
            Task<object> task = EvalExpressionTask(code);
            task.Wait();
            st.Stop();
            Reporter.ToLog(eLogLevel.DEBUG, "Executed CodeProcessor - Elapsed: " + st.ElapsedMilliseconds + " ,Expression: " + expression + " ,Result: " + task.Result);
            return task.Result;
        }

        public static string GetResult(string Expression)
        {
            Pattern =   new Regex("{CS(\\s)*Exp(\\s)*=(\\s)*[^}]*}");
            Regex Clean =new  Regex("{CS(\\s)*Exp(\\s)*=");

            foreach (Match M in Pattern.Matches(Expression))
            {
                string match = M.Value;
                string exp = match;
                exp = exp.Replace(Clean.Match(exp).Value, "");
                //not doing string replacement to
                exp = exp.Remove(exp.Length-1);
                string Evalresult = exp;
                try
                {
                    Evalresult = CSharpScript.EvaluateAsync(exp).Result.ToString();
                }

                catch (Exception e)
                {
                    Console.Write(e.Message);
                }
                Expression = Expression.Replace(match, Evalresult);
            }

            return Expression;

        }

   
        public static bool EvalCondition(string condition)
        {
            bool result =(bool) CSharpScript.EvaluateAsync(condition).Result;
            return result;
        }

        // condition can be: 1=2 or complex like 1+3=5
        private async Task<bool> EvalConditionAsync(string condition)
        {
            // bool b;
            // if (1 == 1) b = true; else b = false;    

            var script = CSharpScript.
                Create<bool>("bool b;").
                ContinueWith("if (" + condition + ") b=true; else b=false;").   // check the condition
                ContinueWith("b");    // return the value of b

            return ((bool)(await script.RunAsync()).ReturnValue);            
        }



        public async Task<object> EvalExpressionTask(string expression)
        {
            var rc = await CSharpScript.EvaluateAsync(expression);
            return rc;
        }


        
        //!!!!!   Cleanup
        
        private static ScriptState<object> scriptState = null;
        public static object Execute(string code)
        {
            Console.WriteLine("Executing script code: " + code);

            // Add ref to DLLs needed
            ScriptOptions options = ScriptOptions.Default.AddReferences(Assembly.GetAssembly(typeof(PluginPackage)));

            //Globals to pass in vars
            GingerConsoleScriptGlobals globals = new GingerConsoleScriptGlobals();

            scriptState = scriptState == null ? CSharpScript.RunAsync(code, options, globals).Result : scriptState.ContinueWithAsync(code).Result;
            if (scriptState.ReturnValue != null && !string.IsNullOrEmpty(scriptState.ReturnValue.ToString()))
                return scriptState.ReturnValue;

            Console.WriteLine("Executing script code complete");
            
            //!!!
            globals.WaitforallNodesShutDown();

            return null;
        }


        //!!!!!   Cleanup
        public void runcode()
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(@"var x = new DateTime(2016,12,1);");
            Console.WriteLine(tree.ToString()); // new DateTime(2016,12,1)
            var result = Task.Run<object>(async () =>
            {
                Console.WriteLine("Enter expression:");
                string exp = Console.ReadLine();
                var rc = await CSharpScript.RunAsync(exp);
                Console.WriteLine(exp + "=" + rc.ReturnValue);

                var s = await CSharpScript.RunAsync(@"using System;");
                // continuing with previous evaluation state
                s = await s.ContinueWithAsync(@"var x = ""my/"" + string.Join(""_"", ""a"", ""b"", ""c"") + "".ss"";");
                s = await s.ContinueWithAsync(@"var y = ""my/"" + @x;");
                s = await s.ContinueWithAsync(@"y // this just returns y, note there is NOT trailing semicolon");
                // inspecting defined variables
                Console.WriteLine("inspecting defined variables:");
                foreach (var variable in s.Variables)
                {
                    Console.WriteLine("name: {0}, type: {1}, value: {2}", variable.Name, variable.Type.Name, variable.Value);
                }
                return s.ReturnValue;

            }).Result;

            Console.WriteLine("Result is: {0}", result);
        }

        public object RunCode2(string code)
        {            
            //SyntaxTree tree = CSharpSyntaxTree.ParseText(@"object result;");
            var result = Task.Run<object>(async () =>
            {                                
                var s = await CSharpScript.RunAsync(@"using System;");
                s = await s.ContinueWithAsync(@"object result;");
                // continuing with previous evaluation state
                s = await s.ContinueWithAsync(code);                
                s = await s.ContinueWithAsync("result");   // output result
                // inspecting defined variables
                Console.WriteLine("inspecting defined variables:");
                foreach (var variable in s.Variables)
                {
                    string varInfo = string.Format("name: {0}, type: {1}, value: {2}", variable.Name, variable.Type.Name, variable.Value);
                    Reporter.ToConsole(eLogLevel.DEBUG, varInfo);                    
                }
                return s.ReturnValue;

            }).Result;

            return result;
        }
    }
}
