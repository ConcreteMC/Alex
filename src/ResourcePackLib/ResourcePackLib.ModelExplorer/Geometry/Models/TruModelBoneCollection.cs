using System.Collections.ObjectModel;
using System.Diagnostics;
using JetBrains.Annotations;

namespace ResourcePackLib.ModelExplorer.Geometry;

[DebuggerDisplay("{_value}", Name = "{_key}")]
internal class KeyValuePairs
{
    private TruModelBoneCollection _boneCollection;
    private object _key;
    private object _value;

    public KeyValuePairs(TruModelBoneCollection boneCollection, object key, object value)
    {
        _boneCollection = boneCollection;
        _key = key;
        _value = value;
    }
}
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(TruModelBoneCollectionDebugView))]
public class TruModelBoneCollection : ReadOnlyCollection<TruModelBone>
{
    public TruModelBoneCollection([NotNull] IList<TruModelBone> list) : base(list)
    {
        
    }

    
    internal class TruModelBoneCollectionDebugView
    {
        private readonly TruModelBoneCollection _boneCollection;

        public TruModelBoneCollectionDebugView(TruModelBoneCollection boneCollection)
        {
            _boneCollection = boneCollection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePairs[] Bones
        {
            get
            {
                KeyValuePairs[] keys = new KeyValuePairs[_boneCollection.Count];
                for (var i = 0; i < _boneCollection.Count; i++)
                {
                    keys[i] = new KeyValuePairs(_boneCollection, _boneCollection[i].Name, _boneCollection[i]);
                }

                return keys;
            }
        }
        
    }
}