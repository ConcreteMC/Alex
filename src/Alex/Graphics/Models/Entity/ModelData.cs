using System;
using Alex.Common.Graphics;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models.Entity
{
	public class ModelData
	{
		private BoneData[] Bones { get; }
		public ModelData()
		{
			
		}

		public BoneData GetBone(int index)
		{
			return Bones[index];
		}
		
		public void Update(IUpdateArgs args)
		{
			foreach (var bone in Bones)
			{
				bone.Update(args);
			}
		}
	}
}