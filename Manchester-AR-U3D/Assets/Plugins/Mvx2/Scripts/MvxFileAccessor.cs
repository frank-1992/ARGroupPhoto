using UnityEngine;
using System.IO;

namespace MVXUnity
{
    /// <summary> Class responsible for preparing Mvx2 files for streaming. </summary>
    /// <remarks> On some platforms it may be necessary to process the referenced file before it can be accessed. </remarks>
    public class MvxFileAccessor
    {
        private string m_currentOriginalPath = null;
        private string m_currentProcessedPath = null;

        public void Reset()
        {
            m_currentOriginalPath = null;
            m_currentProcessedPath = null;
        }

        public string PrepareFile(string path)
        {
            if (path == m_currentOriginalPath)
                return m_currentProcessedPath;

            if (Path.IsPathRooted(path))
            {
                m_currentOriginalPath = path;
                m_currentProcessedPath = path;
                return m_currentProcessedPath;
            }

            m_currentOriginalPath = path;
            m_currentProcessedPath = ProcessStreamingAssetsFile(path);
            return m_currentProcessedPath;
        }

        private static string ProcessStreamingAssetsFile(string pathInStreamingAssets)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            return ProcessStreamingAssetsFile_Win(pathInStreamingAssets);
#elif !UNITY_EDITOR && UNITY_ANDROID
            return ProcessStreamingAssetsFile_Android(pathInStreamingAssets);
#elif !UNITY_EDITOR && UNITY_IOS
            return ProcessStreamingAssetsFile_IOS(pathInStreamingAssets);
#elif !UNITY_EDITOR && UNITY_LUMIN
            return ProcessStreamingAssetsFile_LuminOS(pathInStreamingAssets);
#else
            throw new System.NotImplementedException("Missing implementation of streaming assets files processor for the current platform");
#endif
        }

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        private static string ProcessStreamingAssetsFile_Win(string pathInStreamingAssets)
        {
            // no processing required here, only compose full path
            return Path.Combine(Application.streamingAssetsPath, pathInStreamingAssets).Replace('\\', '/');
        }
#endif

#if !UNITY_EDITOR && UNITY_ANDROID
        private static string ProcessStreamingAssetsFile_Android(string pathInStreamingAssets)
        {
            // extract the file to the application's persistent data path
            
            string persistentDataFilePath = Path.Combine(Application.persistentDataPath, pathInStreamingAssets).Replace('\\', '/');
            string streamingAssetsFilePath = Path.Combine(Application.streamingAssetsPath, pathInStreamingAssets).Replace('\\', '/');
            Debug.LogFormat("Mvx2: File {0} will be copied to {1}", streamingAssetsFilePath, persistentDataFilePath);
            
            WWW streamingAssetsFileLoader = new WWW(streamingAssetsFilePath);
            while(!streamingAssetsFileLoader.isDone && string.IsNullOrEmpty(streamingAssetsFileLoader.error)) {}
            if (!string.IsNullOrEmpty(streamingAssetsFileLoader.error))
            {
                Debug.LogErrorFormat("Mvx2: Failed to process file from Unity's streaming assets folder. {0}", streamingAssetsFileLoader.error);
                return string.Empty;
            }
            File.WriteAllBytes(persistentDataFilePath, streamingAssetsFileLoader.bytes);
        
            return persistentDataFilePath;
        }
#endif

#if !UNITY_EDITOR && UNITY_IOS
        private static string ProcessStreamingAssetsFile_IOS(string pathInStreamingAssets)
        {
            // no processing required here, only compose full path
            return Path.Combine(Application.streamingAssetsPath, pathInStreamingAssets).Replace('\\', '/');
        }
#endif

#if !UNITY_EDITOR && UNITY_LUMIN
        private static string ProcessStreamingAssetsFile_LuminOS(string pathInStreamingAssets)
        {
            // no processing required here, only compose full path
            return Path.Combine(Application.streamingAssetsPath, pathInStreamingAssets).Replace('\\', '/');
        }
#endif
    }
}

