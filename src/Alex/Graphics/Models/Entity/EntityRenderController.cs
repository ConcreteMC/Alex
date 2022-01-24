using System;
using System.Collections.Generic;
using Alex.Common.Utils;
using Alex.Entities.Passive;
using Alex.Graphics.Models.Entity.Animations;
using Alex.MoLang.Runtime.Struct;
using Alex.MoLang.Runtime.Value;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using NLog;

namespace Alex.Graphics.Models.Entity
{
	public class EntityRenderController : IAnimation
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(EntityRenderController));
		private readonly AnimationComponent _parent;
		private readonly RenderController _definition;
		private bool _requiresUpdate = false;

		private IDictionary<string, IDictionary<string, IMoValue>> _context;

		public EntityRenderController(AnimationComponent parent, RenderController definition)
		{
			_parent = parent;
			_definition = definition;
			_context = BuildContext(definition);
		}

		private IDictionary<string, IDictionary<string, IMoValue>> BuildContext(RenderController definition)
		{
			Dictionary<string, IDictionary<string, IMoValue>> context =
				new Dictionary<string, IDictionary<string, IMoValue>>(StringComparer.OrdinalIgnoreCase);

			if (_definition.Arrays != null)
			{
				foreach (var array in _definition.Arrays)
				{
					string key = array.Key;
				}
			}

			return context;
		}

		private void UpdatePartVisibility()
		{
			var visibilities = _definition?.PartVisibility;

			if (visibilities == null)
				return;

			foreach (var part in visibilities)
			{
				if (part.Expressions == null)
					continue;

				foreach (var p in part.Expressions)
				{
					var result = _parent.Runtime.Execute(p.Value, _parent.Context);
					_renderer?.SetVisibility(p.Key, result.AsBool());
				}
			}
		}

		/// <inheritdoc />
		public void Tick()
		{
			if (!_requiresUpdate)
				return;

			UpdatePartVisibility();

			try
			{
				var textures = _definition.Textures;

				if (textures != null)
				{
					IDictionary<string, IMoValue> context = _parent.Context;

					if (_definition.Arrays != null && _definition.Arrays.TryGetValue("textures", out var tarray))
					{
						context = new Dictionary<string, IMoValue>();

						foreach (var val in tarray)
						{
							context.Add(val.Key, _parent.Execute(val.Value));
						}
					}

					string[] textureResults = new string[textures.Length];

					for (var index = 0; index < textures.Length; index++)
					{
						var texture = textures[index];

						if (texture.IsString)
						{
							textureResults[index] = texture.StringValue;
						}
						else
						{
							foreach (var element in texture.Expressions)
							{
								var result = _parent.Runtime.Execute(element.Value, context);

								textureResults[index] = result.AsString();
								//Log.Info($"Got texture: {result.AsString()}");
							}
						}
					}

					//if (_parent.Entity is AbstractHorse)
					{
						for (var index = 0; index < textureResults.Length; index++)
						{
							if (index > 0)
								break;

							var result = textureResults[index];

							if (_parent?.Entity?.Texture?.Tag is string str && str == result)
							{
								continue;
							}

							if (_parent.EntityDefinition.Textures.TryGetValue(result, out var texturePath))
							{
								if (Alex.Instance.Resources.TryGetBedrockBitmap(texturePath, out var bitmap))
								{
									TextureUtils.BitmapToTexture2DAsync(
										this, Alex.Instance.GraphicsDevice, bitmap, texture2D =>
										{
											if (texture2D == null)
											{
												return;
											}

											texture2D.Tag = result;
											_parent.Entity.Texture = texture2D;
										});
								}
							}
							//if (Alex.Instance.Resources.)
							//if (_parent.Entity.Texture)
							//Log.Info($"Texture[{index}] = {result}");
						}
					}
				}
			}
			finally
			{
				_requiresUpdate = false;
			}
		}

		private ModelRenderer _renderer;

		/// <inheritdoc />
		public void UpdateBindings(ModelRenderer renderer)
		{
			_renderer = renderer;
			_requiresUpdate = true;
		}

		public void ForceUpdate()
		{
			_requiresUpdate = true;
		}
	}
}