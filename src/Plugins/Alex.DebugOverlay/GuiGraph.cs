using System.Linq;
using Alex.Utils.Collections;
using Microsoft.Xna.Framework;
using RocketUI;
using RocketUI.Utilities.Extensions;

namespace Alex.DebugOverlay
{
	public class GuiGraph : RocketElement
	{
		private ConcurrentDeck<Record> _history = new ConcurrentDeck<Record>(512);
		public GuiGraph()
		{
			
		}

		public void Add(double x, double y)
		{
			_history.Push(new Record(x,y));
		}
		
		/// <inheritdoc />
		protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
		{
			base.OnDraw(graphics, gameTime);

			var datapoints = _history.ReadDeck();

			if (datapoints.Length == 0)
				return;
			
			var maxYValue = datapoints.Max(xx => xx.Y);
			var minYValue = datapoints.Min(xx => xx.Y);
			
			var elementWidth = (float)Width / datapoints.Length;
			var elementHeight = (float)Height / (maxYValue);

			Vector2 previousPoint = Vector2.Zero;
			for (var index = 0; index < datapoints.Length ; index++)
			{
				var element = datapoints[index];
				var pos = new Vector2((float) (index* elementWidth),  (float) (element.Y * elementHeight));
				if (index > 0)
					graphics.DrawLine(RenderPosition + previousPoint,  RenderPosition + pos, Color.Green);
				previousPoint = pos;
			}
			
			graphics.DrawString(RenderBounds.TopLeft(), $"{maxYValue}", Color.White, FontStyle.None, Vector2.One);
			graphics.DrawString(RenderBounds.BottomLeft(), $"{minYValue}", Color.White, FontStyle.None, Vector2.One);
		}
		
		public struct Record
		{
			public double X;
			public double Y;

			public Record(double x, double y)
			{
				X = x;
				Y = y;
			}
		}
	}
}