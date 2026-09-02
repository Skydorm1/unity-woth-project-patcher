using Cysharp.Threading.Tasks;
using Nomnom.UnityProjectPatcher.Editor;
using Nomnom.UnityProjectPatcher.Editor.Steps;
using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Skydorm.WotHProjectPatcher.Editor
{
    public readonly struct WotHFebucciStep : IPatcherStep
    {
        public UniTask<StepResult> Run()
        {
            Debug.Log("[WotH Wrapper] WotHFebucciStep started.");

            var settings = this.GetSettings();
            string assetsPath = settings.ProjectGameAssetsPath;
            
            bool ownsFebucci = EditorUtility.DisplayDialog(
                "Febucci Text Animator",
                "Do you own Febucci Text Animator and have it available in the Unity Package Manager?\n\n" +
                "If yes, the wrapper will apply the required Febucci compatibility patches.\n\n" +
                "If no, the Febucci-specific patches will be skipped.",
                "Yes, I own it",
                "No, I don't"
            );

            if (ownsFebucci)
            {
                Debug.Log(
                    "[WotH Wrapper] User owns Febucci. Applying Febucci patches..."
                );

                PatchFebucci(assetsPath);
            }
            else
            {
                Debug.Log(
                    "[WotH Wrapper] User does not own Febucci. Skipping Febucci patches."
                );
            }

            return UniTask.FromResult(StepResult.Success);
        }

        private static void PatchFebucci(string assetsPath)
        {
            Debug.Log("[WotH Wrapper] Starting Febucci compatibility patch...");

            string projectRoot =
                Directory.GetParent(Application.dataPath).FullName;

            string oldFebucciDllPath = Path.Combine(
                projectRoot,
                "Assets",
                "WhisperoftheHouse",
                "Plugins",
                "Febucci.TextAnimator.Runtime.dll"
            );

            if (File.Exists(oldFebucciDllPath))
            {
                try
                {
                    File.Delete(oldFebucciDllPath);

                    Debug.Log(
                        $"[WotH Wrapper] Removed old Febucci DLL:\n{oldFebucciDllPath}"
                    );
                }
                catch (Exception exception)
                {
                    Debug.LogError(
                        "[WotH Wrapper] Failed to remove old Febucci DLL:\n" +
                        oldFebucciDllPath +
                        "\n" +
                        exception
                    );
                }
            }
            else
            {
                Debug.Log(
                    "[WotH Wrapper] Old Febucci DLL was not found. Nothing to remove."
                );
            }

            string scriptsPath = Path.Combine(
                projectRoot,
                assetsPath.Replace('/', Path.DirectorySeparatorChar),
                "Scripts",
                "Assembly-CSharp"
            );

            if (!Directory.Exists(scriptsPath))
            {
                Debug.LogError(
                    $"[WotH Wrapper] Febucci script directory not found:\n{scriptsPath}"
                );

                return;
            }

            string[] scriptsToPatch =
            {
                "TypeWritterAudio.cs",
                "DialogueItemCtrl.cs",
                "TalkTestTool.cs",
                "BugCountDownInside.cs",
                "BugCountDown.cs",
                "PopUpCtrl.cs"
            };

            foreach (string scriptName in scriptsToPatch)
            {
                string scriptPath = Path.Combine(
                    scriptsPath,
                    scriptName
                );

                PatchFebucciScript(scriptPath);
            }

            AssetDatabase.Refresh();

            Debug.Log(
                "[WotH Wrapper] Febucci compatibility patch completed."
            );
        }

        private static void PatchFebucciScript(string scriptPath)
        {
            if (!File.Exists(scriptPath))
            {
                Debug.LogWarning(
                    $"[WotH Wrapper] Febucci script not found, skipping:\n{scriptPath}"
                );

                return;
            }

            string source;

            try
            {
                source = File.ReadAllText(scriptPath);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "[WotH Wrapper] Failed to read Febucci script:\n" +
                    scriptPath +
                    "\n" +
                    exception
                );

                return;
            }

            if (!source.Contains("TextAnimatorPlayer"))
            {
                Debug.Log(
                    $"[WotH Wrapper] No TextAnimatorPlayer reference found in:\n{scriptPath}"
                );

                return;
            }

            string patchedSource =
                source.Replace(
                    "TextAnimatorPlayer",
                    "TypewriterByCharacter"
                );

            if (patchedSource == source)
                return;

            try
            {
                File.WriteAllText(
                    scriptPath,
                    patchedSource
                );

                Debug.Log(
                    "[WotH Wrapper] Patched Febucci script:\n" +
                    scriptPath +
                    "\n" +
                    "TextAnimatorPlayer -> TypewriterByCharacter"
                );
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "[WotH Wrapper] Failed to write Febucci script:\n" +
                    scriptPath +
                    "\n" +
                    exception
                );
            }
        }

        public void OnComplete(bool failed)
        {
        }
    }
}