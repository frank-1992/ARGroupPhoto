using UnityEngine;

namespace MVXUnity
{
    [AddComponentMenu("Mvx2/Data Processors/Mesh per Vertex Colored Renderer")]
    public class MvxMeshPerVertexColoredRenderer : MvxMeshRenderer
    {
        #region process frame

        public static bool SupportsStreamRendering(Mvx2API.SourceInfo sourceInfo)
        {
            bool streamSupported =
                sourceInfo.ContainsDataLayer(Mvx2API.BasicDataLayersGuids.VERTEX_POSITIONS_DATA_LAYER)
                && sourceInfo.ContainsDataLayer(Mvx2API.BasicDataLayersGuids.VERTEX_INDICES_DATA_LAYER)
                && sourceInfo.ContainsDataLayer(Mvx2API.BasicDataLayersGuids.VERTEX_COLORS_DATA_LAYER);
            return streamSupported;
        }

        protected override bool CanProcessStream(Mvx2API.SourceInfo sourceInfo)
        {
            bool streamSupported = SupportsStreamRendering(sourceInfo);
            Debug.LogFormat("Mvx2: MeshPerVertexColored renderer {0} rendering of the new mvx stream", streamSupported ? "supports" : "does not support");
            return streamSupported;
        }

        protected override bool IgnoreUVs()
        {
            return true;
        }

        #endregion

        #region MonoBehaviour

        public override void Reset()
        {
            base.Reset();
#if UNITY_EDITOR
            Material defaultMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Plugins/Mvx2/Materials/MeshPerVertexColored.mat");
            if (defaultMaterial != null)
                materialTemplates = new Material[] { defaultMaterial };
#endif
        }

        #endregion
    }
}