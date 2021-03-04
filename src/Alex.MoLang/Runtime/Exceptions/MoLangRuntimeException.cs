using System;
using System.Collections.Generic;
using System.Diagnostics;
using Alex.MoLang.Parser;
using Alex.MoLang.Parser.Tokenizer;

namespace Alex.MoLang.Runtime.Exceptions
{
	public class MoLangRuntimeException : Exception
	{
		public MoLangRuntimeException(string message, Exception baseException) : base(message, baseException)
		{
			
		}

		public MoLangRuntimeException(IExpression expression, string message, Exception baseException) : base(
			message, baseException)
		{
			
			//frames.Add(new StackFrame(null, ));
			
			/*do
			{
				if (expression.Attributes.TryGetValue("position", out var pos) && pos is TokenPosition tokenPosition)
				{
					var frame = new StackFrame(null, tokenPosition.LineNumber, tokenPosition.Index);
					
					frames.Add(frame);
				}
				
				expression = previousExpression;
			} while (expression.Attributes.TryGetValue("previous", out var prev)
			         && prev is IExpression previousExpression);*/

			//StackTrace st = new StackTrace;
			
			//st.GetFrames()
		}
		
		//private IEnumerable<IExpression> 
	}
}