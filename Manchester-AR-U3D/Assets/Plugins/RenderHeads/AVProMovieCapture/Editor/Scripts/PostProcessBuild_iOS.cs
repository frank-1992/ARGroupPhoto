#if UNITY_IOS && UNITY_2017_1_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;

//-----------------------------------------------------------------------------
// Copyright 2012-2020 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProMovieCapture.Editor
{
	public class PostProcessBuild_iOS
	{
		[PostProcessBuild]
		public static void ModifyProject(BuildTarget buildTarget, string path)
		{
			if (buildTarget != BuildTarget.iOS)
				return;

			string projectPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
			PBXProject project = new PBXProject();
			project.ReadFromFile(projectPath);

#if UNITY_2019_3_OR_NEWER
			string targetGuid = project.GetUnityMainTargetGuid();
#else
			string targetGuid = project.TargetGuidByName(PBXProject.GetUnityTargetName());
#endif
			string fileGuid = project.FindFileGuidByProjectPath("Frameworks/Plugins/RenderHeads/AVProMovieCapture/Runtime/Plugins/iOS/AVProMovieCapture.framework");
			if (fileGuid != null)
			{
				PBXProjectExtensions.AddFileToEmbedFrameworks(project, targetGuid, fileGuid);
			}
			else
			{
				Debug.LogWarning("Failed to find AVProMovieCapture.framework in the generated project. You will need to manually set AVProMovieCapture.framework to 'Embed & Sign' in the Xcode project's framework list.");
			}
			project.SetBuildProperty(targetGuid, "LD_RUNPATH_SEARCH_PATHS", "$(inherited) @executable_path/Frameworks");
			project.WriteToFile(projectPath);
		}

		[PostProcessBuild]
		public static void ModfifyPlist(BuildTarget buildTarget, string path)
		{
			if (buildTarget != BuildTarget.iOS)
				return;

			string plistPath = path + "/Info.plist";
			PlistDocument plist = new PlistDocument();
			plist.ReadFromFile(plistPath);

			PlistElementDict rootDict = plist.root;

			// Enable file sharing so that files can be pulled off of the device with iTunes
			rootDict.SetBoolean("UIFileSharingEnabled", true);
			// Enable this so that the files app can access the captured movies
			rootDict.SetBoolean("LSSupportsOpeningDocumentsInPlace", true);

			File.WriteAllText(plistPath, plist.WriteToString());
		}
	}
}
#endif // UNITY_IOS && UNITY_2017_1_OR_NEWER
