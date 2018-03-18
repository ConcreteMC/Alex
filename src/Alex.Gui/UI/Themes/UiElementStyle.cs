using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Alex.Graphics.Textures;
using Alex.Graphics.UI.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.UI.Themes
{

	public class UiElementStyle
	{
		public int StylePriority { get; set; } = 0;

		public Color? BackgroundColor { get; set; }
		public TextureRepeatMode? BackgroundRepeat { get; set; }
		public NinePatchTexture Background { get; set; }


		public SpriteFont TextFont { get; set; }
		public Color? TextColor { get; set; }
		public Color? TextShadowColor { get; set; }
		public float? TextSize { get; set; }
		public int? TextShadowSize { get; set; }

		public int? Width { get; set; }
		public int? Height { get; set; }
		public int? MinWidth  { get; set; }
		public int? MinHeight { get; set; }
		public int? MaxWidth  { get; set; }
		public int? MaxHeight { get; set; }

		public Thickness? Padding { get; set; }
		public Thickness? Margin { get; set; }
		
		public Vector2? SizeAnchor { get; set; }
		public Vector2? PositionAnchor { get; set; }
		public Vector2? SizeAnchorOrigin     { get; set; }
		public Vector2? PositionAnchorOrigin { get; set; }

		public HorizontalAlignment? HorizontalContentAlignment { get; set; }
		public VerticalAlignment? VerticalContentAlignment { get; set; }
	}
	
	public class UiElementStyle2
	{
		

		public int Priority { get; set; } = -1;
		
		public UiElementStyleProperty<Color> BackgroundColor { get; set; } = new UiElementStyleProperty<Color>();

		[AvoidNullValue, DefaultValue(TextureRepeatMode.Stretch)]
		public UiElementStyleProperty<TextureRepeatMode> BackgroundRepeat { get; set; } = new UiElementStyleProperty<TextureRepeatMode>(TextureRepeatMode.Stretch);

		public UiElementStyleProperty<NinePatchTexture> Background { get; set; } = new UiElementStyleProperty<NinePatchTexture>();

		public UiElementStyleProperty<Color> TextColor { get; set; } = new UiElementStyleProperty<Color>();
		public UiElementStyleProperty<Color> TextShadowColor { get; set; } = new UiElementStyleProperty<Color>();
		public UiElementStyleProperty<int> TextShadowSize { get; set; } = new UiElementStyleProperty<int>();

		[AvoidNullValue]
		public UiElementStyleProperty<SpriteFont> TextFont { get; set; } = new UiElementStyleProperty<SpriteFont>();

		public UiElementStyleProperty<int?> Width { get; set; } = new UiElementStyleProperty<int?>();

		public UiElementStyleProperty<int?> Height { get; set; } = new UiElementStyleProperty<int?>();

		public UiElementStyleProperty<Thickness> Padding { get; set; } = new UiElementStyleProperty<Thickness>(Thickness.Zero);

		public UiElementStyleProperty<Thickness> Margin { get; set; } = new UiElementStyleProperty<Thickness>(Thickness.Zero);

		[AvoidNullValue, DefaultValue(SizeMode.FitToContent)]
		public UiElementStyleProperty<SizeMode> WidthSizeMode { get; set; } = new UiElementStyleProperty<SizeMode>(SizeMode.FitToContent);

		[AvoidNullValue, DefaultValue(SizeMode.FitToContent)]
		public UiElementStyleProperty<SizeMode> HeightSizeMode { get; set; } = new UiElementStyleProperty<SizeMode>(SizeMode.FitToContent);

		[AvoidNullValue, DefaultValue(HorizontalAlignment.None)]
		public UiElementStyleProperty<HorizontalAlignment> HorizontalContentAlignment { get; set; } = new UiElementStyleProperty<HorizontalAlignment>(HorizontalAlignment.None);

		[AvoidNullValue, DefaultValue(VerticalAlignment.None)]
		public UiElementStyleProperty<VerticalAlignment> VerticalContentAlignment { get; set; } = new UiElementStyleProperty<VerticalAlignment>(VerticalAlignment.None);

		public UiElementStyle2()
		{

		}

		public UiElementStyle2(int priority)
		{
			Priority = priority;
		}
		
		public void ApplyStyle(ref UiElementStyle2 style)
		{
			//Log.InfoFormat("Apply Styles");
			var type = this.GetType();
			foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				if (prop.PropertyType.GetInterfaces().Contains(typeof(IUiElementStyleProperty)) && prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(UiElementStyleProperty<>))
				{
					var propType = prop.PropertyType.GetGenericArguments().FirstOrDefault();

					var isNotNull = prop.IsDefined(typeof(AvoidNullValueAttribute));

					var thisValue = (IUiElementStyleProperty) prop.GetValue(this);
					var styleValue = (IUiElementStyleProperty) prop.GetValue(style);

					var thisPriority = Math.Max(thisValue?.Priority ?? -1, Priority);
					var stylePriority = Math.Max(styleValue?.Priority ?? -1, style.Priority);

					if (thisValue == null)
					{
						var genericType = typeof(UiElementStyleProperty<>).MakeGenericType(propType);
						IUiElementStyleProperty valueToSet = (IUiElementStyleProperty)Activator.CreateInstance(genericType);

						if (prop.IsDefined(typeof(DefaultValueAttribute)))
						{
							valueToSet.Value = Convert.ChangeType(prop.GetCustomAttribute<DefaultValueAttribute>().Value, propType);
						}

						prop.SetValue(this, valueToSet);
					}
					else if(styleValue != null && ((!thisValue.HasValue && styleValue.HasValue) || (stylePriority > thisPriority && !styleValue.HasValue && thisValue.HasValue && isNotNull)))
					{
						prop.SetValue(this, styleValue);
					}

//					Log.InfoFormat("Found {1} {0} = (This = {2}, Style = {3}, Result = {4})", prop.Name, prop.ReflectedType.Name, thisValue, styleValue, prop.GetValue(this));
				}
			}
		}

		[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
		class AvoidNullValueAttribute : Attribute
		{

		}
	}
}
