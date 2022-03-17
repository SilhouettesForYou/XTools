using Newtonsoft.Json;
using Serializer;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace XTools
{
    public class DeserializeJson : SingleBase<DeserializeJson>
    {
        public static object Deserialize<T>(string path)
        {
            if (path == null) return null;
            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }

    public class JsonDataProcesser : SingleBase<JsonDataProcesser>
    {
        private Dictionary<string, Parser> _parserCache = new Dictionary<string, Parser>();
        public Dictionary<string, Parser> ParserCache
        {
            get
            {
                return _parserCache;
            }
        }
        public string mainTableName;
        public TableLocation[] tableLocations;
        public TableFieldInfo[] fields;

        public void Stash(JsonDataModule module, string name, TableLocation[] locations, TableFieldInfo[] _fields)
        {
            mainTableName = name;
            tableLocations = locations;
            fields = _fields;

            module.mainTableName = mainTableName;

            module.tableLocations.Clear();
            foreach (var item in Instance().tableLocations)
            {
                module.tableLocations.Add(item.ExcelPath);
            }

            module.fields.Clear();
            foreach (var item in Instance().fields)
            {
                var field = new FieldBase();
                field.fieldName = item.FieldName;
                field.fieldTypeName.fieldType = ParserUtil.GetFieldType(item.FieldTypeName);
                field.fieldTypeName.size = ParserUtil.GetSequenceLength(item.FieldTypeName);

                var parser = ParserUtil.GetParser(field.fieldTypeName.fieldType, (sbyte)field.fieldTypeName.size);
                if (!_parserCache.ContainsKey(item.FieldTypeName))
                {
                    _parserCache.Add(item.FieldTypeName, parser);
                }
                field.defaultValue = item.DefaultValue;
                field.forClient = item.ForClient;
                field.forServer = item.ForServer;
                field.clientPosId = item.ClientPosID;
                field.serverPosId = item.ServerPosID;
                field.editorPosId = item.EditorPosID;
                field.indexType = item.IndexType;
                field.needLocal = item.NeedLocal;
                module.fields.Add(field.fieldName, field);
            }
        }

        public void ExportToLua(ExportLuaType exportType)
        {
            
        }
    }
}
