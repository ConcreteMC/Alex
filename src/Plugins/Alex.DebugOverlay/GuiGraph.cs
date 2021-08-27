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
			this.ClipToBounds = false;
			BackgroundOverlay = Color.Black * 0.5f;
		}

		public void Add(double x, double y, Color color)
		{
			_history.Push(new Record(x,y, color));
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
			
			var elementWidth = (float)RenderBounds.Width / datapoints.Length;
			var elementHeight = (float)RenderBounds.Height / (maxYValue - minYValue);

			var     yPosTop       = RenderBounds.Top;

			var     basePosition          = RenderBounds.BottomLeft();
			Vector2 previousPoint = basePosition;
			for (var index = 0; index < datapoints.Length ; index++)
			{
				var element = datapoints[index];
				
				var pos = basePosition + 
				          new Vector2((float) (index* elementWidth),  -(float) ((element.Y - minYValue) * elementHeight));
				
				if (index > 0)
					graphics.DrawLine(previousPoint,  pos, element.Color);
				
				previousPoint = pos;
			}
			
			graphics.DrawString(RenderBounds.TopLeft(), $"{maxYValue}", Color.White, FontStyle.None, Vector2.One);
			graphics.DrawString(RenderBounds.BottomLeft(), $"{minYValue}", Color.White, FontStyle.None, Vector2.One);
		}
		
		public struct Record
		{
			public double X;
			public double Y;

			public Color Color;

			public Record(double x, double y, Color color)
			{
				X = x;
				Y = y;
				Color = color;
			}
		}
	}
}