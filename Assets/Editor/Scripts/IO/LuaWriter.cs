using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;


namespace XTools
{
    public class LuaWriter<T> : Writer
    {
        public LuaWriter(string name) : base(name)
        {
            postfix = "lua";
            path = GlobalConfig.OUTPUT_LUA_PATH;
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
                    var index = workSheetInfo.TableSheet.Cells[row.Value, col.Value].StringValue;
                }
                content += $"t[{row.Value}] = {{{builder}}}";
            }

            content += "return t";

            File.WriteAllText(Path.Combine(path, $"{name}.{postfix}"), content);
        }
    }
}
