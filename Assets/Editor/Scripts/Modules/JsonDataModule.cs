using Serializer;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XTools
{
    [Serializable]
    [GUIColor(154 / 255.0f, 220 / 255.0f, 255 / 255.0f)]
    public enum ExportTarget
    {
        Server = 0,
        Client = 1
    }

    [Serializable]
    [GUIColor(154 / 255.0f, 220 / 255.0f, 255 / 255.0f)]
    public enum KeyOrIndex
    {
        Key = 0,
        Index = 1
    }

    [Serializable]
    [GUIColor(181 / 255.0f, 254 / 255.0f, 131 / 255.0f)]
    public class FieldType
    {
        [OnValueChanged("OnFieldTypeChanged")]
        [ShowInInspector, LabelText("Field Type")]
        public FieldTypes fieldType;
        [ShowInInspector, LabelText("Size")]
        public int size;

        private void OnFieldTypeChanged()
        {

        }

        public override string ToString()
        {
            int type = (int)fieldType;
            if (type < 0 || type % 100 >= TypeDefine.GetBasicTypeStr().Count)
            {
                return "";
            }

            string baseType = TypeDefine.GetBasicTypeStr()[type % 100];
            if (type / 100 == 0)
            {
                return baseType;
            }
            else if (type / 100 == 1)
            {
                return $"vector<{baseType}>";
            }
            else if (type / 100 == 2)
            {
                return $"Sequence<{baseType}, {size}>";
            }
            else if (type / 100 == 3)
            {
                return $"vector<Sequence<{baseType}, {size}>>";
            }
            else if (type / 100 == 4)
            {
                return $"vector<vector<{baseType}>>";
            }
            return "";
        }
    }

    [Serializable]
    [GUIColor(255 / 255.0f, 183 / 255.0f, 43 / 255.0f)]
    public class FieldBase
    {
        [ShowInInspector, LabelText("Field Name")]
        public string fieldName;

        [FoldoutGroup("Field Type Name", expanded: true)]
        //[ShowInInspector, LabelText("Field Type Name")]
        [HideLabel]
        public FieldType fieldTypeName = new FieldType();

        [ShowInInspector, LabelText("Default Value")]
        public string defaultValue;

        [FoldoutGroup("Target", expanded: true)]
        [GUIColor(247 / 255.0f, 247 / 255.0f, 247 / 255.0f)]
        [ShowInInspector, LabelText("For Client")]
        public bool forClient;

        [FoldoutGroup("Target", expanded: true)]
        [GUIColor(247 / 255.0f, 247 / 255.0f, 247 / 255.0f)]
        [ShowInInspector, LabelText("For Server")]
        public bool forServer;

        [FoldoutGroup("Target", expanded: true)]
        [GUIColor(247 / 255.0f, 247 / 255.0f, 247 / 255.0f)]
        [ShowInInspector, LabelText("Client Pos ID")]
        public int clientPosId;

        [FoldoutGroup("Target", expanded: true)]
        [GUIColor(247 / 255.0f, 247 / 255.0f, 247 / 255.0f)]
        [ShowInInspector, LabelText("Server Pos ID")]
        public int serverPosId;

        [FoldoutGroup("Target", expanded: true)]
        [GUIColor(247 / 255.0f, 247 / 255.0f, 247 / 255.0f)]
        [ShowInInspector, LabelText("Editor Pos ID")]
        public int editorPosId;

        [ShowInInspector, LabelText("Index Type")]
        public TableFieldInfo.EIndexType indexType;

        [ShowInInspector, LabelText("Need Local")]
        public bool needLocal;

    }

    [Serializable]
    public class JsonDataModule : SerializedScriptableObject
    {
        [HideInInspector]
        public string path { get; set; }

        [Button("Export To Lua", ButtonSizes.Medium, ButtonStyle.FoldoutButton, Expanded = false), GUIColor(255 / 255.0f, 138 / 255.0f, 174 / 255.0f)]
        private void ExportToLua(ExportTarget target, KeyOrIndex key)
        {
            foreach (var name in tableLocations)
            {
                EditorUtility.DisplayProgressBar("Export To Lua", name, 0.5f);
                Writer writer = new LuaWriter(name.Replace(".csv", ""));
                writer.SetConfig(new LuaWriterConfig(target, key));
                writer.Write(this);
                EditorUtility.ClearProgressBar();
            }
        }

        [ShowInInspector, LabelText("Main Table Name")]
        public string mainTableName = "";


        [ShowInInspector, LabelText("Table Locations")]
        public List<String> tableLocations = new List<string>();

        [ShowInInspector, LabelText("Field")]
        [DictionaryDrawerSettings(KeyLabel = "Field Name", ValueLabel = "Field", DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public Dictionary<string, FieldBase> fields = new Dictionary<string, FieldBase>();
    }
}