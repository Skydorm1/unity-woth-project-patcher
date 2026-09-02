using System;
using System.IO;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Nomnom.UnityProjectPatcher.Editor;
using Nomnom.UnityProjectPatcher.Editor.Steps;
using UnityEngine;
using UnityEditor;

namespace Skydorm.WotHProjectPatcher.Editor
{
    public readonly struct WotHAddressPathChange : IPatcherStep
    {
        public UniTask<StepResult> Run()
        {
            Debug.Log("[WotH Wrapper] WotHAddressPathChange started.");

            var settings = this.GetSettings();
            string assetsPath = settings.ProjectGameAssetsPath;

            Debug.Log(
                $"[WotH Wrapper] ProjectGameAssetsPath: {assetsPath}"
            );

            AdjustAddressPathBased(assetsPath);
            FilesToAdjust(assetsPath);

            return UniTask.FromResult(StepResult.Success);
        }

        private static void AdjustAddressPathBased(string assetsPath)
        {
            string projectRoot = Directory.GetParent(
                Application.dataPath
            ).FullName;

            string gameRelativePath = assetsPath
                .Replace('\\', '/')
                .TrimEnd('/');

            string gameAbsolutePath = Path.Combine(
                projectRoot,
                gameRelativePath.Replace('/', Path.DirectorySeparatorChar)
            );

            if (!Directory.Exists(gameAbsolutePath))
            {
                Debug.LogError(
                    "[WotH Wrapper] Game path does not exist:\n" +
                    gameAbsolutePath
                );

                return;
            }

            Debug.Log(
                "[WotH Wrapper] Scanning C# files in:\n" +
                gameRelativePath
            );

            string[] files = Directory.GetFiles(
                gameAbsolutePath,
                "*.cs",
                SearchOption.AllDirectories
            );

            int scannedFiles = 0;
            int changedFiles = 0;
            int changedPaths = 0;

            foreach (string filePath in files)
            {
                scannedFiles++;

                string source;

                try
                {
                    source = File.ReadAllText(filePath);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning(
                        "[WotH Wrapper] Could not read file:\n" +
                        filePath +
                        "\n" +
                        exception.Message
                    );

                    continue;
                }

                string patchedSource = source;

                MatchCollection matches = Regex.Matches(
                    source,
                    @"Assets/[^""'\r\n]+"
                );

                foreach (Match match in matches)
                {
                    string foundPath = match.Value;

                    if (foundPath.StartsWith(
                        gameRelativePath + "/",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (!foundPath.StartsWith(
                        "Assets/",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string replacementPath =
                        gameRelativePath +
                        "/" +
                        foundPath.Substring("Assets/".Length);

                    if (patchedSource.Contains(foundPath))
                    {
                        patchedSource = patchedSource.Replace(
                            foundPath,
                            replacementPath
                        );

                        changedPaths++;
                    }
                }

                if (patchedSource == source)
                {
                    continue;
                }

                try
                {
                    File.WriteAllText(
                        filePath,
                        patchedSource
                    );

                    changedFiles++;

                    Debug.Log(
                        "[WotH Wrapper] Patched:\n" +
                        filePath
                    );
                }
                catch (Exception exception)
                {
                    Debug.LogError(
                        "[WotH Wrapper] Could not write file:\n" +
                        filePath +
                        "\n" +
                        exception.Message
                    );
                }
            }

            AssetDatabase.Refresh();

            Debug.Log(
                "[WotH Wrapper] Asset path patch completed.\n" +
                "Files scanned: " +
                scannedFiles +
                "\n" +
                "Files changed: " +
                changedFiles +
                "\n" +
                "Paths changed: " +
                changedPaths
            );
        }

        private static void FilesToAdjust(string assetsPath)
        {
            AdjustAddressPathInFile(
                assetsPath,
                "Scriptable/Levels/City_center.asset"
            );

            AdjustAddressPathInFile(
                assetsPath,
                "Scriptable/Levels/L_CaptainRoom.asset"
            );

            AdjustAddressPathInFolder(
                assetsPath,
                "Scriptable/Features/CreateParticles"
            );

            AdjustAddressPathInFolder(
                assetsPath,
                "Scriptable/Features/Feature_BubbleCtrl"
            );

            AdjustAddressPathInFolder(
                assetsPath,
                "Scriptable/Features/MouseOnShowPrefab"
            );

            AdjustAddressPathInFolder(
                assetsPath,
                "Scriptable/Features/OpenPanel"
            );

            AdjustAddressPathInFolder(
                assetsPath,
                "Scriptable/Features/OpenUi"
            );

            AdjustAddressPathInFolder(
                assetsPath,
                "Scriptable/Features/OverDisplayFeature"
            );

            AdjustAddressPathInFolder(
                assetsPath,
                "Scriptable/Features/SpriteMaskFollowVisualChangeFeature"
            );
        }
        
        private static void AdjustAddressPathInFile(
            string assetsPath,
            string relativeFilePath)
        {
            string projectRoot = Directory.GetParent(
                Application.dataPath
            ).FullName;

            string filePath = Path.Combine(
                projectRoot,
                assetsPath,
                relativeFilePath
            );

            if (!File.Exists(filePath))
            {
                Debug.LogError(
                    "[WotH Wrapper] File not found:\n" +
                    filePath
                );

                return;
            }

            string source;

            try
            {
                source = File.ReadAllText(filePath);
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    "[WotH Wrapper] File could not be read as text:\n" +
                    filePath +
                    "\n" +
                    exception.Message
                );

                return;
            }

            string gameRoot =
                assetsPath.TrimEnd('/') + "/";

            string patchedSource = source;

            MatchCollection matches = Regex.Matches(
                source,
                @"Assets/[^""'\r\n]+"
            );

            int changedPaths = 0;

            foreach (Match match in matches)
            {
                string foundPath = match.Value;

                if (foundPath.StartsWith(
                    gameRoot,
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!foundPath.StartsWith(
                    "Assets/",
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string replacementPath =
                    gameRoot +
                    foundPath.Substring("Assets/".Length);

                patchedSource = patchedSource.Replace(
                    foundPath,
                    replacementPath
                );

                changedPaths++;
            }

            if (patchedSource == source)
            {
                Debug.Log(
                    "[WotH Wrapper] No paths to change in:\n" +
                    relativeFilePath
                );

                return;
            }

            try
            {
                File.WriteAllText(
                    filePath,
                    patchedSource
                );

                Debug.Log(
                    "[WotH Wrapper] Patched " +
                    changedPaths +
                    " asset path(s) in:\n" +
                    relativeFilePath
                );
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "[WotH Wrapper] Could not write file:\n" +
                    filePath +
                    "\n" +
                    exception.Message
                );
            }
        }

        private static void AdjustAddressPathInFolder(
            string assetsPath,
            string relativeFolderPath)
        {
            string projectRoot = Directory.GetParent(
                Application.dataPath
            ).FullName;

            string folderPath = Path.Combine(
                projectRoot,
                assetsPath,
                relativeFolderPath
            );

            if (!Directory.Exists(folderPath))
            {
                Debug.LogError(
                    "[WotH Wrapper] Folder not found:\n" +
                    folderPath
                );

                return;
            }

            Debug.Log(
                "[WotH Wrapper] Scanning folder for asset paths:\n" +
                folderPath
            );

            string[] files = Directory.GetFiles(
                folderPath,
                "*",
                SearchOption.AllDirectories
            );

            int scannedFiles = 0;
            int changedFiles = 0;
            int changedPaths = 0;

            string gameRoot =
                assetsPath.TrimEnd('/') + "/";

            foreach (string filePath in files)
            {
                scannedFiles++;

                string source;

                try
                {
                    source = File.ReadAllText(filePath);
                }
                catch
                {
                    continue;
                }

                string patchedSource = source;

                MatchCollection matches = Regex.Matches(
                    source,
                    @"Assets/[^""'\r\n]+"
                );

                foreach (Match match in matches)
                {
                    string foundPath = match.Value;

                    if (foundPath.StartsWith(
                        gameRoot,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (!foundPath.StartsWith(
                        "Assets/",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string replacementPath =
                        gameRoot +
                        foundPath.Substring("Assets/".Length);

                    if (patchedSource.Contains(foundPath))
                    {
                        patchedSource = patchedSource.Replace(
                            foundPath,
                            replacementPath
                        );

                        changedPaths++;
                    }
                }

                if (patchedSource == source)
                {
                    continue;
                }

                try
                {
                    File.WriteAllText(
                        filePath,
                        patchedSource
                    );

                    changedFiles++;

                    Debug.Log(
                        "[WotH Wrapper] Patched:\n" +
                        filePath
                    );
                }
                catch (Exception exception)
                {
                    Debug.LogWarning(
                        "[WotH Wrapper] Could not write file:\n" +
                        filePath +
                        "\n" +
                        exception.Message
                    );
                }
            }

            Debug.Log(
                "[WotH Wrapper] Folder asset path patch completed.\n" +
                "Folder: " +
                relativeFolderPath +
                "\n" +
                "Files scanned: " +
                scannedFiles +
                "\n" +
                "Files changed: " +
                changedFiles +
                "\n" +
                "Paths changed: " +
                changedPaths
            );
        }

        public void OnComplete(bool failed)
        {
        }
    }
}