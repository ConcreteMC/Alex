using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Alex.ResourcePackLib
{
    public class FontRenderer
    {
	    const string chars = "\u00c0\u00c1\u00c2\u00c8\u00ca\u00cb\u00cd\u00d3\u00d4\u00d5\u00da\u00df\u00e3\u00f5\u011f\u0130\u0131\u0152\u0153\u015e\u015f\u0174\u0175\u017e\u0207\u0000\u0000\u0000\u0000\u0000\u0000\u0000 !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~\u0000\u00c7\u00fc\u00e9\u00e2\u00e4\u00e0\u00e5\u00e7\u00ea\u00eb\u00e8\u00ef\u00ee\u00ec\u00c4\u00c5\u00c9\u00e6\u00c6\u00f4\u00f6\u00f2\u00fb\u00f9\u00ff\u00d6\u00dc\u00f8\u00a3\u00d8\u00d7\u0192\u00e1\u00ed\u00f3\u00fa\u00f1\u00d1\u00aa\u00ba\u00bf\u00ae\u00ac\u00bd\u00bc\u00a1\u00ab\u00bb\u2591\u2592\u2593\u2502\u2524\u2561\u2562\u2556\u2555\u2563\u2551\u2557\u255d\u255c\u255b\u2510\u2514\u2534\u252c\u251c\u2500\u253c\u255e\u255f\u255a\u2554\u2569\u2566\u2560\u2550\u256c\u2567\u2568\u2564\u2565\u2559\u2558\u2552\u2553\u256b\u256a\u2518\u250c\u2588\u2584\u258c\u2590\u2580\u03b1\u03b2\u0393\u03c0\u03a3\u03c3\u03bc\u03c4\u03a6\u0398\u03a9\u03b4\u221e\u2205\u2208\u2229\u2261\u00b1\u2265\u2264\u2320\u2321\u00f7\u2248\u00b0\u2219\u00b7\u221a\u207f\u00b2\u25a0\u0000";

		public int FONT_HEIGHT = 9;

		private byte[] GlyphWidth { get; }
	    private int[] CharWidth { get; set; } = null;
		private Texture2D FontTexture { get; }
		private bool Unicode { get; }
	    private int[] colorCode { get; }

		protected float posX;
	    protected float posY;

	    private int textColor;
	    private bool randomStyle;
	    private bool boldStyle;
	    private bool italicStyle;
	    private bool underlineStyle;
	    private bool strikethroughStyle;

	    private Microsoft.Xna.Framework.Color TextColor = Microsoft.Xna.Framework.Color.Black;

	    private Random fontRandom;
		public FontRenderer(bool unicode, Texture2D fontTexture, byte[] glyphWidth)
	    {
			fontRandom = new Random();

		    Unicode = unicode;
		    FontTexture = fontTexture;

			colorCode = new int[32];
			for (int i = 0; i < 32; ++i)
		    {
			    int j = (i >> 3 & 1) * 85;
			    int k = (i >> 2 & 1) * 170 + j;
			    int l = (i >> 1 & 1) * 170 + j;
			    int i1 = (i >> 0 & 1) * 170 + j;

			    if (i == 6)
			    {
				    k += 85;
			    }

			    if (i >= 16)
			    {
				    k /= 4;
				    l /= 4;
				    i1 /= 4;
			    }

			    this.colorCode[i] = (k & 255) << 16 | (l & 255) << 8 | i1 & 255;
		    }

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

		private float RenderChar(SpriteBatch sb, char ch, bool italic)
		{
			if (CharWidth == null)
			{
				CharWidth = ReadFontTexture();
			}

			if (ch == ' ')
			{
				return 4.0F;
			}
			else
			{
				int i = chars.IndexOf(ch);
				return i != -1 && !this.Unicode ? this.RenderDefaultChar(sb, i, italic) : this.RenderDefaultChar(sb, ch, italic);
			}
		}

		protected float RenderDefaultChar(SpriteBatch sb, int ch, bool italic)
		{
			int x = ch % 16 * 8;
			int y = ch / 16 * 8;
			int k = italic ? 1 : 0;
			//this.renderEngine.bindTexture(this.locationFontTexture);
			int l = this.CharWidth[ch];

			sb.Draw(FontTexture, new Vector2(posX, posY), new Rectangle(x / 128, y / 128, l, 8), TextColor);
			/*GlStateManager.glBegin(5);

			GlStateManager.glTexCoord2f((float)x / 128.0F, (float)y / 128.0F);
			GlStateManager.glVertex3f(this.posX + (float)k, this.posY, 0.0F);
			GlStateManager.glTexCoord2f((float)x / 128.0F, ((float)y + 7.99F) / 128.0F);
			GlStateManager.glVertex3f(this.posX - (float)k, this.posY + 7.99F, 0.0F);

			GlStateManager.glTexCoord2f(((float)x + f - 1.0F) / 128.0F, (float)y / 128.0F);
			GlStateManager.glVertex3f(this.posX + f - 1.0F + (float)k, this.posY, 0.0F);
			GlStateManager.glTexCoord2f(((float)x + f - 1.0F) / 128.0F, ((float)y + 7.99F) / 128.0F);
			GlStateManager.glVertex3f(this.posX + f - 1.0F - (float)k, this.posY + 7.99F, 0.0F);

			GlStateManager.glEnd();*/
			return (float)l;
		}

	    protected float RenderUnicodeChar(SpriteBatch sb, char ch, bool italic)
	    {
		/*    int i = this.GlyphWidth[ch] & 255;

		    if (i == 0)
		    {
			    return 0.0F;
		    }
		    else
		    {
			    int j = ch / 256;
			    this.loadGlyphTexture(j);
			    int k = i >> 4;
			    int l = i & 15;
			    float f = (float)k;
			    float f1 = (float)(l + 1);
			    float f2 = (float)(ch % 16 * 16) + f;
			    float f3 = (float)((ch & 255) / 16 * 16);
			    float f4 = f1 - f - 0.02F;
			    float f5 = italic ? 1.0F : 0.0F;
			    GlStateManager.glBegin(5);
			    GlStateManager.glTexCoord2f(f2 / 256.0F, f3 / 256.0F);
			    GlStateManager.glVertex3f(this.posX + f5, this.posY, 0.0F);
			    GlStateManager.glTexCoord2f(f2 / 256.0F, (f3 + 15.98F) / 256.0F);
			    GlStateManager.glVertex3f(this.posX - f5, this.posY + 7.99F, 0.0F);
			    GlStateManager.glTexCoord2f((f2 + f4) / 256.0F, f3 / 256.0F);
			    GlStateManager.glVertex3f(this.posX + f4 / 2.0F + f5, this.posY, 0.0F);
			    GlStateManager.glTexCoord2f((f2 + f4) / 256.0F, (f3 + 15.98F) / 256.0F);
			    GlStateManager.glVertex3f(this.posX + f4 / 2.0F - f5, this.posY + 7.99F, 0.0F);
			    GlStateManager.glEnd();
			    return (f1 - f) / 2.0F + 1.0F;
		    }*/
		    return 0f;
	    }

		private void RenderStringAtPos(SpriteBatch sb, string text, bool shadow)
		{
			for (int i = 0; i < text.Length; ++i)
			{
				char c0 = text[i];

				if (c0 == 167 && i + 1 < text.Length)
				{
					int i1 = "0123456789abcdefklmnor".IndexOf(text[i + 1].ToString().ToLower()[0]);

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

						int j1 = this.colorCode[i1];
						this.textColor = j1;
						Color((float)(j1 >> 16) / 255.0F, (float)(j1 >> 8 & 255) / 255.0F, (float)(j1 & 255) / 255.0F, TextColor.A);
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
						Color(TextColor.R, TextColor.B, TextColor.G, TextColor.A);
					}

					++i;
				}
				else
				{
					int j = chars.IndexOf(c0);

					if (this.randomStyle && j != -1)
					{
						int k = this.GetCharWidth(c0);
						char c1;

						while (true)
						{
							j = this.fontRandom.Next(chars.Length);
							c1 = chars[j];//.charAt(j);

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
						this.posX -= f1;
						this.posY -= f1;
					}

					float f = this.RenderChar(sb, c0, this.italicStyle);

					if (flag)
					{
						this.posX += f1;
						this.posY += f1;
					}

					if (this.boldStyle)
					{
						this.posX += f1;

						if (flag)
						{
							this.posX -= f1;
							this.posY -= f1;
						}

						this.RenderChar(sb, c0, this.italicStyle);
						this.posX -= f1;

						if (flag)
						{
							this.posX += f1;
							this.posY += f1;
						}

						++f;
					}

					/*if (this.strikethroughStyle)
					{
						Tessellator tessellator = Tessellator.getInstance();
						BufferBuilder bufferbuilder = tessellator.getBuffer();
						GlStateManager.disableTexture2D();
						bufferbuilder.begin(7, DefaultVertexFormats.POSITION);
						bufferbuilder.pos((double)this.posX, (double)(this.posY + (float)(this.FONT_HEIGHT / 2)), 0.0D).endVertex();
						bufferbuilder.pos((double)(this.posX + f), (double)(this.posY + (float)(this.FONT_HEIGHT / 2)), 0.0D).endVertex();
						bufferbuilder.pos((double)(this.posX + f), (double)(this.posY + (float)(this.FONT_HEIGHT / 2) - 1.0F), 0.0D).endVertex();
						bufferbuilder.pos((double)this.posX, (double)(this.posY + (float)(this.FONT_HEIGHT / 2) - 1.0F), 0.0D).endVertex();
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
						bufferbuilder1.pos((double)(this.posX + (float)l), (double)(this.posY + (float)this.FONT_HEIGHT), 0.0D).endVertex();
						bufferbuilder1.pos((double)(this.posX + f), (double)(this.posY + (float)this.FONT_HEIGHT), 0.0D).endVertex();
						bufferbuilder1.pos((double)(this.posX + f), (double)(this.posY + (float)this.FONT_HEIGHT - 1.0F), 0.0D).endVertex();
						bufferbuilder1.pos((double)(this.posX + (float)l), (double)(this.posY + (float)this.FONT_HEIGHT - 1.0F), 0.0D).endVertex();
						tessellator1.draw();
						GlStateManager.enableTexture2D();
					}*/

					this.posX += (float)((int)f);
				}
			}
		}

	    private void Color(float red, float blue, float green, float alpha)
	    {
		   TextColor = new Color(red, green, blue, alpha);
	    }

	    public int GetStringWidth(string text)
	    {
		    if (text == null)
		    {
			    return 0;
		    }
		    else
		    {
			    int i = 0;
			    bool flag = false;

			    for (int j = 0; j < text.Length; ++j)
			    {
				    char c0 = text[j];
				    int k = this.GetCharWidth(c0);

				    if (k < 0 && j < text.Length - 1)
				    {
					    ++j;
					    c0 = text[j];

					    if (c0 != 'l' && c0 != 'L')
					    {
						    if (c0 == 'r' || c0 == 'R')
						    {
							    flag = false;
						    }
					    }
					    else
					    {
						    flag = true;
					    }

					    k = 0;
				    }

				    i += k;

				    if (flag && k > 0)
				    {
					    ++i;
				    }
			    }

			    return i;
		    }
	    }

		public int GetCharWidth(char character)
	    {
		    if (character == 167)
		    {
			    return -1;
		    }
		    else if (character == ' ')
		    {
			    return 4;
		    }
		    else
		    {
			    int i = chars.IndexOf(character);

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
				    return (l - k) / 2 + 1;
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
		    return this.DrawString(sb, text, (float)x, (float)y, color, false);
	    }

	    public int DrawString(SpriteBatch sb, string text, float x, float y, int color, bool dropShadow)
	    {
		    //GlStateManager.enableAlpha();
			
		    this.ResetStyles();
		    int i;

		    if (dropShadow)
		    {
			    i = this.RenderString(sb, text, x + 1.0F, y + 1.0F, color, true);
			    i = Math.Max(i, this.RenderString(sb, text, x, y, color, false));
		    }
		    else
		    {
			    i = this.RenderString(sb, text, x, y, color, false);
		    }

		    return i;
	    }

	    private int RenderString(SpriteBatch sb, string text, float x, float y, int color, bool dropShadow)
	    {
		    if (text == null)
		    {
			    return 0;
		    }
		    else
		    {
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

			    Color((float)(color >> 16 & 255) / 255.0F, (float)(color >> 8 & 255) / 255.0F, (float)(color & 255) / 255.0F, (float)(color >> 24 & 255) / 255.0F);
			    this.posX = x;
			    this.posY = y;
			    this.RenderStringAtPos(sb, text, dropShadow);
			    return (int)this.posX;
		    }
	    }
	}
}
