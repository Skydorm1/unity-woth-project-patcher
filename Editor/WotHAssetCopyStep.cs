using System;
using System.IO;
using Cysharp.Threading.Tasks;
using Nomnom.UnityProjectPatcher.Editor;
using Nomnom.UnityProjectPatcher.Editor.Steps;
using UnityEditor;
using UnityEngine;

namespace Skydorm.WotHProjectPatcher.Editor
{
    public readonly struct WotHAssetCopyStep : IPatcherStep
    {
        public UniTask<StepResult> Run()
        {
            Debug.Log("[WotH Wrapper] WotHAssetCopyStep started.");

            var settings = this.GetSettings();
            string assetsPath = settings.ProjectGameAssetsPath;

            CopyFuckTMp(assetsPath);

            return UniTask.FromResult(StepResult.Success);
        }

        private static void CopyFuckTMp(string assetsPath)
        {
            string sourcePath = Path.Combine(
                assetsPath,
                "Arts/font/FuckTMp 1.asset"
            ).Replace('\\', '/');

            string targetPath = Path.Combine(
                assetsPath,
                "Arts/font/FuckTMp.asset"
            ).Replace('\\', '/');

            Debug.Log(
                "[WotH Wrapper] Replacing FuckTMp.asset:\n" +
                $"Source: {sourcePath}\n" +
                $"Target: {targetPath}"
            );

            if (!File.Exists(sourcePath))
            {
                Debug.LogError(
                    $"[WotH Wrapper] Source asset not found:\n{sourcePath}"
                );

                return;
            }

            try
            {
                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);

                    Debug.Log(
                        $"[WotH Wrapper] Deleted existing asset:\n{targetPath}"
                    );
                }

                File.Copy(
                    sourcePath,
                    targetPath
                );

                Debug.Log(
                    $"[WotH Wrapper] Copied:\n{sourcePath}\n→\n{targetPath}"
                );

                AssetDatabase.Refresh();

                Debug.Log(
                    "[WotH Wrapper] FuckTMp.asset replacement finished."
                );
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "[WotH Wrapper] Failed to replace FuckTMp.asset:\n" +
                    exception
                );
            }
        }

        public void OnComplete(bool failed)
        {
        }
    }
}