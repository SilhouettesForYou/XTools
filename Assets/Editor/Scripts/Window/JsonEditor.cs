using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace XTools
{
    public class JsonEditorWindow : OdinMenuEditorWindow
    {
        protected int selectIndex = 0;
        protected Rect buttonRectTools = new Rect(0, -5, GlobalConfig.MENU_TOOLS_NAME.Length * 20, 24);
        public CommonMenuWindow menu;
        public OdinMenuTree menuTree { get; set; }
        public static JsonEditorWindow MainWindow;

        [MenuItem("XTools/Json Editor")]
        public static void Open()
        {
            MainWindow = GetWindow<JsonEditorWindow>();
            MainWindow.position = GUIHelper.GetEditorWindowRect().AlignCenter(1280, 800);
            MainWindow.minSize = new Vector2(1280, 800);
            MainWindow.titleContent = new GUIContent("Main Window");
            MainWindow.Show();
        }

        [MenuItem("XTools/Export Json")]
        public static void ExportJsonToAsset()
        {
            var asset = AssetDatabase.LoadAssetAtPath<JsonDataModuleList>(GlobalConfig.JSON_ASSET_NAME);
            if (asset != null)
            {
                asset.jsonList.Clear();
            }
            else
            {
                asset = ScriptableObject.CreateInstance<JsonDataModuleList>();
                AssetDatabase.CreateAsset(asset, GlobalConfig.JSON_ASSET_NAME);
            }
            
            if (!Directory.Exists(GlobalConfig.JSON_DIR))
            {
                return;
            }

            var files = Directory.GetFiles(GlobalConfig.JSON_DIR, $"*.json", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var name = Utils.GetFileNameFromPath(file, Path.AltDirectorySeparatorChar);
                var obj = CreateInstance<JsonDataModule>();
                obj.path = file;
                var raw = DeserializeJson.Deserialize<TableInfo>(obj.path) as TableInfo;
                JsonDataProcesser.Instance().Stash(obj, raw.MainTableName, raw.TableLocations, raw.Fields);
                asset.jsonList.Add(obj);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("<color=#97DBAE>Export Successfully</color>");
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            if (menuTree != null)
            {
                return menuTree;
            }
            menuTree = new OdinMenuTree();
            menuTree.Config.DrawSearchToolbar = true;
            menuTree.Selection.SelectionChanged -= OnClickMenuItem;
            menuTree.Selection.SelectionChanged += OnClickMenuItem;
            LoadMenuTree();
            return menuTree;
        }

        protected override void OnGUI()
        {
            EditorStyles.popup.fontSize = 12;
            EditorStyles.popup.fixedHeight = 18;
            EditorStyles.popup.alignment = TextAnchor.MiddleCenter;

            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                if (EditorGUILayout.DropdownButton(new GUIContent(GlobalConfig.MENU_TOOLS_NAME), FocusType.Passive, selectIndex == 1 ? CustomEditorStyle.dropdownButtonSelected : CustomEditorStyle.dropdownButtonNormal, GUILayout.Width(GlobalConfig.MENU_TOOLS_NAME.Length * 20), GUILayout.Height(24)))
                {
                    CreateMenuTools();
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();

            base.OnGUI();
        }

        private void LoadMenuTree()
        {
            if (!Directory.Exists(GlobalConfig.JSON_DIR))
            {
                return;
            }

            var files = Directory.GetFiles(GlobalConfig.JSON_DIR, $"*.json", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var name = Utils.GetFileNameFromPath(file, Path.AltDirectorySeparatorChar);
                var obj = CreateInstance<JsonDataModule>();
                obj.path = file;
                var menuItem = new OdinMenuItem(menuTree, name.Substring(0, name.Length - 5), obj);
                menuTree.RootMenuItem.ChildMenuItems.Add(menuItem);
            }
        }

        private void OnClickMenuItem(SelectionChangedType type)
        {
            if (type == SelectionChangedType.ItemAdded)
            {
                OdinMenuItem curSelectItem = menuTree.Selection[menuTree.Selection.Count - 1];
                if (curSelectItem != null && curSelectItem.Value != null)
                {
                    var item = curSelectItem.Value as JsonDataModule;
                    if (item.path != null)
                    {
                        var raw = DeserializeJson.Deserialize<TableInfo>(item.path) as TableInfo;
                        JsonDataProcesser.Instance().Stash(item, raw.MainTableName, raw.TableLocations, raw.Fields);
                    }
                }
                if (curSelectItem != null && curSelectItem.Value == null)
                {
                    curSelectItem.Value = AssetDatabase.LoadAssetAtPath(curSelectItem.AssetPath, typeof(UnityEngine.Object));
                }
            }
        }

        protected void CreateMenuTools()
        {
            menu = ScriptableObject.CreateInstance<CommonMenuWindow>();
            menu.AddItem(GlobalConfig.MENU_TOOLS_EXPORT_LUA_ALL_SERVER_WITH_INDEX, 0, MenuFunctions.ExportLuaAllForServerWithIndex);
            menu.AddItem(GlobalConfig.MENU_TOOLS_EXPORT_LUA_ALL_CLIENT_WITH_INDEX, 0, MenuFunctions.ExportLuaAllForClientWithIndex);
            menu.AddItem(GlobalConfig.MENU_TOOLS_EXPORT_LUA_ALL_SERVER_WITH_KEY, 0, MenuFunctions.ExportLuaAllForServerWithKey);
            menu.AddItem(GlobalConfig.MENU_TOOLS_EXPORT_LUA_ALL_CLIENT_WITH_KEY, 0, MenuFunctions.ExportLuaAllForClientWithKey);
            menu.DropDown(buttonRectTools, position, delegate () {
                selectIndex = 0;
                menu = null;
            });
            selectIndex = 1;
        }

        protected void OnMouseChangePosition()
        {
            Vector2 mouserPosition = Event.current.mousePosition;
            if (selectIndex > 0)
            {
                if (buttonRectTools.Contains(mouserPosition) && selectIndex != 1)
                {
                    menu.CloseAndChild();
                    menu.Close();
                    CreateMenuTools();
                }
                this.Repaint();
            }
        }
    }
}
