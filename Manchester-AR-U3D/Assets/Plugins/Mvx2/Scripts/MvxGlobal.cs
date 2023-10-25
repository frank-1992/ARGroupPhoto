using UnityEngine;
using System.IO;

namespace MVXUnity
{
    public static class MvxGlobal
    {
        private static string s_pluginLocation = ComposePluginLocation();
        public static string pluginLocation
        {
            get { return s_pluginLocation; }
        }

        private static string ComposePluginLocation()
        {
            return Path.Combine(Path.Combine(
                Application.dataPath,
                "Plugins"),
                "Mvx2");
        }
    }
}
