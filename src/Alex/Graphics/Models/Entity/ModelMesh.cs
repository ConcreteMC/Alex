namespace Alex.Graphics.Models.Entity
{
	public class ModelMesh
	{
		public int StartIndex { get; }
		public int ElementCount { get; }
		
		public ModelMesh(int startIndex, int elementCount)
		{
			StartIndex = startIndex;
			ElementCount = elementCount;
		}
	}
}