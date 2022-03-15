using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace XTools
{
    public abstract class WriterConfig
    {
    }

    public class LuaWriterConfig : WriterConfig
    {
        public ExportTarget target { get; private set; }
        public KeyOrIndex key { get; private set; }
        private int index = -1;
        private string name = "";

        public LuaWriterConfig(ExportTarget target, KeyOrIndex key)
        {
            this.target = target;
            this.key = key;
        }

        public bool FilterEnable(FieldBase fieldBase)
        {
            name = fieldBase.fieldName;
            switch (target)
            {
                case ExportTarget.Server:
                    index = fieldBase.serverPosId;
                    return fieldBase.forServer;
                case ExportTarget.Client:
                    index = fieldBase.clientPosId;
                    return fieldBase.forClient;
            }
            return true;
        }

        public string FilterElement(string src)
        {
            switch (key)
            {
                case KeyOrIndex.Key:
                    return string.IsNullOrWhiteSpace(name) ? src : $"{name} = {src}";
                case KeyOrIndex.Index:
                    return index == -1 ? src : $"[{index}] = {src}";
            }

            return src;
        }
    }
}
