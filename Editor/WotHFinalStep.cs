using System.IO;
using Cysharp.Threading.Tasks;
using Nomnom.UnityProjectPatcher.Editor;
using Nomnom.UnityProjectPatcher.Editor.Steps;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.SceneManagement;

namespace Skydorm.WotHProjectPatcher.Editor
{
    public readonly struct WotHFinalStep : IPatcherStep
    {
        private const string LayoutFileName =
            "WotH Custom Item SDK Layout.wlt";

        private const string SceneAssetPath =
            "Assets/WhisperoftheHouse/Game/Scenes/InGameLevelEditor.unity";

        public UniTask<StepResult> Run()
        {
            Debug.Log("[WotH Wrapper] WotHFinalStep started.");

            var settings = this.GetSettings();

            Debug.Log(
                $"[WotH Wrapper] ProjectGameAssetsPath: {settings.ProjectGameAssetsPath}"
            );

            LoadWotHLayout();

            return UniTask.FromResult(StepResult.Success);
        }

        private static void LoadWotHLayout()
        {
            UnityEditor.PackageManager.PackageInfo packageInfo =
                UnityEditor.PackageManager.PackageInfo.FindForAssembly(
                    typeof(WotHFinalStep).Assembly
                );

            if (packageInfo == null)
            {
                Debug.LogError(
                    "[WotH Wrapper] Could not determine the WotH wrapper package path."
                );

                return;
            }

            string packageRoot = packageInfo.resolvedPath;

            if (string.IsNullOrEmpty(packageRoot))
            {
                Debug.LogError(
                    "[WotH Wrapper] WotH wrapper package resolvedPath is empty."
                );

                return;
            }

            string layoutPath = Path.Combine(
                packageRoot,
                "Layout",
                LayoutFileName
            );

            layoutPath = layoutPath.Replace('\\', '/');

            if (!File.Exists(layoutPath))
            {
                Debug.LogError(
                    $"[WotH Wrapper] Layout file not found: {layoutPath}"
                );

                return;
            }

            Debug.Log(
                $"[WotH Wrapper] Loading WotH editor layout:\n{layoutPath}"
            );

            bool layoutLoaded = EditorUtility.LoadWindowLayout(layoutPath);

            if (!layoutLoaded)
            {
                Debug.LogError(
                    "[WotH Wrapper] Failed to load WotH editor layout."
                );

                return;
            }

            Debug.Log(
                "[WotH Wrapper] WotH editor layout loaded successfully."
            );

            EditorApplication.delayCall += OpenInGameLevelEditorScene;
        }

        private static void OpenInGameLevelEditorScene()
        {
            EditorApplication.delayCall -= OpenInGameLevelEditorScene;

            string projectRoot = Directory.GetParent(
                Application.dataPath
            )!.FullName;

            string absoluteScenePath = Path.Combine(
                projectRoot,
                SceneAssetPath
            );

            absoluteScenePath = absoluteScenePath.Replace('\\', '/');

            if (!File.Exists(absoluteScenePath))
            {
                Debug.LogError(
                    $"[WotH Wrapper] Scene not found: {absoluteScenePath}"
                );

                return;
            }

            Debug.Log(
                $"[WotH Wrapper] Opening scene:\n{SceneAssetPath}"
            );

            try
            {
                EditorSceneManager.OpenScene(
                    SceneAssetPath,
                    OpenSceneMode.Single
                );

                Debug.Log(
                    $"[WotH Wrapper] Scene opened successfully: {SceneAssetPath}"
                );
            }
            catch (System.Exception ex)
            {
                Debug.LogError(
                    $"[WotH Wrapper] Failed to open scene '{SceneAssetPath}':\n{ex}"
                );
            }
        }

        public void OnComplete(bool failed)
        {
        }
    }
}