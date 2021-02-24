using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alex.MoLang.Parser;
using Alex.MoLang.Parser.Tokenizer;
using Alex.MoLang.Runtime;

namespace Alex.MoLang.TestApp
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
			TokenIterator tokenIterator = new TokenIterator(@"
t.a = 213 + 2 / 0.5 + 5 + 2 * 3;

array.test.0 = 100;
array.test[1] = 200;
array.test[2] = 10.5;

for_each(v.r, array.test, {
  t.a = t.a + v.r;
});

loop(10, {
  t.a = this->t.a + math.cos(270);
});

return t.a + 100;");
			MoLangParser  parser        = new MoLangParser(tokenIterator);
			
			MoLangRuntime runtime       = new MoLangRuntime();
			var           expressions   = parser.Parse();
			//JsonSerializer.Serialize(expressions);
			Console.WriteLine("Eval Test Result: " + runtime.Execute(expressions).AsDouble());
		}
	}
}