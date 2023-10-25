using UnityEngine;
using System.IO;

namespace MVXUnity
{
    public static class MvxPluginsLoader
    {
        private static bool s_pluginsLoaded = false;

        public static void LoadPlugins()
        {
            if (s_pluginsLoaded)
                return;
#if !UNITY_EDITOR && UNITY_IOS
            // nothing to load here, plugins are already embedded on iOS platform
#else
            string pluginsDirectory = DeterminePluginsDirectory();
            Debug.LogFormat("Mvx2: Loading plugins ({0})", pluginsDirectory);

            Mvx2API.PluginsLoader.LoadPluginsInFolder(pluginsDirectory, false, false);
#endif
            s_pluginsLoaded = true;
        }

        private static string DeterminePluginsDirectory()
        {
#if UNITY_EDITOR_WIN
            return GetPluginsDirectory_EditorWin();
#elif UNITY_EDITOR_OSX
            return GetPluginsDirectory_EditorMacOS();
#elif UNITY_STANDALONE_WIN
            return GetPluginsDirectory_RuntimeWin();
#elif UNITY_STANDALONE_OSX
            return GetPluginsDirectory_RuntimeMacOS();
#elif UNITY_LUMIN
            return GetPluginsDirectory_LuminOS();
#elif !UNITY_EDITOR && UNITY_ANDROID
            return GetPluginsDirectory_Android();
#else
            throw new System.NotImplementedException("Missing implementation of plugins directory getter for the current platform");
#endif
        }

#if UNITY_EDITOR_WIN
        private static string GetPluginsDirectory_EditorWin()
        {
            return Path.Combine(Path.Combine(
                MvxGlobal.pluginLocation, 
                "plugins"),
                "windows");
        }
#endif

#if UNITY_EDITOR_OSX
        private static string GetPluginsDirectory_EditorMacOS()
        {
            return Path.Combine(Path.Combine(
                MvxGlobal.pluginLocation, 
                "plugins"), 
                "macos_unity");
        }
#endif

#if UNITY_STANDALONE_WIN
        private static string GetPluginsDirectory_RuntimeWin()
        {
            return Path.Combine(Application.dataPath, "Plugins");
        }
#endif

#if UNITY_STANDALONE_OSX
        private static string GetPluginsDirectory_RuntimeMacOS()
        {
            return Path.Combine(Application.dataPath, "Plugins");
        }
#endif

#if !UNITY_EDITOR && UNITY_ANDROID
        private static string GetPluginsDirectory_Android()
        {
            AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            if (unityPlayerClass == null)
            {
                Debug.LogError("Mvx2: Failed to get plugins directory: UnityPlayer class not found");
                return string.Empty;
            }

            AndroidJavaObject currentAndroidActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
            if (currentAndroidActivity == null)
            {
                Debug.LogError("Mvx2: Failed to get plugins directory: Current activity object not found");
                return string.Empty;
            }

            AndroidJavaObject applicationInfo = currentAndroidActivity.Call<AndroidJavaObject>("getApplicationInfo");
            if (applicationInfo == null)
            {
                Debug.LogError("Mvx2: Failed to get plugins directory: Application info object not found");
                return string.Empty;
            }

            return applicationInfo.Get<string>("nativeLibraryDir");
        }
#endif

#if UNITY_LUMIN
        private static string GetPluginsDirectory_LuminOS()
        {
            return Mvx2API.Utils.GetAppExeDirectory().NetString;
        }
#endif
    }
}
