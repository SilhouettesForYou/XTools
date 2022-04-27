using Newtonsoft.Json;
using Serializer;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEditor;
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
            IEnumerator _enum = null;
            switch (exportType)
            {
                case ExportLuaType.SERVER_WITH_INDEX:
                    _enum = _ExportToLua(ExportTarget.Server, KeyOrIndex.Index);
                    break;
                case ExportLuaType.CLIENT_WITH_INDEX:
                    _enum = _ExportToLua(ExportTarget.Client, KeyOrIndex.Index);
                    break;
                case ExportLuaType.SERVER_WITH_KEY:
                    _enum = _ExportToLua(ExportTarget.Server, KeyOrIndex.Key);
                    break;
                case ExportLuaType.CLIENT_WITH_KEY:
                    _enum = _ExportToLua(ExportTarget.Client, KeyOrIndex.Key);
                    break;
            }
            if (_enum != null && _enum.MoveNext())
            {
                return;
            }
        }


        private IEnumerator _ExportToLua(ExportTarget target, KeyOrIndex key)
        {
            var asset = AssetDatabase.LoadAssetAtPath<JsonDataModuleList>(GlobalConfig.JSON_ASSET_NAME);
            if (asset == null)
            {
                yield return null;
            }

            foreach (var item in asset.jsonList)
            {
                try
                {
                    foreach (var name in item.tableLocations)
                    {
                        Writer writer = new LuaWriter(name.Replace(".csv", ""));
                        writer.SetConfig(new LuaWriterConfig(target, key));
                        writer.Write(item);
                    }
                }
                catch(Exception e)
                {
                    Debug.Log($"<color=#F24A72>Export Error: {item.mainTableName}</color>");
                }
                
            }
            yield return null;
        }
    }
}
