using System;
using UnityEngine;

namespace MVXUnity
{
    [ExecuteInEditMode]
    public class MvxMeshPart : MonoBehaviour
    {
        #region data

        [SerializeField, HideInInspector] private MeshRenderer m_meshRenderer = null;
        [SerializeField, HideInInspector] private MeshFilter m_meshFilter = null;

        // an array of meshes - they are switched between updates to improve performance -> meshes double-buffering
        private Mesh[] m_meshes = new Mesh[2];
        private int m_activeMeshIndex = -1;

        #endregion

        #region update

        public void SetMaterials(Material[] sharedMaterials)
        {
            m_meshRenderer.sharedMaterials = sharedMaterials != null ? sharedMaterials : new Material[0];
        }

        public void UpdateMesh(
            Vector3[] vertices, Vector3[] normals, Color32[] colors, Vector2[] uvs,
            Int32[] indices, MeshTopology meshTopology)
        {
            m_activeMeshIndex = (m_activeMeshIndex + 1) % m_meshes.Length;
            Mesh newActiveMesh = m_meshes[m_activeMeshIndex];

            newActiveMesh.Clear();      // this shall be called always before rebuilding the mesh
            newActiveMesh.vertices = vertices;
            newActiveMesh.normals = normals;
            newActiveMesh.uv = uvs;
            newActiveMesh.colors32 = colors;
            newActiveMesh.SetIndices(indices, meshTopology, 0, false);
            newActiveMesh.UploadMeshData(false);

            m_meshFilter.sharedMesh = newActiveMesh;
        }

        public void ClearMesh()
        {
            foreach (Mesh mesh in m_meshes)
                mesh.Clear();
        }

        #endregion

        #region MonoBehaviour

        private void Awake()
        {
            if (m_meshFilter == null)
                m_meshFilter = gameObject.AddComponent<MeshFilter>();
            if (m_meshRenderer == null)
                m_meshRenderer = gameObject.AddComponent<MeshRenderer>();

            for (int meshIndex = 0; meshIndex < m_meshes.Length; meshIndex++)
            {
                Mesh mesh = new Mesh();
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                mesh.MarkDynamic();
                m_meshes[meshIndex] = mesh;
            }
        }

        private void OnDestroy()
        {
            for (int meshIndex = 0; meshIndex < m_meshes.Length; meshIndex++)
            {
                Mesh mesh = m_meshes[meshIndex];
                if (Application.isPlaying)
                    Destroy(mesh);
                else
                    DestroyImmediate(mesh);

                m_meshes[meshIndex] = null;
            }

            if (Application.isPlaying)
            {
                Destroy(m_meshRenderer);
                Destroy(m_meshFilter);
            }

            m_meshRenderer = null;
            m_meshFilter = null;
        }

        #endregion
    }
}
