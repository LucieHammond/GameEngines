using GameEngine.Core.Unity.System;
using UnityEngine;

namespace GameEngine.Core.Unity.Descriptors
{
    /// <summary>
    /// A descriptor containing the information needed to configure a mesh
    /// </summary>
    [CreateAssetMenu(fileName = "NewMeshDescriptor", menuName = "Content/Unity Objects/Mesh Descriptor", order = 0)]
    public class MeshDescriptor : ContentDescriptor
    {
        /// <summary>
        /// The positions of the mesh vertices
        /// </summary>
        public Vector3[] Vertices;

        /// <summary>
        /// The triangle faces (successive sequences of 3 vertices) that compose the mesh
        /// </summary>
        public int[] Triangles;

        /// <summary>
        /// The texture coordinates of each mesh vertice in UV space
        /// </summary>
        public Vector2[] UV;
    }
}
