using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Alex.API.Utils
{

	public class ClickEvent
	{
		public string action { get; set; }
		public string value { get; set; }
	}

	public class HoverEvent
	{
		public string action { get; set; }
		public string value { get; set; }
	}

	public class ChatTextComponent : ChatObjectComponent
	{
		
	}

	public class ChatObjectComponent
	{
		public string text { get; set; } = null;
	//	public string clickEvent { get; set; } = null;
	//	public string hoverEvent { get; set; } = null;

		public string insertion { get; set; } = null;
		public bool bold { get; set; } = false;
		public bool italic { get; set; } = false;
		public bool underlined { get; set; } = false;
		public bool strikethrough { get; set; } = false;
		public bool obfuscated { get; set; } = false;
		public string color { get; set; } = null;

		
	}

	public class ChatRootObject
	{
		public string translate { get; set; }
		public List<string> with { get; set; } = new List<string>();
	}

	public class ChatObject
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ChatObject));

		private string Msg { get; set; }
		public ChatObjectComponent[] Components { get; set; }
		public ChatObject()
	    {
		    
	    }

	    public override string ToString()
	    {
		    return Msg;
	    }

	    public static bool TryParse(string json, out ChatObject result)
	    {
		    try
		    {
				var chatRoot = new ChatRootObject();

				var cr = JObject.Parse(json);
			    foreach (var o in cr)
			    {
				    if (o.Key.Equals("with"))
				    {
					    if (o.Value.Type == JTokenType.Array)
					    {
						    foreach (var child in o.Value.Children())
						    {
								chatRoot.with.Add(child.ToString());
						    }
					    }
						else if (o.Value.Type == JTokenType.String)
					    {
							chatRoot.with.Add(o.Value.Value<string>());
					    }
				    }
					else if (o.Key.Equals("translate"))
				    {
					    chatRoot.translate = o.Value.Value<string>();
				    }
			    }

			    if (string.IsNullOrWhiteSpace(chatRoot.translate))
			    {
				    result = null;
				    return false;
			    }

			    ChatObject obj = new ChatObject();
				List<ChatObjectComponent> components = new List<ChatObjectComponent>();

			    if (chatRoot.translate == "chat.type.text")
			    {
				    bool gotUsername = false;
					StringBuilder sb = new StringBuilder();
				    foreach (var with in chatRoot.with)
				    {
					    if (string.IsNullOrWhiteSpace(with)) continue;

						try
					    {
						    var component = JsonConvert.DeserializeObject<ChatObjectComponent>(with);
							components.Add(component);

						    if (!string.IsNullOrWhiteSpace(component.insertion) && !string.IsNullOrWhiteSpace(component.text) && !gotUsername) //Hacky way to check if this is the username property
						    {
							  //  ClickEvent clickEvent = JsonConvert.DeserializeObject<ClickEvent>(component.clickEvent);
							    sb.Append($"<{component.text}> ");
							    gotUsername = true;
						    }
						    else if (!string.IsNullOrWhiteSpace(component.text))
						    {
							    sb.Append(component.text);
						    }
							else if (!string.IsNullOrWhiteSpace(component.insertion))
						    {
							    sb.Append(component.insertion);
						    }	    
						}
					    catch
					    {
						    sb.Append(with);
					    }
				    }

				    obj.Msg = sb.ToString();
				    result = obj;
				    return true;
			    }
		    }
		    catch (Exception ex)
		    {
				Log.Error(ex, $"Failed to parse chat object: {ex.ToString()}");

			    result = null;
			    return false;
		    }

		    result = null;
		    return false;
	    }
    }
}
