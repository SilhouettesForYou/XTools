using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;


namespace XTools
{
    public class LuaWriter : Writer
    {
        public LuaWriter(string name) : base(name)
        {
            postfix = "lua";
            path = GlobalConfig.OUTPUT_LUA_PATH;
        }

        public override void SetConfig(WriterConfig config)
        {
            base.SetConfig(config);
        }

        public override void Write(object obj)
        {
            var module = obj as JsonDataModule;
            string content = "local t = {}\n";

            foreach (var row in workSheetInfo.DicId2RowIndex)
            {
                StringBuilder builder = new StringBuilder();
                foreach (var col in workSheetInfo.DicColumnName2Index)  
                {
                    if (module.fields.ContainsKey(col.Key))
                    {
                        // apply config
                        var _config = config as LuaWriterConfig;
                        if (!(_config.FilterEnable(module.fields[col.Key])))
                        {
                            continue;
                        }

                        var fieldTypeName = module.fields[col.Key].fieldTypeName.ToString();
                        var cell = workSheetInfo.TableSheet.Cells[row.Value, col.Value].StringValue;
                        var parser = JsonDataProcesser.Instance().ParserCache[fieldTypeName];
                        parser.Parse(cell, out object res);
                        var element = parser.SerializeLua(res);
                        var splitStr = (col.Value + 1 == workSheetInfo.DicColumnName2Index.Count) ? "" : ", ";
                        builder.Append(_config.FilterElement(element) + splitStr);
                    }
                }
                content += $"t[{row.Value - 1}] = {{ {builder} }}\n";
            }

            content += "return t";

            if (name.Contains(Path.AltDirectorySeparatorChar.ToString()))
            {
                name = name.Split(Path.AltDirectorySeparatorChar)[1];
            }

            File.WriteAllText(Path.Combine(path, $"{name}.{postfix}"), content);
        }
    }
}
