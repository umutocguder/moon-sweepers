#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ElevenHopem.EditorScripts
{
    public static class MacBuildScript
    {
        [MenuItem("11Hopem/Build/Build macOS Standalone App (.app)")]
        public static void PerformMacBuild()
        {
            string buildPath = Path.Combine(Directory.GetCurrentDirectory(), "Builds", "macOS", "11HopemMoonSweepers.app");
            string buildDir = Path.GetDirectoryName(buildPath);

            if (!Directory.Exists(buildDir))
            {
                Directory.CreateDirectory(buildDir);
            }

            string[] scenes = new string[] { "Assets/Scenes/MainLunarBase.unity" };

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.None
            };

            Debug.Log($"[11Hopem] Starting macOS Standalone Build to: {buildPath}");
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[11Hopem] macOS Build Succeeded! Size: {summary.totalSize} bytes at: {buildPath}");
                EditorUtility.RevealInFinder(buildPath);
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError($"[11Hopem] macOS Build Failed with {summary.totalErrors} errors!");
            }
        }
    }
}
#endif
