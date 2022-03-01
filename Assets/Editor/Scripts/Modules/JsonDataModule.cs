using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XTools
{
    public class FieldType
    {
        public FieldTypes fieldType;
        public int size;
    }
    public class CheckInfos
    {

    }

    [Serializable]
    public class FieldBase
    {
        public string fieldName;

        public FieldType fieldTypeName;

        public object defaultValue;

        public bool forClient;

        public bool forServer;

        public int clientPosId;

        public int serverPosId;

        public int editorPosId;

        public TableFieldInfo.EIndexType indexType;

        public List<CheckInfos> checkInfos;

        public bool needLocal;

    }

    [Serializable]
    public class JsonDataModule : SerializedScriptableObject
    {
        [HideInInspector]
        public string path { get; set; }

        [ShowInInspector, LabelText("Main Table Name")]
        public string mainTableName
        {
            get
            {
                return JsonDataStash.Instance().mainTableName;
            }
        }

        [HideInInspector]
        public List<String> _tableLocations = new List<string>();

        [ShowInInspector, LabelText("Table Locations")]
        public List<String> tableLocations
        {
            get
            {
                if (JsonDataStash.Instance().tableLocations != null)
                {
                    _tableLocations.Clear();
                    foreach(var item in JsonDataStash.Instance().tableLocations)
                    {
                        _tableLocations.Add(item.ExcelPath);
                    }
                }
                return _tableLocations;
            }
            set
            {
                _tableLocations = value;
            }
        }

        [HideInInspector]
        public List<FieldBase> _fields = new List<FieldBase>();

        [ShowInInspector, LabelText("Fields")]
        public List<FieldBase> fields
        {
            get
            {
                if (JsonDataStash.Instance().fields != null)
                {
                    _fields.Clear();
                    foreach (var item in JsonDataStash.Instance().fields)
                    {
                        var field = new FieldBase();
                        field.fieldName = item.FieldName;
                        field.fieldTypeName.fieldType = Parser.ParseTypeFromString(item.FieldTypeName, out field.fieldTypeName.size);
                        field.defaultValue = item.DefaultValue;
                        field.forClient = item.ForClient;
                        field.forServer = item.ForServer;
                        field.clientPosId = item.ClientPosID;
                        field.serverPosId = item.ServerPosID;
                        field.editorPosId = item.EditorPosID;
                        field.indexType = item.IndexType;
                        field.needLocal = item.NeedLocal;
                        _fields.Add(field);
                    }
                }
                return _fields;
            }
            set
            {
                _fields = value;

            }
        }

        //[ShowInInspector, LabelText("Children")]
        //public List<JsonDataModule> Children;

        //[ShowInInspector, LabelText("Check Infos")]
        //public CheckInfos chekcInfos;
    }
}