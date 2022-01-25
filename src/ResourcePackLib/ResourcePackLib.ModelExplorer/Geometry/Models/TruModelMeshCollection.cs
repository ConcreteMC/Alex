using System.Collections.ObjectModel;
using System.Diagnostics;
using JetBrains.Annotations;

namespace ResourcePackLib.ModelExplorer.Geometry;

[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(TruModelMeshCollectionDebugView))]
public class TruModelMeshCollection : ReadOnlyCollection<TruModelMesh>
{
    public TruModelMeshCollection([NotNull] IList<TruModelMesh> list) : base(list)
    {
        
    }

    internal class TruModelMeshCollectionDebugView
    {
        private readonly TruModelMeshCollection _meshCollection;

        public TruModelMeshCollectionDebugView(TruModelMeshCollection meshCollection)
        {
            _meshCollection = meshCollection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TruModelMesh[] Meshes
        {
            get
            {
                return _meshCollection.ToArray();
            }
        }
        
    }
}