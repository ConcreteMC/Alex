using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Alex.Common.Utils
{
	/// <summary>
    /// An octree implementation specifically for chunks.
    /// </summary>
    public sealed class ChunkOctree
    {
        /// <summary>
        /// The maximum number of elements we can have before we need to subdivide.
        /// </summary>
        private const int MaxItemCount = 48;

        private List<BoundingBox> _objects;
        private ChunkOctree[]     _children;
        private BoundingBox       _bounds;

        /// <summary>
        /// Gets this octree's bounds.
        /// </summary>
        public BoundingBox Bounds
        {
            get
            {
                return _bounds;
            }
        }

        /// <summary>
        /// Checks to see if this octree has divided.
        /// </summary>
        public bool HasDivided
        {
            get
            {
                return _children[ 0 ] != null;
            }
        }

        /// <summary>
        /// Creates a new octree.
        /// </summary>
        /// <param name="bounds">The octree's bounds.</param>
        public ChunkOctree( BoundingBox bounds )
        {
            _objects = new List<BoundingBox>( MaxItemCount );
            _children = new ChunkOctree[ 8 ];
            _bounds = bounds;
        }

        /// <summary>
        /// Checks to see if this octree's bounds contains or intersects another bounding box.
        /// </summary>
        /// <param name="box">The bounding box.</param>
        /// <returns></returns>
        public bool Contains( BoundingBox box )
        {
            ContainmentType type = _bounds.Contains( box );
            return type == ContainmentType.Contains
                || type == ContainmentType.Intersects;
        }

        /// <summary>
        /// Adds an object's bounds to this octree.
        /// </summary>
        /// <param name="obj">The bounds of the object to add.</param>
        /// <returns>True if the object was added, false if not.</returns>
        public bool Add( BoundingBox obj )
        {
            // make sure we contain the object
            if ( !Contains( obj ) ) // might need to change
            {
                return false;
            }

            // make sure we can add the object to our children
            if ( _objects.Count < MaxItemCount && !HasDivided )
            {
                _objects.Add( obj );
                return true;
            }
            else
            {
                // check if we need to divide
                if ( !HasDivided )
                {
                    Divide();
                }

                // try to get the child octree that contains the object
                for ( int i = 0; i < 8; ++i )
                {
                    if ( _children[ i ].Add( obj ) )
                    {
                        return true;
                    }
                }

                // honestly, we shouldn't get here
                _objects.Add( obj );
                return true;
            }
        }

        /// <summary>
        /// Removes the given object's bounds from the octree.
        /// </summary>
        /// <param name="obj">The bounds of the object.</param>
        public bool Remove( BoundingBox obj )
        {
            // make sure we contain the object
            if ( !Contains( obj ) )
            {
                return false;
            }

            // check if any children contain it first
            if ( HasDivided )
            {
                for ( int i = 0; i < 8; ++i )
                {
                    if ( _children[ i ].Remove( obj ) )
                    {
                        return true;
                    }
                }
            }

            // now we need to check all of our objects
            int index = _objects.IndexOf( obj );
            if ( index == -1 )
            {
                return false;
            }
            _objects.RemoveAt( index );
            return true;
        }

        /// <summary>
        /// Clears this octree. Subdivisions, if any, will remain intact but will also be cleared.
        /// </summary>
        public void Clear()
        {
            _objects.Clear();
            if ( HasDivided )
            {
                for ( int i = 0; i < 8; ++i )
                {
                    _children[ i ].Clear();
                }
            }
        }

        /// <summary>
        /// Checks to see if the given bounding box collides with this octree.
        /// </summary>
        /// <param name="box">The bounds to check.</param>
        /// <returns></returns>
        public bool Collides( BoundingBox box )
        {
            // make sure we at least contain the given bounding box.
            if ( !Contains( box ) )
            {
                return false;
            }

            // check children
            if ( HasDivided )
            {
                for ( int i = 0; i < 8; ++i )
                {
                    if ( _children[ i ].Collides( box ) )
                    {
                        return true;
                    }
                }
            }

            // check our objects
            for ( int i = 0; i < _objects.Count; ++i )
            {
                ContainmentType type = _objects[ i ].Contains( box );
                if ( type == ContainmentType.Contains || type == ContainmentType.Intersects )
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks to see if the given bounding box collides with this octree.
        /// </summary>
        /// <param name="box">The bounds to check.</param>
        /// <param name="collisions">The list of bounding boxes that the given box collides with.</param>
        /// <returns></returns>
        public bool Collides( BoundingBox box, ref List<BoundingBox> collisions )
        {
            // make sure we at least contain the given bounding box.
            if ( !Contains( box ) )
            {
                return false;
            }

            // check children
            if ( HasDivided )
            {
                for ( int i = 0; i < 8; ++i )
                {
                    _children[ i ].Collides( box, ref collisions );
                }
            }

            // check our blocks
            for ( int i = 0; i < _objects.Count; ++i )
            {
                ContainmentType type = _objects[ i ].Contains( box );
                if ( type == ContainmentType.Contains || type == ContainmentType.Intersects )
                {
                    collisions.Add( _objects[ i ] );
                }
            }

            return collisions.Count > 0;
        }

        /// <summary>
        /// Gets all of the distances of the intersections a ray makes in this octree.
        /// </summary>
        /// <param name="ray">The ray.</param>
        /// <returns></returns>
        public List<float> GetIntersectionDistances( Ray ray )
        {
            var dists = new List<float>();
            GetIntersectionDistances( ray, ref dists );
            return dists;
        }

        /// <summary>
        /// Gets all of the distances of the intersections a ray makes in this octree.
        /// </summary>
        /// <param name="ray">The ray.</param>
        /// <param name="dists">The list of distances to populate.</param>
        /// <returns></returns>
        public void GetIntersectionDistances( Ray ray, ref List<float> dists )
        {
            dists.Clear();

            // if we've divided, check our children first
            if ( HasDivided )
            {
                for ( int i = 0; i < 8; ++i )
                {
                    dists.AddRange( _children[ i ].GetIntersectionDistances( ray ) );
                }
            }

            // now check our objects
            for ( int i = 0; i < _objects.Count; ++i )
            {
                float? value = _objects[ i ].Intersects( ray );
                if ( value.HasValue )
                {
                    dists.Add( value.Value );
                }
            }
        }

        /// <summary>
        /// Divides this octree into its eight children.
        /// </summary>
        private void Divide()
        {
            // make sure we haven't divided already
            if ( HasDivided )
            {
                throw new InvalidOperationException( "This octree has already divided." );
            }

            // get helper variables
            Vector3 center = _bounds.GetCenter();
            Vector3 qdim = _bounds.GetDimensions() * 0.25f;

            // get child centers
            Vector3 trb = new Vector3( center.X + qdim.X, center.Y + qdim.Y, center.Z + qdim.Z );
            Vector3 trf = new Vector3( center.X + qdim.X, center.Y + qdim.Y, center.Z - qdim.Z );
            Vector3 brb = new Vector3( center.X + qdim.X, center.Y - qdim.Y, center.Z + qdim.Z );
            Vector3 brf = new Vector3( center.X + qdim.X, center.Y - qdim.Y, center.Z - qdim.Z );
            Vector3 tlb = new Vector3( center.X - qdim.X, center.Y + qdim.Y, center.Z + qdim.Z );
            Vector3 tlf = new Vector3( center.X - qdim.X, center.Y + qdim.Y, center.Z - qdim.Z );
            Vector3 blb = new Vector3( center.X - qdim.X, center.Y - qdim.Y, center.Z + qdim.Z );
            Vector3 blf = new Vector3( center.X - qdim.X, center.Y - qdim.Y, center.Z - qdim.Z );

            // create children
            _children[ 0 ] = new ChunkOctree( new BoundingBox( tlb - qdim, tlb + qdim ) ); // top left back
            _children[ 1 ] = new ChunkOctree( new BoundingBox( tlf - qdim, tlf + qdim ) ); // top left front
            _children[ 2 ] = new ChunkOctree( new BoundingBox( trb - qdim, trb + qdim ) ); // top right back
            _children[ 3 ] = new ChunkOctree( new BoundingBox( trf - qdim, trf + qdim ) ); // top right front
            _children[ 4 ] = new ChunkOctree( new BoundingBox( blb - qdim, blb + qdim ) ); // bottom left back
            _children[ 5 ] = new ChunkOctree( new BoundingBox( blf - qdim, blf + qdim ) ); // bottom left front
            _children[ 6 ] = new ChunkOctree( new BoundingBox( brb - qdim, brb + qdim ) ); // bottom right back
            _children[ 7 ] = new ChunkOctree( new BoundingBox( brf - qdim, brf + qdim ) ); // bottom right front

            // go through our items and try to move them into children
            for ( int i = 0; i < _objects.Count; ++i )
            {
                for ( int j = 0; j < 8; ++j )
                {
                    if ( _children[ j ].Add( _objects[ i ] ) )
                    {
                        // move the object from this tree to the child
                        _objects.RemoveAt( i );
                        --i;
                        break;
                    }
                }
            }
        }
    }
}