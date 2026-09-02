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
    /// Editor and CI/CD headless build automation script for HappyShoot.
    /// Supports StandaloneWindows64, Android APK, and Android AAB.
    /// Usage in CLI: -batchmode -nographics -quit -executeMethod HappyShoot.Editor.BuildScript.BuildWindows
    ///                -batchmode -nographics -quit -executeMethod HappyShoot.Editor.BuildScript.BuildAndroid
    ///                -batchmode -nographics -quit -executeMethod HappyShoot.Editor.BuildScript.BuildAndroidAab
    /// </summary>
    public static class BuildScript
    {
        private const string WindowsOutputDir = "Builds/StandaloneWindows64";
        private const string WindowsExeName = "HappyShoot.exe";

        private const string AndroidOutputDir = "Builds/Android";
        private const string AndroidApkName = "HappyShoot.apk";
        private const string AndroidAabName = "HappyShoot.aab";

        #region Menu Items

        [MenuItem("HappyShoot/Build/Build Windows 64-bit", priority = 10)]
        public static void BuildWindows()
        {
            ExecuteBuild(
                platformName: "Windows 64-bit",
                target: BuildTarget.StandaloneWindows64,
                outputDir: WindowsOutputDir,
                outputFileName: WindowsExeName
            );
        }

        [MenuItem("HappyShoot/Build/Build Android (APK)", priority = 20)]
        public static void BuildAndroid()
        {
            ExecuteBuild(
                platformName: "Android (APK)",
                target: BuildTarget.Android,
                outputDir: AndroidOutputDir,
                outputFileName: AndroidApkName,
                preBuildConfig: () =>
                {
                    EditorUserBuildSettings.buildAppBundle = false;
                }
            );
        }

        [MenuItem("HappyShoot/Build/Build Android (Google Play AAB)", priority = 30)]
        public static void BuildAndroidAab()
        {
            ExecuteBuild(
                platformName: "Android (AAB)",
                target: BuildTarget.Android,
                outputDir: AndroidOutputDir,
                outputFileName: AndroidAabName,
                preBuildConfig: () =>
                {
                    EditorUserBuildSettings.buildAppBundle = true;
                }
            );
        }

        #endregion

        #region Common Build Pipeline

        private static void ExecuteBuild(
            string platformName,
            BuildTarget target,
            string outputDir,
            string outputFileName,
            Action preBuildConfig = null)
        {
            if (EditorApplication.isCompiling)
            {
                string compilingMsg = "Unity 에디터가 스크립트를 컴파일하는 중입니다. 컴파일이 끝난 후 다시 빌드 버튼을 눌러주세요!";
                Debug.LogWarning("[BuildScript] " + compilingMsg);
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("빌드 대기", compilingMsg, "확인");
                }
                return;
            }

            Debug.Log("[BuildScript] ========================================");
            Debug.Log("[BuildScript] Starting " + platformName + " build...");

            // 1. Collect enabled scenes from EditorBuildSettings
            string[] scenes = EditorBuildSettings.scenes != null
                ? EditorBuildSettings.scenes
                    .Where(s => s != null && s.enabled)
                    .Select(s => s.path)
                    .ToArray()
                : Array.Empty<string>();

            if (scenes.Length == 0)
            {
                string fallbackScene = "Assets/Scenes/SampleScene.unity";
                if (File.Exists(fallbackScene))
                {
                    scenes = new[] { fallbackScene };
                    Debug.LogWarning("[BuildScript] No enabled scenes in EditorBuildSettings. Using fallback: " + fallbackScene);
                }
                else
                {
                    string errorMsg = "[BuildScript] No scenes found to build for " + platformName + "! Aborting.";
                    Debug.LogError(errorMsg);
                    HandleFailure(platformName, errorMsg, 1);
                    return;
                }
            }

            Debug.Log("[BuildScript] Target Scenes (" + scenes.Length + "):\n" + string.Join("\n", scenes));

            // 2. Ensure output directory exists
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            string buildPath = Path.Combine(outputDir, outputFileName);

            // 3. Pre-build custom configuration hook (e.g. AAB flag)
            preBuildConfig?.Invoke();

            // 4. Configure BuildPlayerOptions
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = target,
                options = BuildOptions.None
            };

            // 5. Execute Build
            DateTime startTime = DateTime.UtcNow;
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            TimeSpan duration = DateTime.UtcNow - startTime;

            // 6. Evaluate and Report
            if (summary.result == BuildResult.Succeeded)
            {
                float sizeMb = summary.totalSize / (1024f * 1024f);
                string successMsg = "[BuildScript] [OK] " + platformName + " Build SUCCEEDED in " + duration.TotalSeconds.ToString("F1") + "s! Output: " + buildPath + " (" + sizeMb.ToString("F1") + " MB)";
                Debug.Log(successMsg);

                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
                else
                {
                    EditorUtility.DisplayDialog(
                        platformName + " Build Succeeded",
                        platformName + " 빌드가 성공적으로 완료되었습니다!\n\n경로: " + buildPath + "\n소요 시간: " + duration.TotalSeconds.ToString("F1") + "초\n크기: " + sizeMb.ToString("F1") + " MB",
                        "확인"
                    );
                }
            }
            else
            {
                string failMsg = "[BuildScript] [FAIL] " + platformName + " Build FAILED with result '" + summary.result + "'. Errors: " + summary.totalErrors + ", Warnings: " + summary.totalWarnings;
                Debug.LogError(failMsg);

                foreach (var step in report.steps)
                {
                    foreach (var msg in step.messages)
                    {
                        if (msg.type == LogType.Error || msg.type == LogType.Exception)
                        {
                            Debug.LogError("[BuildScript Step Error] " + msg.content);
                        }
                    }
                }

                HandleFailure(platformName, failMsg, summary.totalErrors > 0 ? summary.totalErrors : 1);
            }
        }

        private static void HandleFailure(string platformName, string errorDetails, int exitCode)
        {
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(exitCode);
            }
            else
            {
                EditorUtility.DisplayDialog(
                    platformName + " Build Failed",
                    platformName + " 빌드 실패!\n\n" + errorDetails + "\n\nUnity Console 로그를 확인해주세요.",
                    "확인"
                );
            }
        }

        #endregion
    }
}
#endif
