using Alex.Net;
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