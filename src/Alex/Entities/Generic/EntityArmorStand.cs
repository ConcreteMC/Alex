using System.Collections.Generic;
using Alex.Common.Utils;
using Alex.Net;
using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using MiNET.Entities;

namespace Alex.Entities.Generic
{
	public class EntityArmorStand : LivingEntity
	{
		/// <inheritdoc />
		public EntityArmorStand(World level) : base(
			level)
		{
			//HealthManager.Invulnerable = true;
			IsAffectedByGravity = false;
			
			SetSmall(false);
		}

		/// <inheritdoc />
		protected override void OnModelUpdated()
		{
			base.OnModelUpdated();

			if (ModelRenderer != null)
			{
				var meta = _pendingMetadata.ToArray();
				_pendingMetadata.Clear();
				foreach(var entry in meta)
					HandleJavaMeta(entry);
			}
		}

		private List<MetaDataEntry> _pendingMetadata = new List<MetaDataEntry>();
		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 15 && entry is MetadataByte data)
			{
				if (ModelRenderer == null)
				{
					_pendingMetadata.Add(entry);
					return;
				}
				
				var isSmall = (data.Value & 0x01) != 0;
				
				SetSmall(isSmall);
					
				var hasArms = (data.Value & 0x04) != 0;
				var noBasePlate = (data.Value & 0x08) != 0;
				var setMarker = (data.Value & 0x10) != 0;

				if (setMarker)
				{
					Width = 0f;
					//Height = 0f;
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
			else if (entry.Index >= 16 && entry.Index <= 21 && entry is MetadataRotation rotation)
			{
				if (ModelRenderer == null)
				{
					_pendingMetadata.Add(entry);
					return;
				}
				
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
			if (ModelRenderer == null) return;
			if (ModelRenderer.GetBoneTransform("head", out var head))
			{
				//rotation.Y = 180f - rotation.Y;
				head.Rotation = rotation;
				// Quaternion.CreateFromYawPitchRoll(MathUtils.ToRadians(rotation.Y), MathUtils.ToRadians(rotation.X), MathUtils.ToRadians(rotation.Z));;
			}
		}

		public void SetBodyRotation(Vector3 rotation)
		{
			if (ModelRenderer == null) return;
			if (ModelRenderer.GetBoneTransform("body", out var head))
			{
				rotation.Y = 180f - rotation.Y;

				head.Rotation =
					rotation; //Quaternion.CreateFromYawPitchRoll(MathUtils.ToRadians(rotation.Y), MathUtils.ToRadians(rotation.X), MathUtils.ToRadians(rotation.Z));;
			}
		}

		public void SetArmRotation(Vector3 rotation, bool isLeftArm)
		{
			if (ModelRenderer == null) return;
			if (ModelRenderer.GetBoneTransform(isLeftArm ? "leftarm" : "rightarm", out var head))
			{
				head.Rotation =
					rotation; //Quaternion.CreateFromYawPitchRoll(MathUtils.ToRadians(rotation.Y), MathUtils.ToRadians(rotation.X), MathUtils.ToRadians(rotation.Z));;
			}
		}
		
		public void SetLegRotation(Vector3 rotation, bool isLeftLeg)
		{
			if (ModelRenderer == null) return;
			if (ModelRenderer.GetBoneTransform(isLeftLeg ? "leftleg" : "rightleg", out var head))
			{
				//rotation.Y = 180f - rotation.Y;
				head.Rotation = rotation;// Quaternion.CreateFromYawPitchRoll(MathUtils.ToRadians(rotation.Y), MathUtils.ToRadians(rotation.X), MathUtils.ToRadians(rotation.Z));;
			}
		}

		public void SetSmall(bool small)
		{
			if (small)
			{
				Width = 0.5;
				Height = 1.975;
			}
			else
			{
				Width = 0.25;
				Height = 0.9875;
			}
		}
	}
}