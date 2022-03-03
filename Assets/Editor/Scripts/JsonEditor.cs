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
        public OdinMenuTree menuTree { get; set; }
        public static JsonEditorWindow MainWindow;

        [MenuItem("XTools/Json Editor")]
        public static void Open()
        {
            MainWindow = GetWindow<JsonEditorWindow>();
            MainWindow.position = GUIHelper.GetEditorWindowRect().AlignCenter(1280, 800);
            MainWindow.minSize = new Vector2(1280, 800);
            MainWindow.titleContent = new GUIContent("Ö÷´°¿Ú");
            MainWindow.Show();
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
                        JsonDataStash.Instance().Stash(item, raw.MainTableName, raw.TableLocations, raw.Fields);
                    }
                }
                if (curSelectItem != null && curSelectItem.Value == null)
                {
                    curSelectItem.Value = AssetDatabase.LoadAssetAtPath(curSelectItem.AssetPath, typeof(UnityEngine.Object));
                }
            }
        }
    }
}
