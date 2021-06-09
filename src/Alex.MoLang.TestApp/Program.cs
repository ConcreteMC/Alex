using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Alex.MoLang.Parser;
using Alex.MoLang.Parser.Tokenizer;
using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Exceptions;
using Alex.MoLang.Runtime.Struct;
using Alex.MoLang.Runtime.Value;
using NLog;

namespace Alex.MoLang.TestApp
{
	class Program
	{
		private static Logger Log = LogManager.GetCurrentClassLogger(typeof(Program));
		static void Main(string[] args)
		{
			//LoggerSetup.ConfigureNLog( Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), DefaultConfig);
			Stopwatch     sw      = Stopwatch.StartNew();
			TokenIterator tokenIterator = new TokenIterator(@"t.a = 213 + 2 / 0.5 + 5 + 2 * 3;

array.test.0 = 100;
array.test[1] = 200;
array.test[2] = 10.5;

for_each(v.r, array.test, {
  t.a = t.a + v.r;
query.debug_output('hello1', t.a, v.r);
});

loop(10, {
  t.a = t.a + math.cos((Math.PI / 180.0f) * 270);
query.debug_output('hello', 'test', t.a, array.test[2]);
});

return t.a;");
			MoLangParser  parser        = new MoLangParser(tokenIterator);
			var           expressions   = parser.Parse();
			
			var timeElapsedOnParsing = sw.Elapsed;
			
			Console.WriteLine($"Parser completed in {timeElapsedOnParsing.TotalMilliseconds}ms");
			
			MoLangRuntime runtime = new MoLangRuntime();

			var queryStruct = new QueryStruct(
				new KeyValuePair<string, Func<MoParams, object>>[]
				{
					new(
						"life_time", moParams =>
						{
							return new DoubleValue(sw.Elapsed.TotalSeconds);
						})
				});

			queryStruct.UseNLog = false;
			queryStruct.EnableDebugOutput = false;
			
			runtime.Environment.Structs.TryAdd("query", queryStruct);
			
			try
			{
				const int runs = 100000;
				double totalTicks = 0;
				
				double longest = 0;
				double shortest =long.MaxValue;
				
				IMoValue value;
				for (int i = 0; i < runs; i++)
				{
					sw.Restart();
					value = runtime.Execute(expressions);
					
					var elapsed = sw.Elapsed.TotalMilliseconds;

					if (elapsed > longest)
						longest = elapsed;
					else if (elapsed < shortest)
						shortest = elapsed;
					
					totalTicks += elapsed;
				}

				var timeElapsedOnExecution = sw.Elapsed;
				
				Console.WriteLine($"Execution: Avg={((double)totalTicks) / (double)runs}ms Max={(longest)}ms Min={(shortest)}ms");
			}
			catch (MoLangRuntimeException runtimeException)
			{
				Console.WriteLine($"Runtime exception: {runtimeException.MolangTrace}");
				Console.WriteLine(runtimeException.InnerException.ToString());
			}
			/*int _frames = 0;
			while (sw.Elapsed < TimeSpan.FromSeconds(10))
			{
				Console.WriteLine($"[{_frames}]: " + runtime.Execute(expressions).AsDouble());
				_frames++;
				
				Thread.Sleep(13);
			}*/
		}

		private const string DefaultConfig =
			"<?xml version=\"1.0\" encoding=\"utf-8\" ?><nlog xmlns=\"http://www.nlog-project.org/schemas/NLog.xsd\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><variable name=\"basedir\" value=\"${basedir}\" /><targets><target name=\"colouredConsole\" xsi:type=\"ColoredConsole\" useDefaultRowHighlightingRules=\"false\"layout=\"${pad:padding=5:inner=${level:uppercase=true}}|${callsite:className=true:includeSourcePath=false:methodName=false:includeNamespace=false}|${message} ${exception:format=tostring}\" ><highlight-row condition=\"level == LogLevel.Debug\" foregroundColor=\"DarkGray\" /><highlight-row condition=\"level == LogLevel.Info\" foregroundColor=\"Gray\" /><highlight-row condition=\"level == LogLevel.Warn\" foregroundColor=\"Yellow\" /><highlight-row condition=\"level == LogLevel.Error\" foregroundColor=\"Red\" /><highlight-row condition=\"level == LogLevel.Fatal\" foregroundColor=\"Red\" backgroundColor=\"White\" /></target></targets><rules><logger name=\"*\" minlevel=\"Debug\" writeTo=\"colouredConsole\" /></rules></nlog>";
	}
}