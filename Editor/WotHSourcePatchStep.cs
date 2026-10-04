using System.IO;
using System;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Nomnom.UnityProjectPatcher.Editor;
using Nomnom.UnityProjectPatcher.Editor.Steps;
using UnityEngine;

namespace Skydorm.WotHProjectPatcher.Editor
{
    public readonly struct WotHSourcePatchStep : IPatcherStep
    {
        public UniTask<StepResult> Run()
        {
            Debug.Log("[WotH Wrapper] WotHSourcePatchStep started.");
            var settings = this.GetSettings();
            string assetsPath = settings.ProjectGameAssetsPath;

            Debug.Log($"[WotH Wrapper] ProjectGameAssetsPath: {assetsPath}");
            PatchMirrorSyncVars(
                assetsPath,
                "Scripts/Assembly-CSharp/QuickStart/Net_SceneScript.cs"
            );

            PatchMirrorSyncVars(
                assetsPath,
                "Scripts/Assembly-CSharp/Net_PlayerScriptTemp.cs"
            );

            PatchTabButtonTypes(assetsPath);
            PatchLightCollider2D(assetsPath);
            PatchSteam(assetsPath);
            PatchSteamDLCTools(assetsPath);
            PatchSaveManagerEmission(assetsPath);
            PatchGameManager(assetsPath);
            PatchMoneyManager(assetsPath);
            PatchUIManager(assetsPath);
            return UniTask.FromResult(StepResult.Success);
        }

        private static void PatchMirrorSyncVars(
            string assetsPath,
            string relativePath)
        {
            string path = Path.Combine(assetsPath, relativePath);

            if (!File.Exists(path))
            {
                Debug.LogWarning(
                    $"[WotH Wrapper] Source file not found: {path}"
                );

                return;
            }

            string source = File.ReadAllText(path);
            string original = source;

            source = Regex.Replace(
                source,
                @"\bpublic\s+override\s+void\s+SerializeSyncVars\s*\(",
                "protected override void SerializeSyncVars("
            );

            source = Regex.Replace(
                source,
                @"\bpublic\s+override\s+void\s+DeserializeSyncVars\s*\(",
                "protected override void DeserializeSyncVars("
            );

            if (source != original)
            {
                File.WriteAllText(path, source);

                Debug.Log(
                    $"[WotH Wrapper] Patched Mirror SyncVars: {relativePath}"
                );
            }
        }

        private static void PatchTabButtonTypes(string assetsPath)
        {
            PatchExact(
                assetsPath,
                "Scripts/Assembly-CSharp/BagPanel.cs",
                "private TabButton tabButton;",
                "private Water.UI.TabButton tabButton;"
            );

            PatchExact(
                assetsPath,
                "Scripts/Assembly-CSharp/BagPanel.cs",
                "private TabButton shopModel;",
                "private Water.UI.TabButton shopModel;"
            );

            PatchExact(
                assetsPath,
                "Scripts/Assembly-CSharp/UITabView.cs",
                "public List<TabButton> Tabs = new List<TabButton>();",
                "public List<Water.UI.TabButton> Tabs = new List<Water.UI.TabButton>();"
            );

            PatchExact(
                assetsPath,
                "Scripts/Assembly-CSharp/UITabView.cs",
                "TabButton tabButton = Tabs[i];",
                "Water.UI.TabButton tabButton = Tabs[i];"
            );

            PatchExact(
                assetsPath,
                "Scripts/Assembly-CSharp/UITabView.cs",
                "public void AddTab(TabButton tabButton)",
                "public void AddTab(Water.UI.TabButton tabButton)"
            );

            PatchExact(
                assetsPath,
                "Scripts/Assembly-CSharp/UITabView.cs",
                "foreach (TabButton tab in Tabs)",
                "foreach (Water.UI.TabButton tab in Tabs)"
            );

            PatchExact(
                assetsPath,
                "Scripts/Assembly-CSharp/BagCategoryBar.cs",
                "foreach (TabButton tab in obj.Tabs)",
                "foreach (Water.UI.TabButton tab in obj.Tabs)"
            );

            PatchExact(
                assetsPath,
                "Scripts/Assembly-CSharp/CharacterContent.cs",
                "private TabButton exTabButton;",
                "private Water.UI.TabButton exTabButton;"
            );

            PatchExact(
                assetsPath,
                "Scripts/Assembly-CSharp/HorizontalButtonGroup.cs",
                "private TabButton btn_interactCtrl;",
                "private Water.UI.TabButton btn_interactCtrl;"
            );

            PatchExact(
                assetsPath,
                "Scripts/Assembly-CSharp/HorizontalButtonGroup.cs",
                "private TabButton btn_changeItemInfo;",
                "private Water.UI.TabButton btn_changeItemInfo;"
            );

            PatchExact(
                assetsPath,
                "Scripts/Assembly-CSharp/HorizontalButtonGroup.cs",
                "TabButton tabButton = btn_interactCtrl;",
                "Water.UI.TabButton tabButton = btn_interactCtrl;"
            );

            PatchExact(
                assetsPath,
                "Scripts/Assembly-CSharp/UIShopPanel.cs",
                "private TabButton tabButton;",
                "private Water.UI.TabButton tabButton;"
            );

            PatchExact(
                assetsPath,
                "Scripts/Assembly-CSharp/UIShopPanel.cs",
                "TabButton tabButton = UnityEngine.Object.Instantiate(this.tabButton, uITabView.transform);",
                "Water.UI.TabButton tabButton = UnityEngine.Object.Instantiate(this.tabButton, uITabView.transform);"
            );

            PatchExact(
                assetsPath,
                "Scripts/Assembly-CSharp/UIShopPanel.cs",
                "tabButton.SetState(TabButton.Transition.Sprite, shopItemOne.normalSprite, shopItemOne.highlightSprite);",
                "tabButton.SetState(Water.UI.TabButton.Transition.Sprite, shopItemOne.normalSprite, shopItemOne.highlightSprite);"
            );
        }

        private static void PatchLightCollider2D(string assetsPath)
        {
            string path = Path.Combine(
                assetsPath,
                "Plugins/Assembly-CSharp-firstpass/FunkyCode/LightCollider2D.cs"
            );

            if (!File.Exists(path))
            {
                Debug.LogError(
                    $"[WotH Wrapper] File not found: {path}"
                );

                return;
            }

            string text = File.ReadAllText(path);

            text = text.Replace(
                "public LightEvent lightOnEnter;",
                "public FunkyCode.LightSettings.LightEvent lightOnEnter;"
            );

            text = text.Replace(
                "public LightEvent lightOnExit;",
                "public FunkyCode.LightSettings.LightEvent lightOnExit;"
            );

            text = text.Replace(
                "lightOnEnter = new LightEvent();",
                "lightOnEnter = new FunkyCode.LightSettings.LightEvent();"
            );

            text = text.Replace(
                "lightOnExit = new LightEvent();",
                "lightOnExit = new FunkyCode.LightSettings.LightEvent();"
            );

            File.WriteAllText(path, text);
        }

        private static void PatchSteam(string assetsPath)
        {
            string steamManagerPath = Path.Combine(
                assetsPath,
                "Scripts/Assembly-CSharp/SteamManager.cs"
            );

            if (!File.Exists(steamManagerPath))
            {
                Debug.LogWarning(
                    $"[WotH Wrapper] SteamManager.cs not found: {steamManagerPath}"
                );

                return;
            }

            string source = File.ReadAllText(steamManagerPath);

            string patched = source;

            patched = EmptyMethodBody(
                patched,
                "Awake"
            );

            patched = EmptyMethodBody(
                patched,
                "Update"
            );

            if (patched != source)
            {
                File.WriteAllText(
                    steamManagerPath,
                    patched
                );

                Debug.Log(
                    "[WotH Wrapper] Disabled SteamManager.Awake() and SteamManager.Update()."
                );
            }
            else
            {
                Debug.Log(
                    "[WotH Wrapper] SteamManager.Awake() and SteamManager.Update() were already patched."
                );
            }

            PatchExact(
                assetsPath,
                "Scripts/Assembly-CSharp/DeckKeyboardFloating.cs",
                "SteamAPI.RunCallbacks();",
                ""
            );
        }

        private static string EmptyMethodBody(
            string source,
            string methodName)
        {
            string methodPattern =
                $@"protected\s+virtual\s+void\s+{Regex.Escape(methodName)}\s*\(\s*\)";

            Match match = Regex.Match(
                source,
                methodPattern
            );

            if (!match.Success)
            {
                Debug.LogWarning(
                    $"[WotH Wrapper] Could not find SteamManager.{methodName}()."
                );

                return source;
            }

            int openBraceIndex = source.IndexOf(
                '{',
                match.Index + match.Length
            );

            if (openBraceIndex < 0)
            {
                Debug.LogWarning(
                    $"[WotH Wrapper] Could not find opening brace of {methodName}()."
                );

                return source;
            }

            int depth = 0;
            int closeBraceIndex = -1;

            for (int i = openBraceIndex; i < source.Length; i++)
            {
                if (source[i] == '{')
                {
                    depth++;
                }
                else if (source[i] == '}')
                {
                    depth--;

                    if (depth == 0)
                    {
                        closeBraceIndex = i;
                        break;
                    }
                }
            }

            if (closeBraceIndex < 0)
            {
                Debug.LogWarning(
                    $"[WotH Wrapper] Could not find closing brace of {methodName}()."
                );

                return source;
            }

            string indentation = GetLineIndentation(
                source,
                match.Index
            );

            string replacement =
                source.Substring(0, openBraceIndex + 1)
                + "\n"
                + indentation
                + "}"
                + source.Substring(closeBraceIndex + 1);

            Debug.Log(
                $"[WotH Wrapper] Emptied SteamManager.{methodName}()."
            );

            return replacement;
        }

        private static string GetLineIndentation(
            string source,
            int index)
        {
            int lineStart = source.LastIndexOf(
                '\n',
                Math.Max(0, index - 1)
            );

            lineStart++;

            int length = 0;

            while (
                lineStart + length < source.Length &&
                (
                    source[lineStart + length] == ' ' ||
                    source[lineStart + length] == '\t'
                ))
            {
                length++;
            }

            return source.Substring(
                lineStart,
                length
            );
        }

        private static void PatchSteamDLCTools(string assetsPath)
        {
            string path = Path.Combine(
                assetsPath,
                "Scripts/Assembly-CSharp/SteamDLC/SteamDLCTools.cs"
            );

            if (!File.Exists(path))
            {
                Debug.LogWarning(
                    $"[WotH Wrapper] SteamDLCTools.cs not found: {path}"
                );

                return;
            }

            string source = File.ReadAllText(path);

            const string methodSignature =
                "public static void Initialize()";

            int signatureIndex = source.IndexOf(
                methodSignature,
                StringComparison.Ordinal
            );

            if (signatureIndex < 0)
            {
                Debug.LogWarning(
                    "[WotH Wrapper] SteamDLCTools.Initialize() signature not found."
                );

                return;
            }

            int openBraceIndex = source.IndexOf(
                '{',
                signatureIndex
            );

            if (openBraceIndex < 0)
            {
                Debug.LogWarning(
                    "[WotH Wrapper] Could not find opening brace of SteamDLCTools.Initialize()."
                );

                return;
            }

            int braceDepth = 0;
            int closeBraceIndex = -1;

            for (int i = openBraceIndex; i < source.Length; i++)
            {
                char c = source[i];

                if (c == '{')
                {
                    braceDepth++;
                }
                else if (c == '}')
                {
                    braceDepth--;

                    if (braceDepth == 0)
                    {
                        closeBraceIndex = i;
                        break;
                    }
                }
            }

            if (closeBraceIndex < 0)
            {
                Debug.LogWarning(
                    "[WotH Wrapper] Could not find closing brace of SteamDLCTools.Initialize()."
                );

                return;
            }

            string replacement =
        @"public static void Initialize()
                {
                    // Steam/DLC system disabled in the WotH wrapper.
                }";

            string patched =
                source.Substring(0, signatureIndex)
                + replacement
                + source.Substring(closeBraceIndex + 1);

            if (patched == source)
            {
                Debug.LogWarning(
                    "[WotH Wrapper] SteamDLCTools.Initialize() was not changed."
                );

                return;
            }

            File.WriteAllText(path, patched);

            Debug.Log(
                "[WotH Wrapper] Successfully disabled SteamDLCTools.Initialize()."
            );
        }

        private static void PatchExact(
            string assetsPath,
            string relativePath,
            string originalText,
            string replacementText)
        {
            string path = Path.Combine(assetsPath, relativePath);

            if (!File.Exists(path))
            {
                Debug.LogWarning(
                    $"[WotH Wrapper] Source file not found: {path}"
                );

                return;
            }

            string source = File.ReadAllText(path);

            if (!source.Contains(originalText))
            {
                Debug.LogWarning(
                    $"[WotH Wrapper] Exact text not found in {relativePath}: {originalText}"
                );

                return;
            }

            string patched = source.Replace(originalText, replacementText);

            if (patched != source)
            {
                File.WriteAllText(path, patched);

                Debug.Log(
                    $"[WotH Wrapper] Patched: {relativePath}"
                );
            }
        }

        private static string RemoveExactLine(
            string source,
            string line)
        {
            string[] lines = source.Split(
                new[] { "\r\n", "\n" },
                StringSplitOptions.None
            );

            bool changed = false;

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Trim() == line)
                {
                    lines[i] = "";
                    changed = true;
                }
            }

            if (!changed)
            {
                return source;
            }

            return string.Join(
                Environment.NewLine,
                lines
            );
        }

        private static void PatchSaveManagerEmission(string assetsPath)
        {
            string relativePath =
                "Scripts/Assembly-CSharp/SaveManager.cs";

            string path = Path.Combine(
                assetsPath,
                relativePath
            );

            if (!File.Exists(path))
            {
                Debug.LogWarning(
                    $"[WotH Wrapper] SaveManager.cs not found: {path}"
                );

                return;
            }

            string source = File.ReadAllText(path);

            const string marker =
                "List<Material> list = LoadLightMaterial(NewItem.Emission);";

            if (!source.Contains(marker))
            {
                Debug.LogWarning(
                    "[WotH Wrapper] Could not find LoadLightMaterial marker in SaveManager.cs."
                );

                return;
            }

            const string patch =
        @"if (NewItem.Emission == ""house"")
            NewItem.Emission = ""House"";

        if (NewItem.Emission == ""npc"")
            NewItem.Emission = ""NPC"";

        if (NewItem.Emission == ""car"")
            NewItem.Emission = ""Car"";

        ";

            if (source.Contains(patch))
            {
                Debug.Log(
                    "[WotH Wrapper] SaveManager Emission patch already applied."
                );

                return;
            }

            string patched =
                source.Replace(
                    marker,
                    patch + marker
                );

            if (patched == source)
            {
                Debug.LogWarning(
                    "[WotH Wrapper] SaveManager.cs was not changed."
                );

                return;
            }

            File.WriteAllText(
                path,
                patched
            );

            Debug.Log(
                "[WotH Wrapper] Successfully patched SaveManager Emission handling."
            );
        }

        private static void PatchGameManager(string assetsPath)
        {
            const string relativePath =
                "Scripts/Assembly-CSharp/GameManager.cs";

            string path = Path.Combine(assetsPath, relativePath);

            if (!File.Exists(path))
            {
                Debug.LogWarning(
                    $"[WotH Wrapper] GameManager.cs not found: {path}"
                );

                return;
            }

            string source = File.ReadAllText(path);

            const string patch =
                "        ManagerBase<StageSave>.Instance.HouseName = \"NpcQuest16x16\";\n" +
                "        ManagerBase<StageSave>.Instance.SaveName = \"NpcQuest16x16_Room\";";

            if (source.Contains(patch))
            {
                Debug.Log(
                    "[WotH Wrapper] GameManager.Start() patch already applied."
                );

                return;
            }

            Match startMatch = Regex.Match(
                source,
                @"private\s+void\s+Start\s*\(\s*\)\s*\{"
            );

            if (!startMatch.Success)
            {
                Debug.LogWarning(
                    "[WotH Wrapper] Could not find GameManager.Start()."
                );

                return;
            }

            int insertIndex = startMatch.Index + startMatch.Length;

            string patched =
                source.Substring(0, insertIndex) +
                Environment.NewLine +
                patch +
                source.Substring(insertIndex);

            File.WriteAllText(path, patched);

            Debug.Log(
                "[WotH Wrapper] Successfully patched GameManager.Start(): " +
                "HouseName=NpcQuest16x16, SaveName=NpcQuest16x16_Room."
            );
        }

        private static void PatchMoneyManager(string assetsPath)
        {
            const string relativePath =
                "Scripts/Assembly-CSharp/MoneyManager.cs";

            string path = Path.Combine(assetsPath, relativePath);

            if (!File.Exists(path))
            {
                Debug.LogWarning(
                    $"[WotH Wrapper] MoneyManager.cs not found: {path}"
                );

                return;
            }

            string source = File.ReadAllText(path);

            const string original =
                "\t\t\t//ManagerBase<UIManager>.Instance.OpenPanel<PopUpPanel>().EnqueuePopUp(new PopContent(\"<bounce>\" + LocalizationTools.GetTermTranslation(\"UI/代金券\") + \"</bounce>\", camPileUp: true, ChangeNum));";

            const string unCommented =
                "\t\t\tManagerBase<UIManager>.Instance.OpenPanel<PopUpPanel>().EnqueuePopUp(new PopContent(\"<bounce>\" + LocalizationTools.GetTermTranslation(\"UI/代金券\") + \"</bounce>\", camPileUp: true, ChangeNum));";

            if (source.Contains(original))
            {
                Debug.Log(
                    "[WotH Wrapper] MoneyManager PopUp call is already commented out."
                );

                return;
            }

            if (!source.Contains(unCommented))
            {
                Debug.LogWarning(
                    "[WotH Wrapper] Could not find MoneyManager PopUp call."
                );

                return;
            }

            string patched = source.Replace(
                unCommented,
                original
            );

            File.WriteAllText(path, patched);

            Debug.Log(
                "[WotH Wrapper] Successfully commented out MoneyManager PopUp call."
            );
        }

        private static void PatchUIManager(string assetsPath)
        {
            const string relativePath =
                "Scripts/Assembly-CSharp/UIManager.cs";

            string path = Path.Combine(assetsPath, relativePath);

            if (!File.Exists(path))
            {
                Debug.LogWarning(
                    $"[WotH Wrapper] UIManager.cs not found: {path}"
                );

                return;
            }

            string source = File.ReadAllText(path);

            const string original =
                "\t\t\t//OpenPanel<PopUpPanel>(null, \"Assets/WhisperoftheHouse/Game/Prafab/UI/Panels/PopUpPanel.prefab\", PanelPos.Front);";

            const string unCommented =
                "\t\t\tOpenPanel<PopUpPanel>(null, \"Assets/WhisperoftheHouse/Game/Prafab/UI/Panels/PopUpPanel.prefab\", PanelPos.Front);";

            if (source.Contains(original))
            {
                Debug.Log(
                    "[WotH Wrapper] UIManager PopUp call is already commented out."
                );

                return;
            }

            if (!source.Contains(unCommented))
            {
                Debug.LogWarning(
                    "[WotH Wrapper] Could not find UIManager PopUp call."
                );

                return;
            }

            string patched = source.Replace(
                unCommented,
                original
            );

            File.WriteAllText(path, patched);

            Debug.Log(
                "[WotH Wrapper] Successfully commented out UIManager PopUp call."
            );
        }

        public void OnComplete(bool failed)
        {
        }
    }
}