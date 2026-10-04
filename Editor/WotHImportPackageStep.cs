using System.IO;
using System;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Nomnom.UnityProjectPatcher.Editor;
using Nomnom.UnityProjectPatcher.Editor.Steps;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;

namespace Skydorm.WotHProjectPatcher.Editor
{
    public readonly struct WotHImportPackageStep : IPatcherStep
    {
        public UniTask<StepResult> Run()
        {
            Debug.Log("[WotH Wrapper] WotHShaderPatchStep started.");

            var settings = this.GetSettings();
            string assetsPath = settings.ProjectGameAssetsPath;

            Debug.Log(
                $"[WotH Wrapper] ProjectGameAssetsPath: {assetsPath} - should be Assets/WhisperoftheHouse/Game"
            );

            ImportCustomItemSdkPackage();

            return UniTask.FromResult(StepResult.Success);
        }

        private static void ImportCustomItemSdkPackage()
        {
            const string packageFileName = "WotH Custom Item SDK Package.unitypackage";

            UnityEditor.PackageManager.PackageInfo packageInfo =
                UnityEditor.PackageManager.PackageInfo.FindForAssembly(
                    typeof(WotHImportPackageStep).Assembly
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

            string unityPackagePath =
                Path.Combine(
                    packageRoot,
                    "Packages",
                    packageFileName
                );

            unityPackagePath = unityPackagePath.Replace('\\', '/');

            if (!File.Exists(unityPackagePath))
            {
                Debug.LogError(
                    $"[WotH Wrapper] Custom Item SDK package not found: {unityPackagePath}"
                );

                return;
            }

            Debug.Log(
                $"[WotH Wrapper] Importing Custom Item SDK package:\n{unityPackagePath}"
            );

            AssetDatabase.ImportPackage(
                unityPackagePath,
                false
            );

            Debug.Log(
                $"[WotH Wrapper] Custom Item SDK package import started successfully."
            );
        }

        public void OnComplete(bool failed)
        {
        }
    }
}