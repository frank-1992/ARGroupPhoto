#if UNITY_IOS

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.iOS.Xcode;
#if UNITY_2018
using UnityEditor.Build.Reporting;
#endif

namespace MVXUnity
{
    /// <summary>
    /// Intention of this class is to do additional changes to generatex Xcode project.
    /// This script is called by Unity after finishing iOS build.
    /// </summary>
#if UNITY_2018
    public class PostprocessBuildIOS : IPostprocessBuildWithReport
#else
    public class PostprocessBuildIOS : IPostprocessBuild
#endif
    {
        public int callbackOrder
        {
            get { return 999; } // No matter what is here
        }

#if UNITY_2018
        public void OnPostprocessBuild(BuildReport report)
        {
            OnPostprocessBuild(report.summary.platform, report.summary.outputPath);
        }
#endif

        /// <inheritdoc />
        /// <remarks>
        /// Postprocessing of generated Xcode project.
        /// Collects all user frameworks and adds them as embedded to Xcode project.
        /// </remarks>
        public void OnPostprocessBuild(BuildTarget target, string path)
        {
            if (target != BuildTarget.iOS)
                return;

            string projectPath = PBXProject.GetPBXProjectPath(path);
            PBXProject xproject = new PBXProject();
            xproject.ReadFromString(File.ReadAllText(projectPath));

            string targetGuid = xproject.GetUnityMainTargetGuid();

            string embedPhaseGuid = xproject.AddCopyFilesBuildPhase(targetGuid, "Embed Frameworks", "", "10");
            xproject.AddBuildProperty(targetGuid, "LD_RUNPATH_SEARCH_PATHS", "$(inherited) @executable_path/Frameworks");

            xproject.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "false");
            xproject.SetBuildProperty(xproject.GetUnityFrameworkTargetGuid(), "ENABLE_BITCODE", "false");

            foreach (string framework in FindFrameworks())
            {
                Debug.LogFormat("Adding framework `{0}`", framework);
                string frameworkPathInProject = "Frameworks/" + framework;

                var fileGuid = xproject.FindFileGuidByProjectPath(frameworkPathInProject);
                if (fileGuid == null)
                {
                    Debug.LogFormat("Framework `{0}` NOT FOUND in generated xCode project.", frameworkPathInProject);
                    continue;
                }

                xproject.AddFileToBuildSection(targetGuid, embedPhaseGuid, fileGuid);
            }

            // Hack to Enable Code Sign on Copy attribute for embedded frameworks
            // Unity 2017.2.0f3 does not provide necessary API so replace in text required.
            string content = System.Text.RegularExpressions.Regex.Replace(
                    xproject.WriteToString(),
                    @"(.*\.framework in Embed Frameworks \*/ = .*) \};",
                    @"$1 settings = {ATTRIBUTES = (CodeSignOnCopy, ); }; };"
                );

            File.WriteAllText(projectPath, content);
        }

        /// <summary>
        /// Enumerates over all iOS frameworks found in Plugins/Mvx2.
        /// Returned path is relative to Assets folder
        /// </summary>
        /// <returns>Paths to all iOS frameworks found in Mvx2.</returns>
        private IEnumerable<string> FindFrameworks()
        {
            var basePath = Application.dataPath + "/";
            var dirs = Directory.GetDirectories(
                basePath + "Plugins/Mvx2", "*.framework", SearchOption.AllDirectories);

            for (int i = 0; i < dirs.Length; ++i)
                dirs[i] = dirs[i].Replace(basePath, "").Replace('\\', '/');

            return dirs;
        }
    }
}

#endif
