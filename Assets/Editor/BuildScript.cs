#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace HappyShoot.Editor
{
    /// <summary>
    /// Headless CI/CD build script for automated GitHub Actions build pipelines.
    /// Usage in CLI: -batchmode -nographics -quit -executeMethod HappyShoot.Editor.BuildScript.BuildWindows
    /// </summary>
    public static class BuildScript
    {
        private const string BuildOutputDirectory = "Builds/StandaloneWindows64";
        private const string ExecutableName = "HappyShoot.exe";

        [MenuItem("HappyShoot/Build/Build Windows 64-bit")]
        public static void BuildWindows()
        {
            Debug.Log("[BuildScript] Starting Windows 64-bit Standalone build...");

            // 1. Gather all enabled scenes from EditorBuildSettings
            string[] scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                // Fallback to SampleScene if none enabled
                string fallbackScene = "Assets/Scenes/SampleScene.unity";
                if (File.Exists(fallbackScene))
                {
                    scenes = new[] { fallbackScene };
                    Debug.LogWarning($"[BuildScript] No enabled scenes in EditorBuildSettings. Using fallback: {fallbackScene}");
                }
                else
                {
                    Debug.LogError("[BuildScript] No scenes found to build! Aborting.");
                    if (Application.isBatchMode) EditorApplication.Exit(1);
                    return;
                }
            }

            Debug.Log($"[BuildScript] Target Scenes ({scenes.Length}):\n" + string.Join("\n", scenes));

            // 2. Ensure output directory exists
            if (!Directory.Exists(BuildOutputDirectory))
            {
                Directory.CreateDirectory(BuildOutputDirectory);
            }

            string buildPath = Path.Combine(BuildOutputDirectory, ExecutableName);

            // 3. Configure BuildPlayerOptions
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            // 4. Execute Build
            DateTime startTime = DateTime.UtcNow;
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            TimeSpan duration = DateTime.UtcNow - startTime;

            // 5. Evaluate and Report
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[BuildScript] ✅ Build SUCCEEDED in {duration.TotalSeconds:F1}s! Output: {buildPath} ({summary.totalSize / (1024 * 1024):F1} MB)");
                if (Application.isBatchMode) EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError($"[BuildScript] ❌ Build FAILED with result '{summary.result}'. Errors: {summary.totalErrors}, Warnings: {summary.totalWarnings}");
                foreach (var step in report.steps)
                {
                    foreach (var msg in step.messages)
                    {
                        if (msg.type == LogType.Error || msg.type == LogType.Exception)
                        {
                            Debug.LogError($"[BuildScript Step Error] {msg.content}");
                        }
                    }
                }
                if (Application.isBatchMode) EditorApplication.Exit(1);
            }
        }
    }
}
#endif
