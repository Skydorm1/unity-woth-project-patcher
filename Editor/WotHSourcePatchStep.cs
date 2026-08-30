using System.IO;
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
            PatchExact(
                assetsPath,
                "Scripts/Assembly-CSharp/SteamManager.cs",
                @"protected virtual void Awake()
            {
                if (s_instance != null)
                {
                    UnityEngine.Object.Destroy(base.gameObject);
                    return;
                }
                s_instance = this;
                if (s_EverInitialized)
                {
                    throw new Exception(""Tried to Initialize the SteamAPI twice in one session!"");
                }
                UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
                if (!Packsize.Test())
                {
                    Debug.LogError(""[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform."", this);
                }
                if (!DllCheck.Test())
                {
                    Debug.LogError(""[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version."", this);
                }
                try
                {
                    if (SteamAPI.RestartAppIfNecessary((AppId_t)2589500u))
                    {
                        Debug.Log(""[Steamworks.NET] Shutting down because RestartAppIfNecessary returned true. Steam will restart the application."");
                        Application.Quit();
                        return;
                    }
                }
                catch (DllNotFoundException ex)
                {
                    Debug.LogError(""[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n"" + ex, this);
                    Application.Quit();
                    return;
                }
                m_bInitialized = SteamAPI.Init();
                if (!m_bInitialized)
                {
                    Debug.LogError(""[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information."", this);
                    Application.Quit();
                }
                else
                {
                    s_EverInitialized = true;
                }
            }",
                @"protected virtual void Awake()
            {
            }"
            );

            PatchExact(
                assetsPath,
                "Scripts/Assembly-CSharp/SteamManager.cs",
                @"protected virtual void Update()
            {
                if (m_bInitialized)
                {
                    SteamAPI.RunCallbacks();
                }
            }",
                @"protected virtual void Update()
            {
            }"
            );

            PatchExact(
                assetsPath,
                "Scripts/Assembly-CSharp/DeckKeyboardFloating.cs",
                "SteamAPI.RunCallbacks();",
                ""
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

        public void OnComplete(bool failed)
        {
        }
    }
}