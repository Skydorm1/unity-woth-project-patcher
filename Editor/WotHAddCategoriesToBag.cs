using System;
using System.IO;
using Cysharp.Threading.Tasks;
using Nomnom.UnityProjectPatcher.Editor;
using Nomnom.UnityProjectPatcher.Editor.Steps;
using UnityEngine;

namespace Skydorm.WotHProjectPatcher.Editor
{
    public readonly struct WotHAddCategoriesToBag : IPatcherStep
    {
        public UniTask<StepResult> Run()
        {
            Debug.Log("[WotH Wrapper] WotHAddCategoriesToBag started.");

            var settings = this.GetSettings();
            string assetsPath = settings.ProjectGameAssetsPath;

            Debug.Log(
                $"[WotH Wrapper] ProjectGameAssetsPath: {assetsPath} - expected Assets/WhisperoftheHouse/Game"
            );

            PatchTabButton(assetsPath);
            PatchBagCategoryBar(assetsPath);
            PatchUnLimitBagPanel(assetsPath);

            return UniTask.FromResult(StepResult.Success);
        }

        public void OnComplete(bool failed)
        {
        }

        private static void PatchTabButton(string assetsPath)
        {
            const string relativePath = "Scripts/Assembly-CSharp/Water/UI/TabButton.cs";
            string path = Path.Combine(assetsPath, relativePath);

            if (!File.Exists(path))
            {
                Debug.LogWarning($"[WotH Wrapper] TabButton.cs not found: {path}");
                return;
            }

            string source = File.ReadAllText(path);
            source = source.Replace("\r\n", "\n");

            const string methodSignature =
                "public void SetSprites(Sprite normal, Sprite highlight, Sprite selected, Sprite disable)";

            if (source.Contains(methodSignature))
            {
                Debug.Log("[WotH Wrapper] TabButton.SetSprites already exists.");
                return;
            }

            const string anchor =
                "\tprivate void OnEnable()";

            const string addition = @"
	public void SetSprites(Sprite normal, Sprite highlight, Sprite selected, Sprite disable)
	{
		buttonSprite.normalSprite = normal;
		buttonSprite.highlightSprite = highlight;
		buttonSprite.selectedSprite = selected;
		buttonSprite.disableSprite = disable;
	}

";

            if (!source.Contains(anchor))
            {
                Debug.LogWarning("[WotH Wrapper] Could not find TabButton OnEnable anchor.");
                return;
            }

            source = source.Replace(anchor, addition + anchor);

            File.WriteAllText(path, source);

            Debug.Log("[WotH Wrapper] Successfully patched TabButton.cs.");
        }

        private static void PatchBagCategoryBar(string assetsPath)
        {
            const string relativePath = "Scripts/Assembly-CSharp/BagCategoryBar.cs";
            string path = Path.Combine(assetsPath, relativePath);

            if (!File.Exists(path))
            {
                Debug.LogWarning($"[WotH Wrapper] BagCategoryBar.cs not found: {path}");
                return;
            }

            string source = File.ReadAllText(path);
            source = source.Replace("\r\n", "\n");

            const string methodSignature =
                "public void SetSelectThingType(GameObject value)";

            if (source.Contains(methodSignature))
            {
                Debug.Log("[WotH Wrapper] BagCategoryBar helper methods already exist.");
                return;
            }

            const string anchor =
                "\tpublic void Show(bool clear = true, int index = 0)";

            const string addition = @"
	public void SetSelectThingType(GameObject value)
	{
		selectThingType = value;
	}

	public GameObject GetSelectThingType()
	{
		return selectThingType;
	}

	public void SetSelectType(GameObject value)
	{
		selectType = value;
	}

	public GameObject GetSelectType()
	{
		return selectType;
	}

	public void emptyClassificationPages()
	{
		classificationPages.Clear();
	}

	public void addToClassificationPages(UITabView tab)
	{
		classificationPages.Add(tab);
	}

";

            if (!source.Contains(anchor))
            {
                Debug.LogWarning("[WotH Wrapper] Could not find BagCategoryBar Show anchor.");
                return;
            }

            source = source.Replace(anchor, addition + anchor);

            File.WriteAllText(path, source);

            Debug.Log("[WotH Wrapper] Successfully patched BagCategoryBar.cs.");
        }

        private static void PatchUnLimitBagPanel(string assetsPath)
        {
            const string relativePath = "Scripts/Assembly-CSharp/UnLimitBagPanel.cs";
            string path = Path.Combine(assetsPath, relativePath);

            if (!File.Exists(path))
            {
                Debug.LogWarning($"[WotH Wrapper] UnLimitBagPanel.cs not found: {path}");
                return;
            }

            string source = File.ReadAllText(path);
            source = source.Replace("\r\n", "\n");

            if (source.Contains("\tprivate BagCategoryBar bagCategoryBar_3;"))
            {
                Debug.Log("[WotH Wrapper] UnLimitBagPanel custom bag patch already exists.");
                return;
            }

            EnsureUsing(ref source, "using System.Collections;", "using System;");
            EnsureUsing(ref source, "using System.IO;", "using System.Collections.Generic;");
            EnsureUsing(ref source, "using System.Reflection;", "using System.IO;");
            EnsureUsing(ref source, "using System.Text.RegularExpressions;", "using System.Reflection;");
            EnsureUsing(ref source, "using Water.Bag;", "using Water.UI;");

            if (!source.Contains("#if UNITY_EDITOR\nusing UnityEditor;\n#endif"))
            {
                const string editorUsing = "#if UNITY_EDITOR\nusing UnityEditor;\n#endif\n";
                const string waterBagUsing = "using Water.Bag;\n";
                if (!source.Contains(waterBagUsing))
                {
                    Debug.LogWarning("[WotH Wrapper] Could not find Water.Bag using anchor.");
                    return;
                }

                source = source.Replace(waterBagUsing, waterBagUsing + editorUsing);
            }

            const string fieldAnchor =
                "\t[SerializeField]\n\tprivate BagCategoryBar bagCategoryBar_2;";

            if (!source.Contains(fieldAnchor))
            {
                Debug.LogWarning("[WotH Wrapper] Could not find UnLimitBagPanel field anchor.");
                return;
            }

            const string customFields = @"

	private BagCategoryBar bagCategoryBar_3;
	private BagCategoryBar bagCategoryBar_4;

	private GameObject customType;
	private GameObject customPackageType;
	private GameObject customSelectType;
	private GameObject customPackageSelectType;

	public static string[] selectCustomClassifications = new string[10]
	{
		""CustomSelectType/AllCustoms"",
		""CustomSelectType/Materials"",
		""CustomSelectType/Furnitures"",
		""CustomSelectType/Electrical"",
		""CustomSelectType/Plant"",
		""CustomSelectType/Clothing"",
		""CustomSelectType/Decoration"",
		""CustomSelectType/Hygiene"",
		""CustomSelectType/Food"",
		""CustomSelectType/Other""
	};

	public static string[] selectCustomPackageClassifications;

	private const string MyModsFolder = ""Assets/My Mods"";
	private const string CustomUiSpritesFolder = ""Assets/Custom Stuff/Custom UI Sprites"";

	private sealed class CustomPackageInfo
	{
		public string Name;
		public string PackageRoot;
		public string NormalIconPath;
		public string HoverIconPath;
		public string SelectedIconPath;
	}

	private sealed class ItemRename
	{
		public readonly string OriginalName;
		public readonly string NewName;

		public ItemRename(string originalName, string newName)
		{
			OriginalName = originalName;
			NewName = newName;
		}
	}

	private readonly List<CustomPackageInfo> customPackages = new List<CustomPackageInfo>();

	private static readonly ItemRename[] MaterialsItems =
	{
		new ItemRename(""窗户"", ""Window""),
		new ItemRename(""隔断"", ""Partition""),
		new ItemRename(""栏杆"", ""Railing""),
		new ItemRename(""墙壁"", ""Wall""),
		new ItemRename(""地块"", ""Tile""),
		new ItemRename(""楼梯"", ""Stairs""),
		new ItemRename(""门"", ""Door"")
	};

	private static readonly ItemRename[] FurnituresItems =
	{
		new ItemRename(""床"", ""Bed""),
		new ItemRename(""柜子"", ""Cabinet""),
		new ItemRename(""椅子"", ""Chair""),
		new ItemRename(""沙发"", ""Sofa""),
		new ItemRename(""架子"", ""StorageRack""),
		new ItemRename(""茶几"", ""CoffeeTable""),
		new ItemRename(""卫浴"", ""Bathroom""),
		new ItemRename(""厨房台面"", ""KitchenCountertop""),
		new ItemRename(""桌子"", ""Table""),
		new ItemRename(""镜子"", ""Mirror"")
	};

	private static readonly ItemRename[] ElectricalItems =
	{
		new ItemRename(""数码"", ""Digital""),
		new ItemRename(""家电"", ""Appliance""),
		new ItemRename(""灯具"", ""Lighting"")
	};

	private static readonly ItemRename[] PlantItems =
	{
		new ItemRename(""落地植物"", ""FloorPlant""),
		new ItemRename(""壁挂植物"", ""WallMountedPlant""),
		new ItemRename(""种植工具"", ""PlantingTool""),
		new ItemRename(""种子"", ""Seed"")
	};

	private static readonly ItemRename[] ClothingItems =
	{
		new ItemRename(""衣服"", ""Clothes""),
		new ItemRename(""鞋子"", ""Shoes""),
		new ItemRename(""饰品"", ""Accessory"")
	};

	private static readonly ItemRename[] DecorationItems =
	{
		new ItemRename(""纪念品"", ""Collectibles""),
		new ItemRename(""摆件"", ""Figurine""),
		new ItemRename(""地毯"", ""Carpet""),
		new ItemRename(""书"", ""Book""),
		new ItemRename(""工具"", ""Tool""),
		new ItemRename(""兴趣爱好"", ""Hobby""),
		new ItemRename(""墙上装饰"", ""WallDecor""),
		new ItemRename(""装饰"", ""Decor"")
	};

	private static readonly ItemRename[] HygieneItems =
	{
		new ItemRename(""清洁"", ""CleaningSupplies""),
		new ItemRename(""个人护理"", ""PersonalCare""),
		new ItemRename(""健康"", ""HealthCare"")
	};

	private static readonly ItemRename[] FoodItems =
	{
		new ItemRename(""餐具"", ""Tableware""),
		new ItemRename(""饮料"", ""Drinks""),
		new ItemRename(""食物"", ""Food""),
		new ItemRename(""厨具"", ""Coolware""),
		new ItemRename(""调料"", ""Seasoning"")
	};

	private static readonly ItemRename[] OtherItems =
	{
		new ItemRename(""庭院"", ""Courtyard""),
		new ItemRename(""垃圾桶"", ""TrashBin""),
		new ItemRename(""收纳"", ""Storage"")
	};

	private static readonly string[] RenamedItemLocalizationNames =
	{
		""Window"", ""Partition"", ""Railing"", ""Wall"", ""Tile"", ""Stairs"", ""Door"",
		""Bed"", ""Cabinet"", ""Chair"", ""Sofa"", ""StorageRack"", ""CoffeeTable"",
		""Bathroom"", ""KitchenCountertop"", ""Table"", ""Mirror"",
		""Digital"", ""Appliance"", ""Lighting"",
		""FloorPlant"", ""WallMountedPlant"", ""PlantingTool"", ""Seed"",
		""Clothes"", ""Shoes"", ""Accessory"",
		""Collectibles"", ""Figurine"", ""Carpet"", ""Book"", ""Tool"", ""Hobby"",
		""WallDecor"", ""Decor"",
		""CleaningSupplies"", ""PersonalCare"", ""HealthCare"",
		""Tableware"", ""Drinks"", ""Food"", ""Coolware"", ""Seasoning"",
		""Courtyard"", ""TrashBin"", ""Storage""
	};

	private static readonly string[] PackageTabNamesToRemove =
	{
		""收藏"", ""兔子"", ""冰川"", ""哥特"", ""日式"", ""春节"",
		""甜品"", ""赛博"", ""雪王"", ""齿轮"", ""夏日""
	};
";

            source = source.Replace(
                fieldAnchor,
                fieldAnchor + customFields
            );

            string awake =
@"	private void Awake()
	{
		try
		{
			BuildCustomPackageList();
			CreateCustomBagCategories();
		}
		catch (Exception ex)
		{
			Debug.LogError(""[UnLimitBagPanel] Failed to create custom bag categories: "" + ex);
		}
	}";

            const string startAnchor = "\tprivate void Start()";
            if (!source.Contains(startAnchor))
            {
                Debug.LogWarning("[WotH Wrapper] Could not find UnLimitBagPanel Start anchor.");
                return;
            }

            source = source.Replace(startAnchor, awake + "\n\n" + startAnchor);

            source = ReplaceMethod(source, "private void Start()", GetStartMethod());
            source = ReplaceMethod(source, "private void SelectTypeChange(int obj)", GetSelectTypeChangeMethod());
            source = ReplaceMethod(source, "private void ClickBar1()", GetClickBar1Method());
            source = ReplaceMethod(source, "private void ClickBar2()", GetClickBar2Method());
            source = ReplaceMethod(source, "private void ClickBar3()", GetClickBar3Method());

            const string expandAnchor = "\tpublic void Expand()";
            if (!source.Contains(expandAnchor))
            {
                Debug.LogWarning("[WotH Wrapper] Could not find UnLimitBagPanel Expand anchor.");
                return;
            }

            source = source.Replace(
                expandAnchor,
                GetCustomBagMethods() + "\n\n" + expandAnchor
            );

            File.WriteAllText(path, source);

            Debug.Log("[WotH Wrapper] Successfully patched UnLimitBagPanel.cs.");
        }

        private static void EnsureUsing(ref string source, string usingLine, string anchorUsing)
        {
            if (source.Contains(usingLine))
                return;

            string anchor = anchorUsing + "\n";
            if (!source.Contains(anchor))
                throw new InvalidOperationException(
                    "[WotH Wrapper] Could not find using anchor: " + anchorUsing
                );

            source = source.Replace(anchor, anchor + usingLine + "\n");
        }

        private static string ReplaceMethod(string source, string signature, string replacement)
        {
            int methodStart = source.IndexOf("\t" + signature, StringComparison.Ordinal);
            if (methodStart < 0)
                throw new InvalidOperationException(
                    "[WotH Wrapper] Could not find method: " + signature
                );

            int braceStart = source.IndexOf('{', methodStart);
            if (braceStart < 0)
                throw new InvalidOperationException(
                    "[WotH Wrapper] Could not find opening brace for method: " + signature
                );

            int depth = 0;
            for (int i = braceStart; i < source.Length; i++)
            {
                if (source[i] == '{')
                    depth++;
                else if (source[i] == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        int replacementStart = methodStart + 1;
                        return source.Substring(0, replacementStart) +
                               replacement +
                               source.Substring(i + 1);
                    }
                }
            }

            throw new InvalidOperationException(
                "[WotH Wrapper] Could not find closing brace for method: " + signature
            );
        }

        private static string GetStartMethod()
        {
            return @"private void Start()
	{
		EventGlobalManager.LevelLoadEndEventHandler += EventGlobalManager_LevelLoadEndEventHandler;
		btn_Search.onClick.AddListener(Search);
		inputField.onValueChanged.AddListener(CancelSearch);
		ManagerBase<EventGlobalManager>.Instance.LevelChangeEventHandel += Instance_LevelChangeEventHandel;
		tMP_Dropdown.options.Clear();
		List<string> list = new List<string>();
		string[] functionClassifications = BagManager.functionClassifications;
		foreach (string text in functionClassifications)
		{
			list.Add(LocalizationTools.GetTermTranslation(""UI/"" + text));
		}
		tMP_Dropdown.AddOptions(list);
		tMP_Dropdown.onValueChanged.AddListener(SetSelectedFunctionClassification);
		BagManager instance = ManagerBase<BagManager>.Instance;
		instance.TakeInModelChangedCallback = (Action<bool, bool>)Delegate.Combine(instance.TakeInModelChangedCallback, new Action<bool, bool>(ChangeTakeInButton));
		EventManager.StartListening(""UpdateBag"", UpdateContent);
		ManagerBase<EventGlobalManager>.Instance.onShowItemTagEventHandler += ShowItemInfo;
		ManagerBase<EventGlobalManager>.Instance.onHideItemTagEventHandler += HideItemInfo;
		recycleTip.OnInit(TrueRecycle);
		bagCategoryBar.OnInit(BagManager.selectedRoomClassifications, () =>
		{
			UpdateContent();
		});
		bagCategoryBar_1.OnInit(BagManager.selectBag2Classifications, () =>
		{
			UpdateContent();
		});
		bagCategoryBar_2.OnInit(BagManager.selectsuitClassifications, () =>
		{
			UpdateContent();
		});

		bagCategoryBar_3.OnInit(selectCustomClassifications, () =>
		{
			UpdateContent();
		});

		bagCategoryBar_4.OnInit(selectCustomPackageClassifications, () =>
		{
			UpdateContent();
		});
		selectType.BindEvent();
		UITabView uITabView = selectType;
		uITabView.CurrentIndexChanged = (Action<int>)Delegate.Combine(uITabView.CurrentIndexChanged, new Action<int>(SelectTypeChange));
		selectType.SwitchTab(0);
	}";
        }

        private static string GetSelectTypeChangeMethod()
        {
            return @"private void SelectTypeChange(int obj)
	{
		switch (obj)
		{
		case 0:
			ClickBar1();
			break;
		case 1:
			ClickBar2();
			break;
		case 2:
			ClickBar3();
			break;
		case 3:
			ClickBar4();
			break;
		default:
			ClickBar5();
			break;
		}

		ManagerBase<BagManager>.Instance.CurrentCategroy = obj;
		EventManager.EmitEvent(""UpdateBagContent"");
	}";
        }

        private static string GetClickBar1Method()
        {
            return @"private void ClickBar1()
	{
		bagCategoryBar.Show();
		bagCategoryBar_1.Hide();
		bagCategoryBar_2.Hide();
		bagCategoryBar_3.Hide();
		bagCategoryBar_4.Hide();
		Collapse();
	}";
        }

        private static string GetClickBar2Method()
        {
            return @"private void ClickBar2()
	{
		bagCategoryBar.Hide();
		bagCategoryBar_1.Show();
		bagCategoryBar_2.Hide();
		bagCategoryBar_3.Hide();
		bagCategoryBar_4.Hide();
		Collapse();
	}";
        }

        private static string GetClickBar3Method()
        {
            return @"private void ClickBar3()
	{
		bagCategoryBar.Hide();
		bagCategoryBar_1.Hide();
		bagCategoryBar_2.Show();
		bagCategoryBar_3.Hide();
		bagCategoryBar_4.Hide();
		Collapse();
	}";
        }

        private static string GetCustomBagMethods()
        {
            return @"	private void ClickBar4()
	{
		bagCategoryBar.Hide();
		bagCategoryBar_1.Hide();
		bagCategoryBar_2.Hide();
		bagCategoryBar_3.Show();
		bagCategoryBar_4.Hide();
		Collapse();
	}

	private void ClickBar5()
	{
		bagCategoryBar.Hide();
		bagCategoryBar_1.Hide();
		bagCategoryBar_2.Hide();
		bagCategoryBar_3.Hide();
		bagCategoryBar_4.Show();
		Collapse();
	}



	private void CreateCustomBagCategories()
	{
		if (bagCategoryBar == null || bagCategoryBar_1 == null || bagCategoryBar_2 == null)
			throw new InvalidOperationException(""Original BagCategoryBar references are missing."");

		GameObject bagCat3Object = UnityEngine.Object.Instantiate(
			bagCategoryBar_1.gameObject,
			bagCategoryBar_1.transform.parent
		);
		bagCat3Object.name = ""BagCategoryBar (3)"";
		bagCategoryBar_3 = bagCat3Object.GetComponent<BagCategoryBar>();

		GameObject bagCat4Object = UnityEngine.Object.Instantiate(
			bagCategoryBar_2.gameObject,
			bagCategoryBar_2.transform.parent
		);
		bagCat4Object.name = ""BagCategoryBar (4)"";
		bagCategoryBar_4 = bagCat4Object.GetComponent<BagCategoryBar>();

		if (bagCategoryBar_3 == null || bagCategoryBar_4 == null)
			throw new InvalidOperationException(""Could not create custom BagCategoryBar components."");

		SetupSingleItemCategory();
		SetupPackageCategory();

		bagCategoryBar_3.Hide();
		bagCategoryBar_4.Hide();
	}

	private void SetupSingleItemCategory()
	{
		GameObject originalSelectType = bagCategoryBar_1.GetSelectType();
		GameObject originalSelectThingType = bagCategoryBar_1.GetSelectThingType();

		if (originalSelectType == null || originalSelectThingType == null)
			throw new InvalidOperationException(""BagCategoryBar (3) source references are missing."");

		customType = UnityEngine.Object.Instantiate(
			originalSelectType,
			originalSelectType.transform.parent
		);
		customType.name = ""CustomType"";

		customSelectType = UnityEngine.Object.Instantiate(
			originalSelectThingType,
			originalSelectThingType.transform.parent
		);
		customSelectType.name = ""CustomSelectType"";

		bagCategoryBar_3.SetSelectType(customType);
		bagCategoryBar_3.SetSelectThingType(customSelectType);

		GameObject contentObject = FindChildByName(customType.transform, ""Content"");
		if (contentObject == null)
			throw new InvalidOperationException(""CustomType/Content could not be found."");

		UITabView contentTabView = contentObject.GetComponent<UITabView>();
		if (contentTabView == null)
			throw new InvalidOperationException(""CustomType/Content has no UITabView."");

		bagCategoryBar_3.uITabView = contentTabView;

		if (contentTabView.Tabs.Count > 2)
			contentTabView.Tabs.RemoveAt(2);
		if (contentTabView.Tabs.Count > 1)
			contentTabView.Tabs.RemoveAt(1);

		bagCategoryBar_3.emptyClassificationPages();

		RenameRequired(customType, ""全部"", ""AllCustoms"");
		DestroyIfFound(customType.transform, ""收藏"");
		DestroyIfFound(customType.transform, ""方块"");
		RenameRequired(customType, ""建材"", ""Materials"");
		RenameRequired(customType, ""家具"", ""Furnitures"");
		RenameRequired(customType, ""电器"", ""Electrical"");
		RenameRequired(customType, ""植物"", ""Plant"");
		RenameRequired(customType, ""服装"", ""Clothing"");
		RenameRequired(customType, ""装饰"", ""Decoration"");
		RenameRequired(customType, ""食品"", ""Food"");
		RenameRequired(customType, ""卫生"", ""Hygiene"");
		RenameRequired(customType, ""其他"", ""Other"");

		CreateCustomModeButton();

		AddClassificationPageFromChild(customSelectType, ""全部"", ""AllCustoms"", null);
		DestroyIfFound(customSelectType.transform, ""收藏"");
		DestroyIfFound(customSelectType.transform, ""方块"");

		AddClassificationPageFromChild(customSelectType, ""建材"", ""Materials"", MaterialsItems);
		AddClassificationPageFromChild(customSelectType, ""家具"", ""Furnitures"", FurnituresItems);
		AddClassificationPageFromChild(customSelectType, ""电器"", ""Electrical"", ElectricalItems);
		AddClassificationPageFromChild(customSelectType, ""植物"", ""Plant"", PlantItems);
		AddClassificationPageFromChild(customSelectType, ""服装"", ""Clothing"", ClothingItems);
		AddClassificationPageFromChild(customSelectType, ""装饰"", ""Decoration"", DecorationItems);
		AddClassificationPageFromChild(customSelectType, ""食物"", ""Food"", FoodItems);
		AddClassificationPageFromChild(customSelectType, ""卫生"", ""Hygiene"", HygieneItems);
		AddClassificationPageFromChild(customSelectType, ""其他"", ""Other"", OtherItems);

		EnsureI2Terms(RenamedItemLocalizationNames);
	}

	private void SetupPackageCategory()
	{
		if (selectCustomPackageClassifications == null)
			BuildCustomPackageList();

		GameObject originalSelectType = bagCategoryBar_2.GetSelectType();
		GameObject originalSelectThingType = bagCategoryBar_2.GetSelectThingType();

		if (originalSelectType == null || originalSelectThingType == null)
			throw new InvalidOperationException(""BagCategoryBar (4) source references are missing."");

		customPackageType = UnityEngine.Object.Instantiate(
			originalSelectType,
			originalSelectType.transform.parent
		);
		customPackageType.name = ""CustomPackageType"";

		customPackageSelectType = UnityEngine.Object.Instantiate(
			originalSelectThingType,
			originalSelectThingType.transform.parent
		);
		customPackageSelectType.name = ""CustomPackageSelectType"";

		bagCategoryBar_4.SetSelectType(customPackageType);
		bagCategoryBar_4.SetSelectThingType(customPackageSelectType);

		GameObject contentObject = FindChildByName(customPackageType.transform, ""Content"");
		if (contentObject == null)
			throw new InvalidOperationException(""CustomPackageType/Content could not be found."");

		UITabView contentTabView = contentObject.GetComponent<UITabView>();
		if (contentTabView == null)
			throw new InvalidOperationException(""CustomPackageType/Content has no UITabView."");

		bagCategoryBar_4.uITabView = contentTabView;

		for (int index = 12; index >= 3; index--)
		{
			if (index < contentTabView.Tabs.Count)
				contentTabView.Tabs.RemoveAt(index);
		}
		if (contentTabView.Tabs.Count > 1)
			contentTabView.Tabs.RemoveAt(1);

		bagCategoryBar_4.emptyClassificationPages();

		RenameRequired(customPackageType, ""全部"", ""AllCustomsPackage"");

		for (int i = 0; i < PackageTabNamesToRemove.Length; i++)
			DestroyIfFound(customPackageType.transform, PackageTabNamesToRemove[i]);

		CreateCustomPackageModeButton();

		GameObject packageTabTemplate = RequireChild(customPackageType, ""中式"");
		int templateSiblingIndex = packageTabTemplate.transform.GetSiblingIndex();
		int packageIndex = 0;

		for (int i = 0; i < customPackages.Count; i++)
		{
			CustomPackageInfo package = customPackages[i];

			GameObject newTab = UnityEngine.Object.Instantiate(
				packageTabTemplate,
				packageTabTemplate.transform.parent
			);

			newTab.name = package.Name;
			newTab.transform.SetSiblingIndex(templateSiblingIndex + packageIndex + 1);

			AddTab(contentTabView, newTab);
			ConfigurePackageTabSprites(newTab, package);
			EnsureI2Term(""UI/"" + package.Name, package.Name);
			ReplacePackageTabLocalizationText(newTab, ""UI/"" + package.Name);

			packageIndex++;
		}

		RemoveTab(contentTabView, packageTabTemplate);
		UnityEngine.Object.Destroy(packageTabTemplate);

		AddClassificationPageFromChild(
			customPackageSelectType,
			""全部"",
			""AllCustomsPackage"",
			null
		);

		GameObject packagePageTemplate = RequireChild(customPackageSelectType, ""中式"");
		int pageSiblingIndex = packagePageTemplate.transform.GetSiblingIndex();
		int pageIndex = 0;

		for (int i = 0; i < customPackages.Count; i++)
		{
			CustomPackageInfo package = customPackages[i];

			GameObject newPage = UnityEngine.Object.Instantiate(
				packagePageTemplate,
				packagePageTemplate.transform.parent
			);

			newPage.name = package.Name;
			newPage.transform.SetSiblingIndex(pageSiblingIndex + pageIndex + 1);

			UITabView newPageTabView = newPage.GetComponent<UITabView>();
			if (newPageTabView == null)
				throw new InvalidOperationException(
					""Package classification page '"" + package.Name + ""' has no UITabView.""
				);

			bagCategoryBar_4.addToClassificationPages(newPageTabView);
			pageIndex++;
		}

		for (int i = 0; i < PackageTabNamesToRemove.Length; i++)
			DestroyIfFound(customPackageSelectType.transform, PackageTabNamesToRemove[i]);

		UnityEngine.Object.Destroy(packagePageTemplate);
	}

	private void BuildCustomPackageList()
	{
		customPackages.Clear();

		List<string> names = new List<string>();
		HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

#if UNITY_EDITOR
		AssetDatabase.Refresh();

		string[] guids = AssetDatabase.FindAssets(""t:TextAsset"", new[] { MyModsFolder });
		List<string> packageNameFiles = new List<string>();

		for (int i = 0; i < guids.Length; i++)
		{
			string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
			if (string.IsNullOrEmpty(assetPath))
				continue;

			string fileName = Path.GetFileName(assetPath);
			if (string.Equals(fileName, ""packagename.json"", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(fileName, ""packagenames.json"", StringComparison.OrdinalIgnoreCase))
			{
				packageNameFiles.Add(assetPath.Replace('\\', '/'));
			}
		}

		packageNameFiles.Sort(StringComparer.OrdinalIgnoreCase);

		Debug.Log(""[UnLimitBagPanel] Found "" + packageNameFiles.Count +
			"" package name file(s) under "" + MyModsFolder + ""."");

		if (packageNameFiles.Count == 0)
		{
			Debug.LogWarning(
				""[UnLimitBagPanel] No packagename.json was found under "" + MyModsFolder +
				"". Unity must have imported the JSON as a TextAsset.""
			);
		}

		for (int i = 0; i < packageNameFiles.Count; i++)
		{
			string assetPath = packageNameFiles[i];
			TextAsset jsonAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);

			if (jsonAsset == null)
			{
				Debug.LogWarning(
					""[UnLimitBagPanel] Could not load package name file as TextAsset: "" + assetPath
				);
				continue;
			}

			List<string> filePackageNames = ExtractPackageNames(jsonAsset.text);
			if (filePackageNames.Count == 0)
			{
				Debug.LogWarning(
					""[UnLimitBagPanel] No packageName could be extracted from "" + assetPath +
					"". Expected JSON containing \""packageName\"". ""+
					""Content: "" + jsonAsset.text
				);
				continue;
			}

			string packageRoot = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
			if (string.IsNullOrEmpty(packageRoot))
				continue;

			string iconsDirectory = (packageRoot + ""/Package Icons"").Replace('\\', '/');

			for (int nameIndex = 0; nameIndex < filePackageNames.Count; nameIndex++)
			{
				string packageName = filePackageNames[nameIndex].Trim();
				if (string.IsNullOrWhiteSpace(packageName) || !seen.Add(packageName))
					continue;

				string normalIconPath = iconsDirectory + ""/packageIcon.png"";
				string hoverIconPath = iconsDirectory + ""/packageIcon_Hover.png"";
				string selectedIconPath = iconsDirectory + ""/packageIcon_Selected.png"";

				names.Add(packageName);

				customPackages.Add(new CustomPackageInfo
				{
					Name = packageName,
					PackageRoot = packageRoot,
					NormalIconPath = normalIconPath,
					HoverIconPath = hoverIconPath,
					SelectedIconPath = selectedIconPath
				});

				Debug.Log(
					""[UnLimitBagPanel] Registered custom package '"" + packageName +
					""' from '"" + assetPath + ""'.""
				);
				Debug.Log(
					""[UnLimitBagPanel] Package icons: "" + normalIconPath +
					"" | "" + hoverIconPath + "" | "" + selectedIconPath
				);
			}
		}
#else
		Debug.LogWarning(
			""[UnLimitBagPanel] Custom package discovery is only available inside the Unity Editor.""
		);
#endif

		List<string> classifications = new List<string>();
		classifications.Add(""CustomPackageSelectType/AllCustomsPackage"");

		for (int i = 0; i < names.Count; i++)
			classifications.Add(""CustomPackageSelectType/"" + names[i]);

		selectCustomPackageClassifications = classifications.ToArray();

		Debug.Log(
			""[UnLimitBagPanel] Registered "" + customPackages.Count +
			"" custom package(s).""
		);
	}

	private List<string> ExtractPackageNames(string json)
	{
		List<string> result = new List<string>();
		HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		if (string.IsNullOrWhiteSpace(json))
			return result;

		MatchCollection singleMatches = Regex.Matches(
			json,
			@""""""packageName""""\s*:\s*""""([^""""]+)"""""",
			RegexOptions.IgnoreCase
		);

		for (int i = 0; i < singleMatches.Count; i++)
		{
			string value = singleMatches[i].Groups[1].Value.Trim();
			if (!string.IsNullOrWhiteSpace(value) && seen.Add(value))
				result.Add(value);
		}

		if (result.Count > 0)
			return result;

		string[] listKeys = { ""packageNames"", ""packages"", ""packageList"", ""names"" };
		for (int keyIndex = 0; keyIndex < listKeys.Length; keyIndex++)
		{
			string key = listKeys[keyIndex];
			Match listMatch = Regex.Match(
				json,
				@"""""""" + Regex.Escape(key) + @""""""\s*:\s*\[(.*?)\]"",
				RegexOptions.IgnoreCase | RegexOptions.Singleline
			);

			if (!listMatch.Success)
				continue;

			MatchCollection valueMatches = Regex.Matches(listMatch.Groups[1].Value, @""""""([^""""]+)"""""", RegexOptions.Singleline);
			for (int i = 0; i < valueMatches.Count; i++)
			{
				string value = valueMatches[i].Groups[1].Value.Trim();
				if (!string.IsNullOrWhiteSpace(value) && seen.Add(value))
					result.Add(value);
			}
		}

		if (result.Count > 0)
			return result;

		string trimmed = json.Trim();
		if (trimmed.StartsWith(""["") && trimmed.EndsWith(""]""))
		{
			MatchCollection valueMatches = Regex.Matches(trimmed, @""""""([^""""]+)"""""", RegexOptions.Singleline);
			for (int i = 0; i < valueMatches.Count; i++)
			{
				string value = valueMatches[i].Groups[1].Value.Trim();
				if (!string.IsNullOrWhiteSpace(value) && seen.Add(value))
					result.Add(value);
			}
		}

		return result;
	}

	private void CreateCustomModeButton()
	{
		GameObject bagBG = GetTabGameObject(selectType, 1);
		if (bagBG == null)
			throw new InvalidOperationException(""selectType.Tabs[1] is missing."");

		GameObject newButton = UnityEngine.Object.Instantiate(
			bagBG,
			bagBG.transform.parent
		);

		newButton.name = ""CustomMode"";
		newButton.transform.SetSiblingIndex(bagBG.transform.GetSiblingIndex() + 2);
		newButton.transform.position = new Vector3(
			bagBG.transform.position.x,
			bagBG.transform.position.y - 88f * 2f,
			bagBG.transform.position.z
		);

		AddTab(selectType, newButton);

		Sprite normal = LoadProjectSprite(""CustomMode.asset"");
		Sprite hover = LoadProjectSprite(""CustomMode_Hover.asset"");
		Sprite selected = LoadProjectSprite(""SetCustomMode.asset"");

		ConfigureButtonSprites(
			newButton,
			normal,
			hover,
			selected,
			normal
		);

		EnsureI2Term(""UI/CustomMode"", ""Custom Items"");
		ReplaceButtonLocalizationText(newButton, ""UI/CustomMode"");
	}

	private void CreateCustomPackageModeButton()
	{
		GameObject bagBG = GetTabGameObject(selectType, 1);
		if (bagBG == null)
			throw new InvalidOperationException(""selectType.Tabs[1] is missing."");

		GameObject newButton = UnityEngine.Object.Instantiate(
			bagBG,
			bagBG.transform.parent
		);

		newButton.name = ""CustomPackageMode"";
		newButton.transform.SetSiblingIndex(bagBG.transform.GetSiblingIndex() + 3);
		newButton.transform.position = new Vector3(
			bagBG.transform.position.x,
			bagBG.transform.position.y - 88f * 3f,
			bagBG.transform.position.z
		);

		AddTab(selectType, newButton);

		Sprite normal = LoadProjectSprite(""CustomPackageMode.asset"");
		Sprite hover = LoadProjectSprite(""CustomPackageMode_Hover.asset"");
		Sprite selected = LoadProjectSprite(""SetCustomPackageMode.asset"");

		ConfigureButtonSprites(
			newButton,
			normal,
			hover,
			selected,
			normal
		);

		EnsureI2Term(""UI/CustomPackageMode"", ""Custom Packages"");
		ReplaceButtonLocalizationText(newButton, ""UI/CustomPackageMode"");
	}

	private Sprite LoadProjectSprite(string fileName)
	{
#if UNITY_EDITOR
		string assetPath = CustomUiSpritesFolder + ""/"" + fileName;
		assetPath = assetPath.Replace('\\', '/');

		Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
		if (sprite == null)
		{
			Debug.LogError(
				""[UnLimitBagPanel] Could not load custom UI Sprite from Assets path: "" + assetPath
			);
		}

		return sprite;
#else
		return null;
#endif
	}

	private Sprite LoadPackageSprite(string assetPath)
	{
#if UNITY_EDITOR
		if (string.IsNullOrWhiteSpace(assetPath))
			return null;

		assetPath = assetPath.Replace('\\', '/');
		Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);

		if (sprite == null)
		{
			UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
			for (int i = 0; i < assets.Length; i++)
			{
				Sprite candidate = assets[i] as Sprite;
				if (candidate != null)
				{
					sprite = candidate;
					break;
				}
			}
		}

		if (sprite == null)
		{
			Debug.LogWarning(
				""[UnLimitBagPanel] Could not load package icon Sprite from Assets path: "" + assetPath +
				"". Check that the PNG Texture Type is Sprite (2D and UI).""
			);
		}

		return sprite;
#else
		return null;
#endif
	}

	private void ConfigurePackageTabSprites(
		GameObject buttonObject,
		CustomPackageInfo package)
	{
		Sprite normal = LoadPackageSprite(package.NormalIconPath);
		Sprite hover = LoadPackageSprite(package.HoverIconPath);
		Sprite selected = LoadPackageSprite(package.SelectedIconPath);

		if (normal == null || hover == null || selected == null)
		{
			Debug.LogError(
				""[UnLimitBagPanel] Package '"" + package.Name +
				""' is missing one or more Package Icons sprites.""
			);
			return;
		}

		Image image = buttonObject.GetComponent<Image>();
		if (image != null)
			image.sprite = normal;

		Button button = buttonObject.GetComponent<Button>();
		if (button != null)
		{
			SpriteState state = button.spriteState;
			state.highlightedSprite = hover;
			state.pressedSprite = normal;
			state.selectedSprite = hover;
			state.disabledSprite = normal;
			button.spriteState = state;
		}

		Water.UI.TabButton tabButton = buttonObject.GetComponent<Water.UI.TabButton>();
		if (tabButton != null)
			tabButton.SetSprites(normal, hover, selected, normal);
	}

	private void ConfigureButtonSprites(
		GameObject buttonObject,
		Sprite normal,
		Sprite hover,
		Sprite selected,
		Sprite disabled)
	{
		if (normal == null || hover == null || selected == null || disabled == null)
		{
			Debug.LogError(""[UnLimitBagPanel] One or more custom UI sprites are missing."");
			return;
		}

		Image image = buttonObject.GetComponent<Image>();
		if (image != null)
			image.sprite = normal;

		Button button = buttonObject.GetComponent<Button>();
		if (button != null)
		{
			SpriteState state = button.spriteState;
			state.highlightedSprite = hover;
			state.pressedSprite = normal;
			state.selectedSprite = hover;
			state.disabledSprite = disabled;
			button.spriteState = state;
		}

		Water.UI.TabButton tabButton = buttonObject.GetComponent<Water.UI.TabButton>();
		if (tabButton != null)
			tabButton.SetSprites(normal, hover, selected, disabled);
	}

	private void AddClassificationPageFromChild(
		GameObject root,
		string originalName,
		string newName,
		ItemRename[] itemRenames)
	{
		GameObject pageObject = RequireChild(root, originalName);
		UITabView tabView = pageObject.GetComponent<UITabView>();

		if (tabView == null)
			throw new InvalidOperationException(
				""Child '"" + originalName + ""' has no UITabView.""
			);

		pageObject.name = newName;

		BagCategoryBar targetBar =
			root == customSelectType ? bagCategoryBar_3 : bagCategoryBar_4;

		targetBar.addToClassificationPages(tabView);

		if (itemRenames != null)
			RenameItems(pageObject.transform, itemRenames);
	}

	private void RenameItems(Transform root, ItemRename[] renames)
	{
		if (renames == null)
			return;

		for (int i = 0; i < renames.Length; i++)
		{
			ItemRename rename = renames[i];
			GameObject item = FindChildByName(root, rename.OriginalName);

			if (item == null)
			{
				Debug.LogWarning(
					""[UnLimitBagPanel] Could not find item child '"" +
					rename.OriginalName + ""' below '"" + root.name + ""'.""
				);
				continue;
			}

			item.name = rename.NewName;

			TagSlot tagSlot = item.GetComponent<TagSlot>();
			if (tagSlot != null)
				tagSlot.ItemTag = rename.NewName;
		}
	}

	private void AddTab(UITabView uiTabView, GameObject buttonObject)
	{
		Water.UI.TabButton tabButton = buttonObject.GetComponent<Water.UI.TabButton>();

		if (tabButton == null)
			throw new InvalidOperationException(
				""New button '"" + buttonObject.name + ""' has no Water.UI.TabButton.""
			);

		if (!uiTabView.Tabs.Contains(tabButton))
			uiTabView.Tabs.Add(tabButton);
	}

	private void RemoveTab(UITabView uiTabView, GameObject buttonObject)
	{
		if (buttonObject == null)
			return;

		for (int i = uiTabView.Tabs.Count - 1; i >= 0; i--)
		{
			if (uiTabView.Tabs[i] != null &&
				uiTabView.Tabs[i].gameObject == buttonObject)
			{
				uiTabView.Tabs.RemoveAt(i);
				return;
			}
		}
	}

	private GameObject GetTabGameObject(UITabView uiTabView, int index)
	{
		if (uiTabView == null || index < 0 || index >= uiTabView.Tabs.Count)
			return null;

		Water.UI.TabButton tabButton = uiTabView.Tabs[index];
		return tabButton != null ? tabButton.gameObject : null;
	}

	private GameObject RequireChild(GameObject root, string name)
	{
		GameObject child = FindChildByName(root.transform, name);

		if (child == null)
			throw new InvalidOperationException(
				""Could not find child '"" + name + ""' below "" + root.name
			);

		return child;
	}

	private void RenameRequired(GameObject root, string oldName, string newName)
	{
		RequireChild(root, oldName).name = newName;
	}

	private void DestroyIfFound(Transform root, string name)
	{
		GameObject obj = FindChildByName(root, name);

		if (obj != null)
			UnityEngine.Object.Destroy(obj);
	}

	private GameObject FindChildByName(Transform parent, string name)
	{
		if (parent == null)
			return null;

		foreach (Transform child in parent)
		{
			if (string.Equals(child.name, name, StringComparison.Ordinal))
				return child.gameObject;

			GameObject nested = FindChildByName(child, name);
			if (nested != null)
				return nested;
		}

		return null;
	}


	private static object i2Source;
	private static Type i2LanguageSourceDataType;
	private static Type i2TermDataType;

	private void EnsureI2Term(string term, string translation)
	{
		EnsureI2Terms(
			new string[] { term.Replace(""UI/"", """") },
			new string[] { translation }
		);
	}

	private void EnsureI2Terms(IEnumerable<string> names)
	{
		EnsureI2Terms(names, null);
	}

	private void EnsureI2Terms(IEnumerable<string> names, IEnumerable<string> translations)
	{
		if (names == null)
			return;

		try
		{
			object source = FindI2LanguageSource();
			if (source == null)
				return;

			IList languages = GetMember(source, ""mLanguages"") as IList;
			if (languages == null)
				languages = GetMember(source, ""Languages"") as IList;

			if (languages == null || languages.Count == 0)
				return;

			List<string> nameList = new List<string>(names);
			List<string> translationList =
				translations != null
					? new List<string>(translations)
					: new List<string>();

			for (int i = 0; i < nameList.Count; i++)
			{
				string cleanName = nameList[i];
				if (string.IsNullOrWhiteSpace(cleanName))
					continue;

				string term = cleanName.StartsWith(""UI/"", StringComparison.Ordinal)
					? cleanName
					: ""UI/"" + cleanName;

				string translation =
					i < translationList.Count
						? translationList[i]
						: cleanName;

				SetI2TermAllLanguages(
					source,
					term,
					translation ?? string.Empty,
					languages.Count
				);
			}

			MethodInfo updateDictionary = FindMethod(
				source.GetType(),
				""UpdateDictionary"",
				1
			);

			if (updateDictionary != null)
				updateDictionary.Invoke(source, new object[] { true });
		}
		catch (Exception ex)
		{
			Debug.LogWarning(""[UnLimitBagPanel] I2 localization setup failed: "" + ex);
		}
	}

	private void SetI2TermAllLanguages(
		object source,
		string term,
		string translation,
		int languageCount)
	{
		object termData = InvokeOptional(source, ""GetTermData"", term);

		if (termData == null)
		{
			object textTermType = GetI2TextTermType();

			termData = InvokeOptional(
				source,
				""AddTerm"",
				term,
				textTermType
			);
		}

		if (termData == null)
			return;

		object textType = GetI2TextTermType();
		if (textType != null)
			SetMember(termData, ""TermType"", textType);

		SetMember(termData, ""Description"", string.Empty);

		MethodInfo validate = FindMethod(termData.GetType(), ""Validate"", 0);
		if (validate != null)
			validate.Invoke(termData, null);

		ResizeArrayMember(termData, ""Languages"", languageCount);
		ResizeArrayMember(termData, ""Flags"", languageCount);

		MethodInfo setTranslation = FindMethod(
			termData.GetType(),
			""SetTranslation"",
			2
		);

		if (setTranslation == null)
			return;

		ParameterInfo[] parameters = setTranslation.GetParameters();

		for (int languageIndex = 0; languageIndex < languageCount; languageIndex++)
		{
			object[] args = new object[parameters.Length];
			args[0] = languageIndex;
			args[1] = translation;

			for (int i = 2; i < parameters.Length; i++)
				args[i] = parameters[i].DefaultValue;

			setTranslation.Invoke(termData, args);
		}
	}

	private object FindI2LanguageSource()
	{
		if (i2Source != null)
			return i2Source;

		i2LanguageSourceDataType =
			i2LanguageSourceDataType ??
			Type.GetType(""I2.Loc.LanguageSourceData"");

		if (i2LanguageSourceDataType == null)
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				i2LanguageSourceDataType =
					assembly.GetType(""I2.Loc.LanguageSourceData"", false);

				if (i2LanguageSourceDataType != null)
					break;
			}
		}

		if (i2LanguageSourceDataType == null)
			return null;

		Type localizationManagerType =
			Type.GetType(""I2.Loc.LocalizationManager"");

		if (localizationManagerType == null)
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				localizationManagerType =
					assembly.GetType(""I2.Loc.LocalizationManager"", false);

				if (localizationManagerType != null)
					break;
			}
		}

		if (localizationManagerType == null)
			return null;

		PropertyInfo sourcesProperty =
			localizationManagerType.GetProperty(
				""Sources"",
				BindingFlags.Public | BindingFlags.Static
			);

		FieldInfo sourcesField =
			localizationManagerType.GetField(
				""Sources"",
				BindingFlags.Public | BindingFlags.Static
			);

		object sources =
			sourcesProperty != null
				? sourcesProperty.GetValue(null, null)
				: sourcesField != null
					? sourcesField.GetValue(null)
					: null;

		IList sourceList = sources as IList;
		if (sourceList == null)
			return null;

		for (int i = 0; i < sourceList.Count; i++)
		{
			object candidate = sourceList[i];

			if (candidate != null &&
				i2LanguageSourceDataType.IsInstanceOfType(candidate))
			{
				i2Source = candidate;
				return i2Source;
			}
		}

		return null;
	}

	private object GetI2TextTermType()
	{
		i2TermDataType =
			i2TermDataType ??
			Type.GetType(""I2.Loc.TermData"");

		if (i2TermDataType == null)
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				i2TermDataType =
					assembly.GetType(""I2.Loc.TermData"", false);

				if (i2TermDataType != null)
					break;
			}
		}

		if (i2TermDataType == null)
			return null;

		FieldInfo termTypeField =
			i2TermDataType.GetField(
				""TermType"",
				BindingFlags.Instance |
				BindingFlags.Public |
				BindingFlags.NonPublic
			);

		if (termTypeField == null ||
			!termTypeField.FieldType.IsEnum)
			return null;

		try
		{
			return Enum.Parse(termTypeField.FieldType, ""Text"");
		}
		catch
		{
			return Activator.CreateInstance(termTypeField.FieldType);
		}
	}

	private object InvokeOptional(object target, string methodName, params object[] supplied)
	{
		MethodInfo method = FindMethod(
			target.GetType(),
			methodName,
			supplied != null ? supplied.Length : 0
		);

		if (method == null)
			return null;

		ParameterInfo[] parameters = method.GetParameters();
		object[] args = new object[parameters.Length];

		for (int i = 0; i < parameters.Length; i++)
		{
			if (supplied != null && i < supplied.Length)
			{
				args[i] = supplied[i];
			}
			else
			{
				object value = parameters[i].DefaultValue;

				if (value == Missing.Value)
				{
					value = parameters[i].ParameterType.IsValueType
						? Activator.CreateInstance(parameters[i].ParameterType)
						: null;
				}

				args[i] = value;
			}
		}

		return method.Invoke(target, args);
	}

	private static MethodInfo FindMethod(Type type, string name, int parameterCount)
	{
		for (Type current = type; current != null; current = current.BaseType)
		{
			MethodInfo[] methods = current.GetMethods(
				BindingFlags.Instance |
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.NonPublic
			);

			for (int i = 0; i < methods.Length; i++)
			{
				if (methods[i].Name == name &&
					methods[i].GetParameters().Length >= parameterCount)
					return methods[i];
			}
		}

		return null;
	}

	private static object GetMember(object instance, string name)
	{
		if (instance == null)
			return null;

		for (Type current = instance.GetType(); current != null; current = current.BaseType)
		{
			FieldInfo field = current.GetField(
				name,
				BindingFlags.Instance |
				BindingFlags.Public |
				BindingFlags.NonPublic
			);

			if (field != null)
				return field.GetValue(instance);

			PropertyInfo property = current.GetProperty(
				name,
				BindingFlags.Instance |
				BindingFlags.Public |
				BindingFlags.NonPublic
			);

			if (property != null && property.GetMethod != null)
				return property.GetValue(instance, null);
		}

		return null;
	}

	private static bool SetMember(object instance, string name, object value)
	{
		if (instance == null)
			return false;

		for (Type current = instance.GetType(); current != null; current = current.BaseType)
		{
			FieldInfo field = current.GetField(
				name,
				BindingFlags.Instance |
				BindingFlags.Public |
				BindingFlags.NonPublic
			);

			if (field != null)
			{
				field.SetValue(instance, value);
				return true;
			}

			PropertyInfo property = current.GetProperty(
				name,
				BindingFlags.Instance |
				BindingFlags.Public |
				BindingFlags.NonPublic
			);

			if (property != null && property.SetMethod != null)
			{
				property.SetValue(instance, value, null);
				return true;
			}
		}

		return false;
	}

	private static void ResizeArrayMember(
		object instance,
		string memberName,
		int length)
	{
		object value = GetMember(instance, memberName);
		Array array = value as Array;

		if (array != null && array.Length >= length)
			return;

		FieldInfo field = null;

		for (Type current = instance.GetType(); current != null; current = current.BaseType)
		{
			field = current.GetField(
				memberName,
				BindingFlags.Instance |
				BindingFlags.Public |
				BindingFlags.NonPublic
			);

			if (field != null)
				break;
		}

		if (field == null || !field.FieldType.IsArray)
			return;

		Array resized =
			Array.CreateInstance(field.FieldType.GetElementType(), length);

		if (array != null)
			Array.Copy(array, resized, Math.Min(array.Length, length));

		field.SetValue(instance, resized);
	}

	private void ReplaceButtonLocalizationText(GameObject buttonObject, string term)
	{
		if (buttonObject == null || buttonObject.transform.childCount == 0)
			return;

		Transform firstChild = buttonObject.transform.GetChild(0);

		Type localizeType = FindType(""I2.Loc.Localize"");
		if (localizeType == null)
			return;

		Component localize = firstChild.GetComponent(localizeType);
		if (localize == null)
			return;

		if (!SetMember(localize, ""mTerm"", term))
			return;

		GameObject newText = UnityEngine.Object.Instantiate(
			localize.gameObject,
			localize.transform.parent
		);

		if (newText == null)
			return;

		newText.name = localize.gameObject.name;
		UnityEngine.Object.Destroy(localize.gameObject);
	}

	private void ReplacePackageTabLocalizationText(
		GameObject tabObject,
		string term)
	{
		if (tabObject == null || tabObject.transform.childCount == 0)
			return;

		Transform child0 = tabObject.transform.GetChild(0);

		if (child0.childCount == 0)
			return;

		Transform textObject = child0.GetChild(0);

		Type localizeType = FindType(""I2.Loc.Localize"");
		if (localizeType == null)
			return;

		Component localize = textObject.GetComponent(localizeType);
		if (localize == null)
			return;

		if (!SetMember(localize, ""mTerm"", term))
			return;

		GameObject newText = UnityEngine.Object.Instantiate(
			localize.gameObject,
			localize.transform.parent
		);

		if (newText == null)
			return;

		newText.name = localize.gameObject.name;
		UnityEngine.Object.Destroy(localize.gameObject);
	}

	private static Type FindType(string fullName)
	{
		Type type = Type.GetType(fullName);

		if (type != null)
			return type;

		foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			type = assembly.GetType(fullName, false);

			if (type != null)
				return type;
		}

		return null;
	}";
        }
    }
}
