using System.Collections.Generic;
using System.Linq;
using Alex.ResourcePackLib.Json.Converters;
using Newtonsoft.Json;
using NLog;
using Org.BouncyCastle.Security;

namespace Alex.ResourcePackLib.Json.Models.Entities
{
	public class EntityModel
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(EntityModel));

		[JsonIgnore]
		public FormatVersion FormatVersion { get; set; } = FormatVersion.Unknown;
		
		[JsonProperty("description")]
		public ModelDescription Description { get; set; }
	    
		/// <summary>
		/// Bones define the 'skeleton' of the mob: the parts that can be animated, and to which geometry and other bones are attached.
		/// </summary>
		[JsonProperty("bones")]
		public EntityModelBone[] Bones { get; set; }

		public static EntityModel operator +(EntityModel baseEntity, EntityModel topEntity)
		{
			if (baseEntity == null)
			{
				throw new InvalidParameterException("BaseEntity may not be null!");
			}
			
			if (topEntity == null)
			{
				throw new InvalidParameterException("TopEntity may not be null!");
			}
			
			Dictionary<string, EntityModelBone> bones  = new Dictionary<string, EntityModelBone>();
			
			EntityModel           entity = new EntityModel();
			entity.Description = topEntity.Description;

			if (baseEntity.Bones != null)
			{
				foreach (var bone in baseEntity.Bones)
				{
					bones.Add(bone.Name, bone.Clone());
				}
			}

			if (topEntity.Bones != null)
			{
				foreach (var bone in topEntity.Bones)
				{
					if (!bones.TryAdd(bone.Name, bone.Clone()))
					{
						var cubes = bone.Cubes?.Select(x => x.Clone()).ToArray();

						//Already exists.
						if (bone.Cubes == null || bone.Cubes.Length == 0)
						{
							if (cubes != null && cubes.Length > 0)
							{
								bones[bone.Name].Cubes = cubes;
							}
						}
						else if (bone.Cubes != null && bone.Cubes.Length > 0)
						{
							if (cubes != null && cubes.Length > 0)
							{
								bones[bone.Name].Cubes = bones[bone.Name].Cubes.Concat(cubes).ToArray();
							}
						}

						//bones[bone.Name].Cubes = ;
					}
				}
			}

			entity.Bones = bones.Values.ToArray();

			return entity;
		}

		public EntityModel Clone()
		{
			return new EntityModel()
			{
				Bones = Bones?.Select(x => x.Clone()).ToArray(), Description = Description.Clone()
			};
		}
	}
}