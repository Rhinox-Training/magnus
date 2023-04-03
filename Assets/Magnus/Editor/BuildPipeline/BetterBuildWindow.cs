using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils;
using Rhinox.GUIUtils.Editor;
using Rhinox.GUIUtils.Editor.Helpers;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Editor;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using Rhinox.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
#if ODIN_INSPECTOR
using Rhinox.GUIUtils.Odin.Editor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
#endif

namespace Rhinox.Magnus.Editor
{
    public class BetterBuildWindow : PagerMenuEditorWindow<BetterBuildWindow>
    {
        /// ================================================================================================================
        /// PROPERTIES
        private IList<BuildConfig> _buildConfigs;

        private static GUIStyle _headerStyle;
        public static GUIStyle HeaderStyle => _headerStyle ?? (_headerStyle = new GUIStyle(CustomGUIStyles.ToggleGroupBackground)
        {
            fixedHeight = 26
        });

        private BetterBuildPageBase _main;
        protected override object RootPage => _main ?? (_main = new BetterBuildPageBase(_pager));
        protected override string RootPageName => "Overview";

        protected override bool IsMenuAvailable => _pager.IsOnFirstPage;
        
        private BetterBuildSettingsUI _settings;
        public BetterBuildSettingsUI Settings => _settings ?? (_settings = new BetterBuildSettingsUI(_pager));

        private Texture _loadIcon;
        private Texture _saveIcon;
        private GenericMenu _contextMenu;

        private const string WindowTitle = "Build Advanced...";
        
        /// ================================================================================================================
        /// METHODS
        protected override void Initialize()
        {
            base.Initialize();
        
            //TaskViewerSettings.LoadAll();
        
            _loadIcon = UnityIcon.AssetIcon("Fa_Folder");
            _saveIcon = UnityIcon.AssetIcon("save");
        }

        [MenuItem("File/" + WindowTitle, priority = 210)]
        public static void OpenWindow()
        {
            BetterBuildWindow window;
            if (!GetOrCreateWindow(out window)) return;
        
            window.name = nameof(BetterBuildWindow);
            window.titleContent = new GUIContent(WindowTitle, UnityIcon.AssetIcon("Fa_BoxOpen"));
        }
        
        protected override void DrawToolbarIcons(int toolbarHeight)
        {
            GUILayout.Label(EditorUserBuildSettings.activeBuildTarget.ToString(), GUILayout.Height(toolbarHeight));
            // if (SirenixEditorGUI.ToolbarButton("Import"))
            //     EditorApplication.delayCall += Import;
            
            if (CustomEditorGUI.IconButton(UnityIcon.AssetIcon("Fa_Cog"), toolbarHeight - 2, toolbarHeight - 2, "Settings"))
            {
                // TODO _pager.CurrentPage returns the PREV page if you just returned from it
                if (Equals(_pager.CurrentPage.Value, Settings))
                    _pager.NavigateBack();
                else
                    _pager.PushPage(Settings, "Settings");
            }
        }

        protected override CustomMenuTree BuildMenuTree()
        {
            if (this.MenuTree != null)
            {
                foreach (var item in this.MenuTree.MenuItems)
                    item.RightMouseClicked -= OnRightClickItem;
            }
            
            var tree = new CustomMenuTree();
#if ODIN_INSPECTOR
            tree.DrawSearchToolbar = true;
            tree.Config.SearchFunction = SimpleSearch;
#endif

            _buildConfigs = Utility.FindScriptableObjectsOfType<BuildConfig>();
            for (var i = 0; i < _buildConfigs.Count; i++)
            {
                var buildConfig = _buildConfigs[i];
                tree.Add(buildConfig.name, buildConfig);
                
                // foreach (var step in buildConfig.GetComponentsInChildren<BaseStep>())
                // {
                //     var item = new OdinMenuItem(tree, step.name, step);
                //     item.SearchString = GenerateSearchString(buildConfig, step);
                //     tree.AddMenuItemAtPath(buildConfig.name, item);
                // }
            }

            foreach (var item in tree.MenuItems)
                item.RightMouseClicked += OnRightClickItem;

            tree.SelectionChanged += OnSelectionChanged;
            return tree;
        }
        
        private void OnRightClickItem(IMenuItem obj)
        {
            _contextMenu = new GenericMenu();
            _contextMenu.AddItem(new GUIContent("Rename"), false, () => RenameEntry(obj.RawValue as BuildConfig));
            _contextMenu.AddItem(new GUIContent("Delete"), false, () => RemoveEntry(obj.RawValue as BuildConfig));
            _contextMenu.ShowAsContext();
        }

        private void RenameEntry(BuildConfig config)
        {
            if (config == null)
                return;
            var path = AssetDatabase.GetAssetPath(config);
            if (string.IsNullOrWhiteSpace(path))
                return;
            EditorInputDialog.Create("Rename", $"Provide a new name for config '{config.name}'")
                .TextField("Name: ", out var newName)
                .OnAccept(() =>
                {
                    if (string.IsNullOrWhiteSpace(newName))
                    {
                        PLog.Warn<MagnusLogger>($"Cannot rename BuildConfig to '{newName}', aborting...");
                        return;
                    }

                    var newNameStr = newName.Value.Trim();

                    var errorMessage = AssetDatabase.RenameAsset(path, newNameStr);
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        PLog.Error<MagnusLogger>($"errorMessage");
                        return;
                    }

                    var newMenuItem = this.MenuTree.GetMenuItem(newNameStr);
                    if (newMenuItem != null)
                    {
                        this.MenuTree.Selection.Clear();
                        this.MenuTree.Selection.Add(newMenuItem);
                    }
                })
                .ShowInPopup();
        }

        private void RemoveEntry(BuildConfig config)
        {
            if (config == null)
                return;

            var path = AssetDatabase.GetAssetPath(config);
            if (string.IsNullOrWhiteSpace(path))
                return;

            if (AssetDatabase.DeleteAsset(path))
            {
                AssetDatabase.Refresh();
                ForceMenuTreeRebuild();
            }
        }


        private void OnSelectionChanged(GUIUtils.Editor.SelectionChangedType type)
        {
            // Selection can not be more than 1
            var selection = MenuTree.Selection.FirstOrDefault();
            if (selection?.RawValue is BuildConfig)
            {
                if (selection is HierarchyMenuItem hierarchyMenuItem)
                    hierarchyMenuItem.SetExpanded(true);
            }
            _main.SetTarget(selection?.RawValue);
        }

        protected override void DrawMenu()
        {
            base.DrawMenu();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Create New Config..."))
            {
                EditorInputDialog.Create($"Create New {nameof(BuildConfig)}", "Please choose a name")
                    .TextField("Filename:", out var filename)
                    .OnAccept(() =>
                    {
                        string fileNameResult = filename;
                        if (_buildConfigs != null && _buildConfigs.Any(x => x.name.Equals(filename, StringComparison.InvariantCulture)))
                            fileNameResult += " (2)";

                        ScriptableObjectUtility.CreateAsset<BuildConfig>("Assets/Editor", fileNameResult);
                        ForceMenuTreeRebuild();
                    })
                    .ShowBlocking();
            }
            GUILayout.Space(12);
        }
    }
    
    public class BetterBuildPageBase : PagerTreePage
    {
        public BetterBuildPageBase(SlidePagedWindowNavigationHelper<object> pager) 
            : base(pager)
        {
        }
        
        protected override void OnDraw()
        {
            base.OnDraw();

            GUILayout.FlexibleSpace();
            
            TryGetTypedTarget(out BuildConfig config);
            
            bool isValidForPlatform = config == null || config.IsValidForCurrentEditorPlatformTarget();

            EditorGUI.BeginDisabledGroup(!isValidForPlatform);
            
#if ODIN_INSPECTOR
            if (!isValidForPlatform && config != null)
                SirenixEditorGUI.WarningMessageBox($"BuildConfig {config.name} targets {config.Platform}, which is not active for the current UnityEditor instance. (current: {EditorUserBuildSettings.activeBuildTarget})");
#endif

            if (config != null)
            {
                if (GUILayout.Button("Run Build", GUILayout.Height((int) ButtonSizes.Gigantic)))
                {
                    Debug.Log($"RunBuild {config}");
                    EditorApplication.delayCall += () => EditorBuildManager.TriggerBuild(new EditorBuildTask(config));
                }
            }
            EditorGUI.EndDisabledGroup();
        }
    }
    
    public class BetterBuildSettingsUI : PagerPage
    {
        private static readonly GUIContent _dateFormatLabel = new GUIContent("Date Format");
        private static readonly GUIContent _timeFormatLabel = new GUIContent("Time Format");

        private static readonly string[] _separators = new[]
        {
            // Note: these separators will appear in a FolderName so make sure it is possible
            "",
            "-",
            "_"
        };

        private const string SEP_FORMAT = "_";

        private static readonly string[] _dateFormatOptions = new[]
        {
            $"yyyy{SEP_FORMAT}MM{SEP_FORMAT}dd",
            $"yyyy{SEP_FORMAT}M{SEP_FORMAT}d",
            $"yy{SEP_FORMAT}MM{SEP_FORMAT}dd",
        };
        
        private static readonly string[] _timeFormatOptions = new[]
        {
            $"hh{SEP_FORMAT}mm{SEP_FORMAT}ss",
            $"HH{SEP_FORMAT}mm{SEP_FORMAT}ss",
            $"h{SEP_FORMAT}m{SEP_FORMAT}ss",
            $"H{SEP_FORMAT}mm{SEP_FORMAT}ss",
        };

        [OnValueChanged(nameof(UpdateSettings)), ValueDropdown(nameof(_dateFormatOptions))]
        [HorizontalGroup("DateFormat", .85f)]
        public string DateFormat;
        
        [OnValueChanged(nameof(UpdateSettings)), ValueDropdown(nameof(_separators))]
        [HorizontalGroup("DateFormat"), HideLabel]
        public string DateFormatSeparator;

        [OnValueChanged(nameof(UpdateSettings)), ValueDropdown(nameof(_timeFormatOptions))]
        [HorizontalGroup("TimeFormat", .85f)]
        public string TimeFormat;
        
        [OnValueChanged(nameof(UpdateSettings)), ValueDropdown(nameof(_separators))]
        [HorizontalGroup("TimeFormat"), HideLabel]
        public string TimeFormatSeparator;
        
        public BetterBuildSettingsUI(SlidePagedWindowNavigationHelper<object> pager) : base(pager)
        {
            
        }
        
        protected override void OnDraw()
        {
#if ODIN_INSPECTOR
            SirenixEditorGUI.InfoMessageBox("All settings below are saved on your computer and thus shared between all projects.", true);
#endif
            
            base.OnDraw();
        }

        private void UpdateSettings()
        {
            if (!string.IsNullOrEmpty(DateFormat))
                BetterBuildSettings.DateFormat.Set(DateFormat.Replace(SEP_FORMAT, DateFormatSeparator));
            if (!string.IsNullOrEmpty(TimeFormat))
                BetterBuildSettings.TimeFormat.Set(TimeFormat.Replace(SEP_FORMAT, TimeFormatSeparator));
        }
    }

    public static class BetterBuildSettings
    {
        private const string DefaultDateFormat = "yyyy-M-d";
        private const string DefaultTimeFormat = "hhmmss";

        private static PersistentValue<string> _dateFormat;
        public static PersistentValue<string> DateFormat => _dateFormat ?? (_dateFormat = PersistentValue<string>.Create(typeof(BetterBuildSettings), DefaultDateFormat) );


        private static PersistentValue<string> _timeFormat;
        public static PersistentValue<string> TimeFormat => _timeFormat ?? (_timeFormat = PersistentValue<string>.Create(typeof(BetterBuildSettings), DefaultTimeFormat) );
    }

}