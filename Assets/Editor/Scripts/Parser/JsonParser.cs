using Newtonsoft.Json;
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

    public class JsonDataStash : SingleBase<JsonDataStash>
    {
        public string mainTableName;
        public TableLocation[] tableLocations;
        public TableFieldInfo[] fields;

        public void Stash(string name, TableLocation[] locations, TableFieldInfo[] _fields)
        {
            mainTableName = name;
            tableLocations = locations;
            fields = _fields;
        }
    }

    public class Parser : SingleBase<Parser>
    {
        public static FieldTypes ParseTypeFromString(string stringType, out int size)
        {
            size = 0;


            return FieldTypes.NONE;
        }
    }
}
