using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace MVXUnity
{
    public static class MvxThumbnailMenuItems
    {
        [MenuItem("GameObject/Mvx2/Thumbnails/Mesh-point-cloud Thumbnail")]
        private static void AddMeshPointCloudThumbnail()
        {
            CreateThumbnail<MvxMeshPointCloudRenderer>();
        }

        [MenuItem("GameObject/Mvx2/Thumbnails/Mesh-per-vertex-colored Thumbnail")]
        private static void AddMeshPerVertexColoredThumbnail()
        {
            CreateThumbnail<MvxMeshPerVertexColoredRenderer>();
        }

        [MenuItem("GameObject/Mvx2/Thumbnails/Mesh-textured Thumbnail")]
        private static void AddMeshTexturedThumbnail()
        {
            CreateThumbnail<MvxMeshTexturedRenderer>();
        }

        private static void CreateThumbnail<RendererType>() where RendererType : MvxMeshRenderer
        {
            GameObject thumbnailGO = new GameObject("DataStreamThumbnail");
            thumbnailGO.transform.SetParent(null);
            thumbnailGO.transform.localPosition = Vector3.zero;
            thumbnailGO.transform.localRotation = Quaternion.identity;
            thumbnailGO.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

            MvxFileDataStreamDefinition streamDefinition = ScriptableObject.CreateInstance<MvxFileDataStreamDefinition>();
            MvxDataStreamThumbnail stream = thumbnailGO.AddComponent<MvxDataStreamThumbnail>();
            stream.dataStreamDefinition = streamDefinition;
            MvxMeshRenderer renderer = thumbnailGO.AddComponent<RendererType>();
            renderer.mvxStream = stream;

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
}
