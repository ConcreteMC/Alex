



using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class SheepGeometrySheepShearedModel : EntityModel
	{
		public SheepGeometrySheepShearedModel()
		{
			Description = new ModelDescription()
			{
				Identifier = "geometry.sheep:geometry.sheep.sheared",
				VisibleBoundsHeight = 1.75,
				VisibleBoundsWidth = 2,
				VisibleBoundsOffset = new Vector3(0, 0.5f, 0)
			};

			Bones = new EntityModelBone[6]
			{
				new EntityModelBone(){ 
					Name = "head",
					Pivot = new Vector3(0f,18f,-8f),
					Cubes = new EntityModelCube[2]{
						new EntityModelCube()
						{
							Origin = new Vector3(-3f,16f,-14f),
							Size = new Vector3(6f, 6f, 8f),
							Uv = new Vector2(0f, 0f)
						},new EntityModelCube()
						{
							Origin = new Vector3(-3f,16f,-12f),
							Size = new Vector3(6f, 6f, 6f),
							Uv = new Vector2(0f, 32f),
							Inflate = 0.6
						}
					}
				},
				new EntityModelBone(){ 
					Name = "body",
					Pivot = new Vector3(0f,19f,2f),
					BindPoseRotation = new Vector3(90, 0, 0),
					Cubes = new EntityModelCube[2]{
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,13f,-5f),
							Size = new Vector3(8f, 16f, 6f),
							Uv = new Vector2(28f, 8f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-4f,13f,-5f),
							Size = new Vector3(8f, 16f, 6f),
							Uv = new Vector2(28f, 40f),
							Inflate = 1.75
						}
					}
				},
				new EntityModelBone(){ 
					Name = "leg0",
					Parent = "body",
					Pivot = new Vector3(-3f,12f,7f),
					Cubes = new EntityModelCube[2]{
						new EntityModelCube()
						{
							Origin = new Vector3(-5f,0f,5f),
							Size = new Vector3(4f, 12f, 4f),
							Uv = new Vector2(0f, 16f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-5f,6f,5f),
							Size = new Vector3(4f, 6f, 4f),
							Uv = new Vector2(0f, 48f),
							Inflate = 0.5
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg1",
					Parent = "body",
					Pivot = new Vector3(3f,12f,7f),
					Cubes = new EntityModelCube[2]{
						new EntityModelCube()
						{
							Origin = new Vector3(1f,0f,5f),
							Size = new Vector3(4f, 12f, 4f),
							Uv = new Vector2(0f, 16f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(1f,6f,5f),
							Size = new Vector3(4f, 6f, 4f),
							Uv = new Vector2(0f, 48f),
							Inflate = 0.5
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg2",
					Parent = "body",
					Pivot = new Vector3(-3f,12f,-5f),
					Cubes = new EntityModelCube[2]{
						new EntityModelCube()
						{
							Origin = new Vector3(-5f,0f,-7f),
							Size = new Vector3(4f, 12f, 4f),
							Uv = new Vector2(0f, 16f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(-5f,6f,-7f),
							Size = new Vector3(4f, 6f, 4f),
							Uv = new Vector2(0f, 48f),
							Inflate = 0.5
						},
					}
				},
				new EntityModelBone(){ 
					Name = "leg3",
					Parent = "body",
					Pivot = new Vector3(3f,12f,-5f),
					Cubes = new EntityModelCube[2]{
						new EntityModelCube()
						{
							Origin = new Vector3(1f,0f,-7f),
							Size = new Vector3(4f, 12f, 4f),
							Uv = new Vector2(0f, 16f)
						},
						new EntityModelCube()
						{
							Origin = new Vector3(1f,6f,-7f),
							Size = new Vector3(4f, 6f, 4f),
							Uv = new Vector2(0f, 48f),
							Inflate = 0.5
						},
					}
				},
			};
		}

	}

}