using UnityEngine;

namespace GameEngine.Core.Descriptors
{
    /// <summary>
    /// A descriptor containing the information needed to configure a mesh
    /// </summary>
    public struct MeshDescriptor
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
