using Alex.Net;
using Alex.Networking.Java.Packets.Play;
using Alex.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework;

namespace Alex.Entities
{
	public class EntityArmorStand : LivingEntity
	{
		/// <inheritdoc />
		public EntityArmorStand(World level, NetworkProvider network) : base(
			(int) EntityType.ArmorStand, level, network)
		{
			//HealthManager.Invulnerable = true;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 14 && entry is MetadataByte data)
			{
				var isSmall = (data.Value & 0x01) != 0;
				var hasArms = (data.Value & 0x04) != 0;
				var noBasePlate = (data.Value & 0x08) != 0;
				var setMarker = (data.Value & 0x10) != 0;

				if (setMarker)
				{
					Width = 0f;
					Height = 0f;
					NoAi = true;
				}
				//IsInvisible = setMarker;

				var renderer = ModelRenderer;
				if (renderer != null)
				{
					renderer.SetVisibility("leftarm", hasArms);
					renderer.SetVisibility("rightarm", hasArms);
					renderer.SetVisibility("baseplate", !noBasePlate);
				}
			}
			else if (entry.Index >= 15 && entry.Index <= 20 && entry is MetadataRotation rotation)
			{
				switch (entry.Index)
				{
					case 15: //Head
						SetHeadRotation(rotation.Rotation);
						break;
					case 16: //Body
						SetBodyRotation(rotation.Rotation);
						break;
					case 17: //Left Arm
						SetArmRotation(rotation.Rotation, true);
						break;
					case 18: //Right Arm
						SetArmRotation(rotation.Rotation, false);
						break;
					case 19: //Left Leg
						SetLegRotation(rotation.Rotation, true);
						break;
					case 20: //Right Leg
						SetLegRotation(rotation.Rotation, false);
						break;
				}
			}
		}

		public void SetHeadRotation(Vector3 rotation)
		{
			if (ModelRenderer.GetBone("head", out var head))
			{
				head.Rotation = rotation;
			}
		}

		public void SetBodyRotation(Vector3 rotation)
		{
			if (ModelRenderer.GetBone("body", out var head))
			{
				head.Rotation = rotation;
			}
		}

		public void SetArmRotation(Vector3 rotation, bool isLeftArm)
		{
			if (ModelRenderer.GetBone(isLeftArm ? "leftarm" : "rightarm", out var head))
			{
				head.Rotation = rotation;
			}
		}
		
		public void SetLegRotation(Vector3 rotation, bool isLeftLeg)
		{
			if (ModelRenderer.GetBone(isLeftLeg ? "leftleg" : "rightleg", out var head))
			{
				head.Rotation = rotation;
			}
		}
	}
}