using System;
using System.Linq;
using Alex.API.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alex.API.Data.Chat.Serializer
{
	public class BaseComponentSerializer : JsonConverter<BaseComponent>
	{
		public override void WriteJson(JsonWriter writer, BaseComponent value, JsonSerializer serializer)
		{
			
		}

		public override BaseComponent ReadJson(JsonReader reader, Type objectType, BaseComponent component,
			bool hasExistingValue,
			JsonSerializer serializer)
		{
	
				Console.WriteLine($"TokenType: " + reader.TokenType);
				if (reader.TokenType == JsonToken.StartObject)
				{
					ComponentBuilder builder = new ComponentBuilder("");
					
					JObject @object = JObject.Load(reader);
					Console.WriteLine("Object: " + @object.ToString(Formatting.Indented));

					if (@object.Has("text"))
					{
						builder = new ComponentBuilder(@object["text"].Value<string>());
					}
					else if (@object.Has("translate"))
					{

					}

					if (@object.Has("color"))
					{
						builder.Color(TextColor.GetColor(@object["color"].Value<string>()));
					//	component.Color = TextColor.GetColor(@object["color"].Value<string>());
						//component.setColor(ChatColor.valueOf(@object["color"].getAsString().toUpperCase(Locale.ROOT)));
					}

					if (@object.Has("bold"))
					{
						builder.Bold(@object["bold"].Value<bool>());
						//component.Bold = @object["bold"].Value<bool>();
					}

					if (@object.Has("italic"))
					{
						builder.Italic(@object["italic"].Value<bool>());
						//component.Italic = @object["italic"].Value<bool>();
					}

					if (@object.Has("underlined"))
					{
						builder.Underlined(@object["underlined"].Value<bool>());
						//component.Underlined = @object["underlined"].Value<bool>();
						//component.setUnderlined(object.get("underlined").getAsBoolean());
					}

					if (@object.Has("strikethrough"))
					{
						builder.Strikethrough(@object["strikethrough"].Value<bool>());
						//component.Strikethrough = @object["strikethrough"].Value<bool>();
						//component.setStrikethrough(object.get("strikethrough").getAsBoolean());
					}

					if (@object.Has("obfuscated"))
					{
						builder.Obfuscated(@object["obfuscated"].Value<bool>());
						//	component.Obfuscated = @object["obfuscated"].Value<bool>();
						//.setObfuscated(object.get("obfuscated").getAsBoolean());
					}

					if (@object.Has("insertion"))
					{
						builder.Insertion(@object["insertion"].Value<string>());
						//component.Insertion = @object["insertion"].Value<string>();
					}

					if (@object.Has("extra"))
					{
						foreach (var extra in @object["extra"].Value<BaseComponent[]>())
						{
							builder.Append(extra);
							//component.AddExtra(extra);
						}
						//component.setExtra(Arrays.<BaseComponent>asList(context.< BaseComponent[] > deserialize(@object.get("extra"), BaseComponent[].class ) ) );
					}

					//Events
					if (@object.Has("clickEvent"))
					{
						var @event = @object["clickEvent"];
						string rawAction = @event["action"].Value<string>();
						string rawValue = @event["value"].Value<string>();
						if (Enum.TryParse(rawAction, true, out ClickEvent.Action action))
						{
							builder.ClickEvent(new ClickEvent(action, rawValue));
							//component.ClickEvent = new ClickEvent(action, rawValue);
						}
						
						//ClickEvent e = @object.Value<ClickEvent>("clickComponent");
						
						//	e.value
						//ClickEvent.Action a = @event["action"].ToObject<ClickEvent.Action>();
						//ClickEvent.Action a = @event["action"].tos<ClickEvent.Action>();
						//string v = @event["value"].getAsString();
						//component.ClickEvent = 
					}

					if (@object.Has("hoverEvent"))

					{
						var @event = @object["hoverEvent"];
						BaseComponent[] res;
						if (@event["value"].Type == JTokenType.Array)
						{
							res = @event["value"].Value<BaseComponent[]>();
						}
						else
						{
							res = new BaseComponent[]
							{
								@event["value"].Value<BaseComponent>()
							};
						}

						string rawAction = @event["action"].Value<string>();
						if (Enum.TryParse(rawAction, true, out HoverEvent.Action action))
						{
							builder.HoverEvent(new HoverEvent()
							{
								value = res,
								action = action
							});
							/*component.HoverEvent = new HoverEvent()
							{
								value = res,
								action = action
							};*/
						}
						//HoverEvent
						//component = he;

						//component = (new HoverEvent(HoverEvent.Action.valueOf( event.get("action").getAsString()
						//	.toUpperCase(Locale.ROOT) ), res ) );
					//	component = builder.Create();
					}

					component = builder.Create().First();
					foreach (var c in builder.Create())
					{
						Console.WriteLine($"{c}");
					}
					Console.WriteLine($"Returning component: {component}");
					return component;
				}
				Console.WriteLine($"Returning null!");
				// This should not happen. Perhaps better to throw exception at this point?
				return null;

		}
	}

	public static class JsonExtensions
	{
		public static bool Has(this JObject reader, string key)
		{
			return reader.ContainsKey(key);
		}
	}
}
