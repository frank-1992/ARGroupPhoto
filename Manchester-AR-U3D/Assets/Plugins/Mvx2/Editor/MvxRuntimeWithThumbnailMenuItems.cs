using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace MVXUnity
{
    public static class MvxRuntimeWithThumbnailMenuItems
    {
        [MenuItem("GameObject/Mvx2/Runtime with Thumbnail/File-source Mesh-point-cloud Simple-Stream")]
        private static void AddFileSourcePointCloudStream()
        {
            CreateFileSourceStreamWithThumbnail<MvxMeshPointCloudRenderer>();
        }

        [MenuItem("GameObject/Mvx2/Runtime with Thumbnail/File-source Mesh-per-vertex-colored Simple-Stream")]
        private static void AddFileSourceMeshPerVertexColoredStream()
        {
            CreateFileSourceStreamWithThumbnail<MvxMeshPerVertexColoredRenderer>();
        }

        [MenuItem("GameObject/Mvx2/Runtime with Thumbnail/File-source Mesh-textured Simple-Stream")]
        private static void AddFileSourceMeshTexturedStream()
        {
            CreateFileSourceStreamWithThumbnail<MvxMeshTexturedRenderer>();
        }

        private static void CreateFileSourceStreamWithThumbnail<RendererType>() where RendererType : MvxMeshRenderer
        {
            GameObject wrapGO = new GameObject("FileSimpleStream");
            wrapGO.transform.SetParent(null);
            wrapGO.transform.localPosition = Vector3.zero;
            wrapGO.transform.localRotation = Quaternion.identity;
            wrapGO.transform.localScale = Vector3.one;

            GameObject streamGO = new GameObject("RuntimeStream");
            streamGO.transform.SetParent(wrapGO.transform);
            streamGO.transform.localPosition = Vector3.zero;
            streamGO.transform.localRotation = Quaternion.identity;
            streamGO.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            
            MvxFileDataStreamDefinition streamDefinition = ScriptableObject.CreateInstance<MvxFileDataStreamDefinition>();
            MvxSimpleDataStream streamGOStream = streamGO.AddComponent<MvxSimpleDataStream>();
            streamGOStream.dataStreamDefinition = streamDefinition;
            MvxMeshRenderer renderer = streamGO.AddComponent<RendererType>();
            renderer.mvxStream = streamGOStream;

            GameObject thumbnailGO = new GameObject("StreamThumbnail");
            thumbnailGO.transform.SetParent(wrapGO.transform);
            thumbnailGO.transform.localPosition = Vector3.zero;
            thumbnailGO.transform.localRotation = Quaternion.identity;
            thumbnailGO.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

            MvxDataStreamThumbnail thumbnailGOStream = thumbnailGO.AddComponent<MvxDataStreamThumbnail>();
            thumbnailGOStream.dataStreamDefinition = streamDefinition;
            renderer = thumbnailGO.AddComponent<RendererType>();
            renderer.mvxStream = thumbnailGOStream;

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
}
