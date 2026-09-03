using System.IO;
using System;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Nomnom.UnityProjectPatcher.Editor;
using Nomnom.UnityProjectPatcher.Editor.Steps;
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;

namespace Skydorm.WotHProjectPatcher.Editor
{
    public readonly struct WotHShaderPatchStep : IPatcherStep
    {
        public UniTask<StepResult> Run()
        {
            Debug.Log("[WotH Wrapper] WotHShaderPatchStep started.");
            var settings = this.GetSettings();
            string assetsPath = settings.ProjectGameAssetsPath;

            Debug.Log($"[WotH Wrapper] ProjectGameAssetsPath: {assetsPath} - should be Assets/WhisperoftheHouse/Game");
            
            SetMaterialShaders(assetsPath);
            AddMaterials(assetsPath);
            return UniTask.FromResult(StepResult.Success);
        }

        private static void SetMaterialShaders(string assetsPath)
        {
            SetMaterialShader(
                assetsPath,
                "Arts/Meterials/ChangeColor/ChangeColor.mat"
            );

            SetMaterialShader(
                assetsPath,
                "Materials/Sprite.mat"
            );
        }

        private static void AddMaterials(string assetsPath)
        {
            AddMaterial(
                assetsPath,
                "Arts/Meterials/windowcolor",
                "windowcolor"
            );

            AddMaterial(
                assetsPath,
                "Arts/Meterials/House",
                "House"
            );

            AddMaterial(
                assetsPath,
                "Arts/Meterials/house_color",
                "house_color"
            );

            AddMaterial(
                assetsPath,
                "Arts/Meterials/house_always",
                "house_always"
            );

            AddMaterial(
                assetsPath,
                "Arts/Meterials/Car",
                "Car"
            );

            AddMaterial(
                assetsPath,
                "Arts/Meterials/NPC",
                "NPC"
            );
        }

        private static void SetMaterialShader(
            string assetsPath,
            string relativeFilePath)
        {
            string materialPath =
                $"{assetsPath}/{relativeFilePath}".Replace('\\', '/');

            Material material =
                AssetDatabase.LoadAssetAtPath<Material>(materialPath);

            if (material == null)
            {
                Debug.LogError(
                    $"[WotH Wrapper] Could not load material: {materialPath}"
                );

                return;
            }

            Shader shader = Shader.Find("Sprites/Default");

            if (shader == null)
            {
                Debug.LogError(
                    "[WotH Wrapper] Could not find shader: Sprites/Default"
                );

                return;
            }

            material.shader = shader;

            EditorUtility.SetDirty(material);

            Debug.Log(
                $"[WotH Wrapper] Set shader to Sprites/Default: {materialPath}"
            );
        }

        private static void AddMaterial(
            string assetsPath,
            string relativePath,
            string fileName)
        {
            string materialDirectory =
                Path.Combine(
                    assetsPath,
                    relativePath
                ).Replace('\\', '/');

            string materialPath =
                $"{materialDirectory}/{fileName}.mat";

            if (File.Exists(materialPath))
            {
                Debug.Log(
                    $"[WotH Wrapper] Material already exists: {materialPath}"
                );

                return;
            }

            string projectRoot =
                Directory.GetParent(Application.dataPath).FullName;

            string absoluteDirectory =
                Path.Combine(
                    projectRoot,
                    materialDirectory.Replace(
                        '/',
                        Path.DirectorySeparatorChar
                    )
                );

            if (!Directory.Exists(absoluteDirectory))
            {
                Debug.Log(
                    $"[WotH Wrapper] Creating material directory: {materialDirectory}"
                );

                Directory.CreateDirectory(absoluteDirectory);
            }

            Shader shader = Shader.Find("Sprites/Default");

            if (shader == null)
            {
                Debug.LogError(
                    "[WotH Wrapper] Could not find shader: Sprites/Default"
                );

                return;
            }

            Material material = new Material(shader);

            AssetDatabase.CreateAsset(
                material,
                materialPath
            );

            Debug.Log(
                $"[WotH Wrapper] Created material: {materialPath}"
            );

            AssetDatabase.SaveAssets();

            AddToAddressables(materialPath);
        }

        private static void AddToAddressables(string assetPath)
        {
            AddressableAssetSettings settings =
                AddressableAssetSettingsDefaultObject.Settings;

            if (settings == null)
            {
                Debug.LogError(
                    "[WotH Wrapper] AddressableAssetSettings not found."
                );

                return;
            }

            const string groupName = "WotH Generated Materials";

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

            string guid = AssetDatabase.AssetPathToGUID(assetPath);

            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogError(
                    $"[WotH Wrapper] Could not get GUID for asset: {assetPath}"
                );

                return;
            }

            settings.CreateOrMoveEntry(
                guid,
                group,
                false,
                false
            );

            settings.SetDirty(
                AddressableAssetSettings.ModificationEvent.EntryMoved,
                null,
                true,
                true
            );

            Debug.Log(
                $"[WotH Wrapper] Added to Addressables group '{groupName}': {assetPath}"
            );
        }

        public void OnComplete(bool failed)
        {
        }
    }
}