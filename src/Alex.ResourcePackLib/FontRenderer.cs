using System;
using System.Drawing;
using System.Drawing.Imaging;
using Alex.API;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Alex.ResourcePackLib
{
    public class FontRenderer : IFontRenderer
	{
	    public const string Characters = "\u00c0\u00c1\u00c2\u00c8\u00ca\u00cb\u00cd\u00d3\u00d4\u00d5\u00da\u00df\u00e3\u00f5\u011f\u0130\u0131\u0152\u0153\u015e\u015f\u0174\u0175\u017e\u0207\u0000\u0000\u0000\u0000\u0000\u0000\u0000 !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~\u0000\u00c7\u00fc\u00e9\u00e2\u00e4\u00e0\u00e5\u00e7\u00ea\u00eb\u00e8\u00ef\u00ee\u00ec\u00c4\u00c5\u00c9\u00e6\u00c6\u00f4\u00f6\u00f2\u00fb\u00f9\u00ff\u00d6\u00dc\u00f8\u00a3\u00d8\u00d7\u0192\u00e1\u00ed\u00f3\u00fa\u00f1\u00d1\u00aa\u00ba\u00bf\u00ae\u00ac\u00bd\u00bc\u00a1\u00ab\u00bb\u2591\u2592\u2593\u2502\u2524\u2561\u2562\u2556\u2555\u2563\u2551\u2557\u255d\u255c\u255b\u2510\u2514\u2534\u252c\u251c\u2500\u253c\u255e\u255f\u255a\u2554\u2569\u2566\u2560\u2550\u256c\u2567\u2568\u2564\u2565\u2559\u2558\u2552\u2553\u256b\u256a\u2518\u250c\u2588\u2584\u258c\u2590\u2580\u03b1\u03b2\u0393\u03c0\u03a3\u03c3\u03bc\u03c4\u03a6\u0398\u03a9\u03b4\u221e\u2205\u2208\u2229\u2261\u00b1\u2265\u2264\u2320\u2321\u00f7\u2248\u00b0\u2219\u00b7\u221a\u207f\u00b2\u25a0\u0000";

		public int FONT_HEIGHT = 8;

		private byte[] GlyphWidth { get; }
	    private int[] CharWidth { get; set; } = null;
		private Texture2D FontTexture { get; set; }
	    private bool Unicode { get; }

	//	protected float PosX;
	 //   protected float PosY;

	    private int textColor;
	    private bool randomStyle;
	    private bool boldStyle;
	    private bool italicStyle;
	    private bool underlineStyle;
	    private bool strikethroughStyle;

	    private Microsoft.Xna.Framework.Color TextColor = Microsoft.Xna.Framework.Color.Black;

	    private Random fontRandom;
	    private Texture2D[] GlyphTextures = new Texture2D[256];
		    //	private Vector2 Scale { get; set; } = Vector2.One;

		private McResourcePack ResourcePack { get; }

	    private static Texture2D BitmapToTexture2D(GraphicsDevice device, Bitmap bmp)
	    {
		    uint[] imgData = new uint[bmp.Width * bmp.Height];
		    Texture2D texture = new Texture2D(device, bmp.Width, bmp.Height);

		    unsafe
		    {
			    BitmapData origdata =
				    bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);

			    uint* byteData = (uint*)origdata.Scan0;

			    for (int i = 0; i < imgData.Length; i++)
			    {
				    var val = byteData[i];
				    imgData[i] = (val & 0x000000FF) << 16 | (val & 0x0000FF00) | (val & 0x00FF0000) >> 16 | (val & 0xFF000000);
			    }

			    byteData = null;

			    bmp.UnlockBits(origdata);
		    }

		    texture.SetData(imgData);

		    return texture;
	    }

		public FontRenderer(bool unicode, McResourcePack resourcepack, byte[] glyphWidth)
		{
			ResourcePack = resourcepack;
			fontRandom = new Random();

		    Unicode = unicode;

		   // FontTexture = fontTexture;

			GlyphWidth = glyphWidth;
	    }

		private int[] ReadFontTexture()
	    {
		    int width = FontTexture.Width;
		    int height = FontTexture.Height;
		    int[] rgb = new int[width * height];
		    FontTexture.GetData(rgb);

		    int lvt_6_1_ = height / 16;
		    int lvt_7_1_ = width / 16;
		    bool lvt_8_1_ = true;
		    float lvt_9_1_ = 8.0F / (float)lvt_7_1_;

		    int[] charWidth = new int[256];
		    for (int i = 0; i < charWidth.Length; i++)
		    {
			    int j1 = i % 16;
			    int k1 = i / 16;

			    if (i == 32)
			    {
				    charWidth[i] = 4;
			    }

			    int l1;

			    for (l1 = lvt_7_1_ - 1; l1 >= 0; --l1)
			    {
				    int i2 = j1 * lvt_7_1_ + l1;
				    bool flag1 = true;

				    for (int j2 = 0; j2 < lvt_6_1_ && flag1; ++j2)
				    {
					    int k2 = (k1 * lvt_7_1_ + j2) * width;

					    if ((rgb[i2 + k2] >> 24 & 255) != 0)
					    {
						    flag1 = false;
					    }
				    }

				    if (!flag1)
				    {
					    break;
				    }
			    }

			    ++l1;
			    charWidth[i] = (int)(0.5D + (double)((float)l1 * lvt_9_1_)) + 1;
		    }

		    return charWidth;
	    }

		private float RenderChar(SpriteBatch sb, char ch, bool italic, Vector2 pos, Vector2 scale)
		{
			if (FontTexture == null)
			{
				if (ResourcePack.TryGetTexture("font/ascii", out Bitmap b))
				{
					FontTexture = BitmapToTexture2D(sb.GraphicsDevice, b);
				}
				else
				{
					return 0f;
				}
			}

			if (CharWidth == null)
			{
				CharWidth = ReadFontTexture();
			}

			if (ch == ' ')
			{
				return (Unicode ? 8.0F : 4.0f);
			}
			else
			{
				int i = Characters.IndexOf(ch);
				return i != -1 && !this.Unicode ? this.RenderDefaultChar(sb, i, italic, pos, scale) : this.RenderUnicodeChar(sb, ch, italic, pos, scale);
			}
		}

	    private Texture2D LoadGlyphTexture(GraphicsDevice g, int i)
	    {
		    if (GlyphTextures[i] == null)
		    {
			    if (ResourcePack.TryGetTexture($"font/unicode_page_{i:x2}", out Bitmap bmp))
			    {
				    Texture2D t = BitmapToTexture2D(g, bmp);
				    GlyphTextures[i] = t;

				    return t;
			    }
			}
		    else
		    {
			    return GlyphTextures[i];

		    }

		    return FontTexture;
	    }

	    private Vector2 RenderStringAtPos(SpriteBatch sb, string text, bool shadow, Vector2 pos, Vector2 scale)
		{
			Vector2 size = new Vector2();
			for (int i = 0; i < text.Length; i++)
			{
				char c0 = text[i];

				if (c0 == 167 && i + 1 < text.Length)
				{
					char colorSymb = text[i + 1].ToString().ToLower()[0];
					var ccc = "0123456789abcdefklmnor";
					int i1 = ccc.IndexOf(colorSymb);

					if (i1 < 16)
					{
						this.randomStyle = false;
						this.boldStyle = false;
						this.strikethroughStyle = false;
						this.underlineStyle = false;
						this.italicStyle = false;

						if (i1 < 0 || i1 > 15)
						{
							i1 = 15;
						}

						if (shadow)
						{
							i1 += 16;
						}

						var c = API.Utils.TextColor.GetColor(colorSymb);
						
						uint j1 = c.ForegroundColor.PackedValue;
						this.textColor = (int)j1;

						TextColor = c.ForegroundColor;
						//Color(new Color((uint) j1), TextColor.A);
					}
					else if (i1 == 16)
					{
						this.randomStyle = true;
					}
					else if (i1 == 17)
					{
						this.boldStyle = true;
					}
					else if (i1 == 18)
					{
						this.strikethroughStyle = true;
					}
					else if (i1 == 19)
					{
						this.underlineStyle = true;
					}
					else if (i1 == 20)
					{
						this.italicStyle = true;
					}
					else if (i1 == 21)
					{
						this.randomStyle = false;
						this.boldStyle = false;
						this.strikethroughStyle = false;
						this.underlineStyle = false;
						this.italicStyle = false;
						TextColor = new Color((uint) textColor);
					}

					i++;
				}
				else
				{
					int j = Characters.IndexOf(c0);

					if (this.randomStyle && j != -1)
					{
						int k = this.GetCharWidth(c0);
						char c1;

						while (true)
						{
							j = this.fontRandom.Next(Characters.Length);
							c1 = Characters[j];//.charAt(j);

							if (k == this.GetCharWidth(c1))
							{
								break;
							}
						}

						c0 = c1;
					}

					float f1 = this.Unicode ? 0.5F : 1.0F;
					bool flag = (c0 == 0 || j == -1 || this.Unicode) && shadow;

					if (flag)
					{
						pos.X -= f1;
						pos.Y -= f1;
					}

					float f = this.RenderChar(sb, c0, this.italicStyle, pos, scale);

					if (flag)
					{
						pos.X += f1;
						pos.Y += f1;
					}

					if (this.boldStyle)
					{
						pos.X += f1;

						if (flag)
						{
							pos.X -= f1;
							pos.Y -= f1;
						}

						this.RenderChar(sb, c0, this.italicStyle, pos, scale);
						pos.X -= f1;

						if (flag)
						{
							pos.X += f1;
							pos.Y += f1;
						}

						++f;
					}

					/*if (this.strikethroughStyle)
					{
						Tessellator tessellator = Tessellator.getInstance();
						BufferBuilder bufferbuilder = tessellator.getBuffer();
						GlStateManager.disableTexture2D();
						bufferbuilder.begin(7, DefaultVertexFormats.POSITION);
						bufferbuilder.pos((double)this.PosX, (double)(this.PosY + (float)(this.FONT_HEIGHT / 2)), 0.0D).endVertex();
						bufferbuilder.pos((double)(this.PosX + f), (double)(this.PosY + (float)(this.FONT_HEIGHT / 2)), 0.0D).endVertex();
						bufferbuilder.pos((double)(this.PosX + f), (double)(this.PosY + (float)(this.FONT_HEIGHT / 2) - 1.0F), 0.0D).endVertex();
						bufferbuilder.pos((double)this.PosX, (double)(this.PosY + (float)(this.FONT_HEIGHT / 2) - 1.0F), 0.0D).endVertex();
						tessellator.draw();
						GlStateManager.enableTexture2D();
					}

					if (this.underlineStyle)
					{
						Tessellator tessellator1 = Tessellator.getInstance();
						BufferBuilder bufferbuilder1 = tessellator1.getBuffer();
						GlStateManager.disableTexture2D();
						bufferbuilder1.begin(7, DefaultVertexFormats.POSITION);
						int l = this.underlineStyle ? -1 : 0;
						bufferbuilder1.pos((double)(this.PosX + (float)l), (double)(this.PosY + (float)this.FONT_HEIGHT), 0.0D).endVertex();
						bufferbuilder1.pos((double)(this.PosX + f), (double)(this.PosY + (float)this.FONT_HEIGHT), 0.0D).endVertex();
						bufferbuilder1.pos((double)(this.PosX + f), (double)(this.PosY + (float)this.FONT_HEIGHT - 1.0F), 0.0D).endVertex();
						bufferbuilder1.pos((double)(this.PosX + (float)l), (double)(this.PosY + (float)this.FONT_HEIGHT - 1.0F), 0.0D).endVertex();
						tessellator1.draw();
						GlStateManager.enableTexture2D();
					}*/

					pos.X += f;
					size.X += f;
				}
			}

			return size;
		}

	    public int GetStringWidth(string text)
	    {
		    if (text == null)
		    {
			    return 0;
		    }
		    else
		    {
			    return (int)Math.Ceiling(GetStringSize(text, Vector2.One).X);
		    }
	    }

		protected float RenderDefaultChar(SpriteBatch sb, int ch, bool italic, Vector2 pos, Vector2 scale)
		{
			int x = ch % 16 * 8;
			int y = ch / 16 * 8;
			int k = italic ? 1 : 0;
			//this.renderEngine.bindTexture(this.locationFontTexture);
			int l = this.CharWidth[ch];
			float f = (float)l - 0.01F;

			var sourceRectangle = new Rectangle(x, y, l, FONT_HEIGHT);
			var destRectangle = new Rectangle(
				(int)(pos.X),
				(int)(pos.Y),
				(int)(l),
				(int)(FONT_HEIGHT));

			sb.Draw(FontTexture, destRectangle, sourceRectangle, TextColor);

			return (float)l;
		}

		protected float RenderUnicodeChar(SpriteBatch sb, char ch, bool italic, Vector2 pos, Vector2 scale)
		{
			int i = this.GlyphWidth[ch] & 255;

			if (i == 0)
			{
				return 0.0F;
			}
			else
			{
				var texture = LoadGlyphTexture(sb.GraphicsDevice, ch / 256);

				int k = i >> 4;
				int l = i & 15;
				var f = k;
				var f1 = (l + 1);
				var x = (ch % 16 * 16) + f;
				var y = ((ch & 255) / 16 * 16);
				float f4 = f1 - f - 0.02F;
				float f5 = italic ? 1.0F : 0.0F;

				var sourceRectangle = new Rectangle(x, y, l, FONT_HEIGHT * 2);
				var destRectangle = new Rectangle(
					(int)(pos.X),
					(int)(pos.Y),
					(int)(((l + f4 + f5)) * scale.X),
					(int)((FONT_HEIGHT * 2) * scale.Y));

				sb.Draw(texture, destRectangle, sourceRectangle, TextColor);

				return destRectangle.Width + (((f1 - f) / 2.0F + 1.0F) * scale.X);
			}
		}

		public Vector2 GetStringSize(string text, Vector2 scale)
		{
			float width = 0, height = 0;

			foreach (var line in text.Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries))
			{
				float lineWidth = 0;
				for (int index = 0; index < line.Length; index++)
				{
					var ch = line[index];

					if (ch == 167)
					{
						index++;
						continue;
					}

					if (ch == ' ')
					{
						lineWidth += ((Unicode ? 8.0F : 4.0f) * scale.X);
						continue;
					}

					int i = this.GlyphWidth[ch] & 255;

					if (i > 0)
					{
						int k = i >> 4;
						int l = i & 15;
						var f = k;
						var f1 = (l + 1);

						float f4 = f1 - f - 0.02F;
						//float f5 = italic ? 1.0F : 0.0F;

						lineWidth += (int) (((l + f4)) * scale.X) + (((f1 - f) / 2.0F + 1.0F) * scale.X);
					}
				}

				if (lineWidth > width)
				{
					width = lineWidth;
				}

				height += (((Unicode ? FONT_HEIGHT * 2 : FONT_HEIGHT)) * scale.Y);
			}

			return new Vector2(width, height);
		}

		public int GetCharWidth(char character)
	    {
		    if (character == 167)
		    {
			    return -1;
		    }
		    else if (character == ' ')
		    {
			    return (int) ((Unicode ? 8.0F : 4.0f));
		    }
		    else
		    {
			    int i = Characters.IndexOf(character);

			    if (character > 0 && i != -1 && !this.Unicode)
			    {
				    return this.CharWidth[i];
			    }
			    else if (this.GlyphWidth[character] != 0)
			    {
				    int j = this.GlyphWidth[character] & 255;
				    int k = j >> 4;
				    int l = j & 15;
				    ++l;
				    return ((l - k) / 2);
			    }
			    else
			    {
				    return 0;
			    }
		    }
	    }

		private void ResetStyles()
	    {
		    this.randomStyle = false;
		    this.boldStyle = false;
		    this.italicStyle = false;
		    this.underlineStyle = false;
		    this.strikethroughStyle = false;
	    }

	    public int DrawString(SpriteBatch sb, String text, int x, int y, int color)
	    {
		    return this.DrawString(sb, text, (float)x, (float)y, color, false, Vector2.One);
	    }

		public int DrawString(SpriteBatch sb, string text, float x, float y, int color, bool dropShadow)
		{
			return DrawString(sb, text, x, y, color, dropShadow, Vector2.One);
		}

		public int DrawString(SpriteBatch sb, string text, float x, float y, int color, bool dropShadow, Vector2 scale)
		{
			int lineWidth;
			this.ResetStyles();

			if (dropShadow)
			{
				lineWidth = this.RenderString(sb, text, x + 1.0F, y + 1.0F, color, true, scale);
				lineWidth = Math.Max(lineWidth, this.RenderString(sb, text, x, y, color, false, scale));
			}
			else
			{
				lineWidth = this.RenderString(sb, text, x, y, color, false, scale);
			}

			return (int) (lineWidth);
		}

		private int RenderString(SpriteBatch sb, string text, float x, float y, int color, bool dropShadow, Vector2 scale)
	    {
		    var pos = new Vector2(x, y);

			if (text == null)
		    {
			    return 0;
		    }
		    else
		    {
			    float width = 0, height = 0;

			    foreach (var line in text.Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries))
			    {
				    float lineWidth = 0;

				    //  if (this.bidiFlag)
				    {
					    //   text = this.bidiReorder(text);
				    }

				    if ((color & -67108864) == 0)
				    {
					    color |= -16777216;
				    }

				    if (dropShadow)
				    {
					    color = (color & 16579836) >> 2 | color & -16777216;
				    }


				    TextColor = new Color((float) (color >> 16 & 255) / 255.0F, (float) (color >> 8 & 255) / 255.0F,
					    (float) (color & 255) / 255.0F, (float) (color >> 24 & 255) / 255.0F);
					//Color((float)(color >> 16 & 255) / 255.0F, (float)(color >> 8 & 255) / 255.0F, (float)(color & 255) / 255.0F, (float)(color >> 24 & 255) / 255.0F);
					//	this.PosX = x;
					//   this.PosY = y;
				    lineWidth = this.RenderStringAtPos(sb, line, dropShadow, pos + new Vector2(0, height), scale).X;
				    if (lineWidth > width)
				    {
					    width = lineWidth;
				    }

				    height += (((Unicode ? FONT_HEIGHT * 2 : FONT_HEIGHT)) * scale.Y);
				}

			    return (int) (Math.Ceiling(width));
		    }
	    }
	}
}
