using System.IO;
using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Nomnom.UnityProjectPatcher.Editor;
using Nomnom.UnityProjectPatcher.Editor.Steps;
using UnityEngine;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor;

namespace Skydorm.WotHProjectPatcher.Editor
{
    public readonly struct WotHAddressables : IPatcherStep
    {
        public UniTask<StepResult> Run()
        {
            Debug.Log("[WotH Wrapper] WotHAddressables started.");
            var settings = this.GetSettings();
            string assetsPath = settings.ProjectGameAssetsPath;

            Debug.Log($"[WotH Wrapper] ProjectGameAssetsPath: {assetsPath} - should be Assets/WhisperoftheHouse/Game");
            
            AddBundlesToAddressables();
            AddItemSOCToAddressables(assetsPath);
            AddHouseSaveToAddressables(assetsPath);
            AddLevelSaveToAddressables(assetsPath);
            return UniTask.FromResult(StepResult.Success);
        }

        private static void AddBundlesToAddressables()
        {
            Debug.Log(
                "[WotH Wrapper] Start Legacy AssetBundle → Addressables Converting..."
            );
            string[] bundles = AssetDatabase.GetAllAssetBundleNames();

            Debug.Log(
                $"[WotH Wrapper] Found Legacy AssetBundles: {bundles.Length}"
            );
            AddressableAssetSettings settings =
                AddressableAssetSettingsDefaultObject.Settings;

            if (settings == null)
            {
                settings = AddressableAssetSettings.Create(
                    AddressableAssetSettingsDefaultObject.kDefaultConfigFolder,
                    AddressableAssetSettingsDefaultObject.kDefaultConfigAssetName,
                    true,
                    true
                );

                AddressableAssetSettingsDefaultObject.Settings = settings;

                Debug.Log(
                    "[WotH Wrapper] Addressables Settings wurden erstellt."
                );
            }
            else
            {
                Debug.Log(
                    "[WotH Wrapper] Addressables Settings existieren bereits."
                );
            }

            Assembly addressablesAssembly =
                typeof(AddressableAssetSettings).Assembly;

            System.Type utilityType =
                addressablesAssembly.GetType(
                    "UnityEditor.AddressableAssets.Settings.AddressableAssetUtility"
                );

            if (utilityType == null)
            {
                Debug.LogError(
                    "[WotH Wrapper] AddressableAssetUtility konnte nicht gefunden werden."
                );

                return;
            }

            MethodInfo convertMethod =
                utilityType.GetMethod(
                    "ConvertAssetBundlesToAddressables",
                    BindingFlags.Static | BindingFlags.NonPublic
                );

            if (convertMethod == null)
            {
                Debug.LogError(
                    "[WotH Wrapper] ConvertAssetBundlesToAddressables() couldn't be found."
                );

                return;
            }

            Debug.Log(
                "[WotH Wrapper] Call ConvertAssetBundlesToAddressables() ..."
            );

            try
            {
                convertMethod.Invoke(null, null);

                Debug.Log(
                    "[WotH Wrapper] Legacy AssetBundles successfully converted."
                );
            }
            catch (TargetInvocationException exception)
            {
                Debug.LogException(
                    exception.InnerException ?? exception
                );
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "[WotH Wrapper] Legacy AssetBundle → Addressables finished."
            );
        }

        private static void AddItemSOCToAddressables(string assetsPath)
        {
            const string groupName = "ItemSOC";
            const string labelName = "ItemSOC";

            string folderPath =
                $"{assetsPath}/Scriptable/NewItems".Replace('\\', '/');

            AddressableAssetSettings settings =
                AddressableAssetSettingsDefaultObject.Settings;

            if (settings == null)
            {
                Debug.LogError(
                    "[WotH Wrapper] No Addressable Settings found."
                );
                return;
            }

            AddressableAssetGroup group =
                GetOrCreateAddressableGroup(settings, groupName);

            settings.AddLabel(labelName);

            string[] files =
                Directory.GetFiles(
                    folderPath,
                    "*.asset",
                    SearchOption.AllDirectories
                );

            int added = 0;

            foreach (string file in files)
            {
                string assetPath = file.Replace('\\', '/');

                string guid =
                    AssetDatabase.AssetPathToGUID(assetPath);

                if (string.IsNullOrEmpty(guid))
                    continue;

                AddressableAssetEntry entry =
                    settings.CreateOrMoveEntry(
                        guid,
                        group
                    );

                entry.SetLabel(
                    labelName,
                    true,
                    true
                );

                added++;
            }

            Debug.Log(
                $"[WotH Wrapper] ItemSOC Addressables finished. " +
                $"Registered: {added}"
            );
        }


        private static void AddHouseSaveToAddressables(string assetsPath)
        {
            const string groupName = "HouseSaveSO";
            const string labelName = "HouseSave";

            string folderPath =
                $"{assetsPath}/Scriptable/Houses".Replace('\\', '/');

            AddressableAssetSettings settings =
                AddressableAssetSettingsDefaultObject.Settings;

            if (settings == null)
            {
                Debug.LogError(
                    "[WotH Wrapper] No Addressable Settings found."
                );
                return;
            }

            AddressableAssetGroup group =
                GetOrCreateAddressableGroup(settings, groupName);

            settings.AddLabel(labelName);

            string[] files =
                Directory.GetFiles(
                    folderPath,
                    "*.asset",
                    SearchOption.AllDirectories
                );

            int added = 0;

            foreach (string file in files)
            {
                string assetPath = file.Replace('\\', '/');

                string guid =
                    AssetDatabase.AssetPathToGUID(assetPath);

                if (string.IsNullOrEmpty(guid))
                    continue;

                AddressableAssetEntry entry =
                    settings.CreateOrMoveEntry(
                        guid,
                        group
                    );

                entry.SetLabel(
                    labelName,
                    true
                );

                added++;
            }

            Debug.Log(
                $"[WotH Wrapper] HouseSaveSO Addressables finished. " +
                $"Registered: {added}"
            );
        }


        private static void AddLevelSaveToAddressables(string assetsPath)
        {
            const string groupName = "LevelSaveSO";
            const string labelName = "LevelSave";

            string folderPath =
                $"{assetsPath}/Scriptable/Levels".Replace('\\', '/');

            AddressableAssetSettings settings =
                AddressableAssetSettingsDefaultObject.Settings;

            if (settings == null)
            {
                Debug.LogError(
                    "[WotH Wrapper] No Addressable Settings found."
                );
                return;
            }

            AddressableAssetGroup group =
                GetOrCreateAddressableGroup(settings, groupName);

            settings.AddLabel(labelName);

            string[] files =
                Directory.GetFiles(
                    folderPath,
                    "*.asset",
                    SearchOption.AllDirectories
                );

            int added = 0;

            foreach (string file in files)
            {
                string assetPath = file.Replace('\\', '/');

                string guid =
                    AssetDatabase.AssetPathToGUID(assetPath);

                if (string.IsNullOrEmpty(guid))
                    continue;

                AddressableAssetEntry entry =
                    settings.CreateOrMoveEntry(
                        guid,
                        group
                    );

                entry.SetLabel(
                    labelName,
                    true
                );

                added++;
            }

            Debug.Log(
                $"[WotH Wrapper] LevelSaveSO Addressables finished. " +
                $"Registered: {added}"
            );
        }

        private static AddressableAssetGroup GetOrCreateAddressableGroup(
            AddressableAssetSettings settings,
            string groupName)
        {
            AddressableAssetGroup group =
                settings.FindGroup(groupName);

            if (group == null)
            {
                group = settings.CreateGroup(
                    groupName,
                    false,
                    false,
                    false,
                    null
                );

                Debug.Log(
                    $"[WotH Wrapper] Created Addressables group: {groupName}"
                );
            }
            else
            {
                Debug.Log(
                    $"[WotH Wrapper] Addressables group already exists: {groupName}"
                );
            }

            return group;
        }
        

        public void OnComplete(bool failed)
        {
        }
    }
}