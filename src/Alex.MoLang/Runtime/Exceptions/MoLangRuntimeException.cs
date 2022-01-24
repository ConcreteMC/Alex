using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Alex.MoLang.Parser;
using Alex.MoLang.Parser.Tokenizer;

namespace Alex.MoLang.Runtime.Exceptions
{
	public class MoLangRuntimeException : Exception
	{
		public string MolangTrace { get; }

		public MoLangRuntimeException(string message, Exception baseException) : base(message, baseException)
		{
			MolangTrace = "Unknown";
		}

		public MoLangRuntimeException(IExpression expression, string message, Exception baseException) : base(
			message, baseException)
		{
			StringBuilder sb = new StringBuilder();

			do
			{
				if (expression.Meta?.Token?.Position != null)
				{
					var token = expression.Meta.Token;
					var tokenPosition = token.Position;

					sb.Append(
						$"at <{tokenPosition.LineNumber}:{tokenPosition.Index}> near {token.Type.TypeName} \"{token.Text}\"");
					//var frame = new StackFrame(null, tokenPosition.LineNumber, tokenPosition.Index);

					//	frames.Add(frame);
				}

				expression = expression.Meta.Parent;
			} while (expression?.Meta?.Parent != null);

			MolangTrace = sb.ToString();
			//st.GetFrames()
		}

		//private IEnumerable<IExpression> 
	}
}