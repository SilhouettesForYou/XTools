using Serializer;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XTools
{
    [Serializable]
    [GUIColor(110 / 255.0f, 191 / 255.0f, 139 / 255.0f)]
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
    public class CheckInfos
    {

    }

    [Serializable]
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
        [ShowInInspector, LabelText("For Client")]
        public bool forClient;

        [FoldoutGroup("Target", expanded: true)]
        [ShowInInspector, LabelText("For Server")]
        public bool forServer;

        [FoldoutGroup("Target", expanded: true)]
        [ShowInInspector, LabelText("Client Pos ID")]
        public int clientPosId;

        [FoldoutGroup("Target", expanded: true)]
        [ShowInInspector, LabelText("Server Pos ID")]
        public int serverPosId;

        [FoldoutGroup("Target", expanded: true)]
        [ShowInInspector, LabelText("Editor Pos ID")]
        public int editorPosId;

        [ShowInInspector, LabelText("Index Type")]
        public TableFieldInfo.EIndexType indexType;

        [ShowInInspector, LabelText("Check Infos")]
        public List<CheckInfos> checkInfos = new List<CheckInfos>();

        [ShowInInspector, LabelText("Need Local")]
        public bool needLocal;

    }

    [Serializable]
    public class JsonDataModule : SerializedScriptableObject
    {
        [HideInInspector]
        public string path { get; set; }

        [ShowInInspector, LabelText("Main Table Name")]
        public string mainTableName = "";


        [ShowInInspector, LabelText("Table Locations")]
        public List<String> tableLocations = new List<string>();

        [ShowInInspector, LabelText("Field")]
        [ListDrawerSettings(ListElementLabelName = "fieldName", DraggableItems = false, Expanded = false)]
        public List<FieldBase> fields = new List<FieldBase>();


        //[ShowInInspector, LabelText("Children")]
        //public List<JsonDataModule> Children;

        //[ShowInInspector, LabelText("Check Infos")]
        //public CheckInfos chekcInfos;
    }
}